/*
* Copyright (c) 2014 SirSengir
* Starliners (http://github.com/SirSengir/Starliners)
*
* This file is part of Starliners.
*
* Starliners is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* Starliners is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with Starliners.  If not, see <http://www.gnu.org/licenses/>.
*/

﻿using BLibrary.Resources;
using OpenTK.Audio;
using System;
using System.Collections.Generic;
using DragonOgg.MediaPlayer;
using System.Threading;
using System.Collections.Concurrent;
using BLibrary.Util;
using Starliners;

namespace BLibrary.Audio {

    public sealed class SoundManager : IDisposable {
        #region Constants

        const int CHANNEL_COUNT = 16;
        const int CHANNEL_BUFFERS = 32;
        const int CHANNEL_BUFFERSIZE = 4096;

        #endregion

        public static SoundManager Instance {
            get;
            set;
        }

        #region Fields

        bool _isRunning;
        bool _disabled;
        Thread _updateAudio;
        AudioContext _context;
        SoundChannel[] _channels;
        OggPlayer _player;
        OggPlaylist _playlist;
        Dictionary<string, IClipContainer> _clips = new Dictionary<string, IClipContainer> ();

        List<string> _completedTasks = new List<string> ();
        ConcurrentDictionary<string, SoundTask> _currentTasks = new ConcurrentDictionary<string, SoundTask> ();
        ConcurrentQueue<SoundTask> _taskLaunch = new ConcurrentQueue<SoundTask> ();

        #endregion

        public SoundManager () {
            _disabled = !GameAccess.Settings.Get<bool> ("sound", "effects");
            if (_disabled) {
                return;
            }
            _isRunning = true;

            // Register sound clips
            foreach (SoundDefinition sound in GameAccess.Game.Sounds) {
                RegisterSound (sound);
            }

            // Setup audio channels
            SetupChannels ();

            // Configure music player
            _player = new OggPlayerFBN ();
            _playlist = new OggPlaylist ();

            if (GameAccess.Settings.Get<bool> ("sound", "music")) {
                int count = 0;
                foreach (ResourceFile resource in GameAccess.Resources.Search ("Resources.Music")) {
                    _playlist.Add (new OggPlaylistFile (new OggFile (new ResourceOggFile (new AudioResourceFile (resource))), count));
                    count++;
                }

                _player.PlayerMessage += HandlePlayerMessage;
                NextTrack ();
            }
        }

        #region IDisposable

        ~SoundManager ()
        {
            Dispose (false);
        }

        public void Dispose () {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        bool _disposed = false;

        void Dispose (bool manual) {
            if (_disposed) {
                return;
            }

            if (manual) {
                for (int i = 0; i < _channels.Length; i++) {
                    _channels [i].Dispose ();
                }
            } else {
                Console.Out.WriteLine ("Warning: SoundManager leaked!");
            }
        }

        #endregion

        #region Channel managment

        void SetupChannels () {
            _updateAudio = new Thread (UpdateChannels);
            _updateAudio.IsBackground = true;
            _updateAudio.Start ();
        }

        void UpdateChannels () {
            // Init the context
            _context = new AudioContext ();
            GameAccess.Interface.GameConsole.Audio ("Created Audio Context {0}.", _context.ToString ());

            // Setup the sound channels.
            _channels = new SoundChannel[CHANNEL_COUNT];
            for (int i = 0; i < _channels.Length; i++) {
                _channels [i] = new SoundChannel (CHANNEL_BUFFERS, CHANNEL_BUFFERSIZE);
            }

            while (_isRunning) {
                // Launch at least one sound task per update if any.
                if (_taskLaunch.Count > 0) {
                    SoundTask task = null;
                    _taskLaunch.TryDequeue (out task);
                    if (task != null) {
                        StartTask (task);
                    }
                }

                // Update all channels.
                for (int i = 0; i < _channels.Length; i++) {
                    _channels [i].Update ();
                }

                _completedTasks.Clear ();
                foreach (var entry in _currentTasks) {
                    if (entry.Value.IsCompleted) {
                        _completedTasks.Add (entry.Key);
                    }
                }

                for (int i = 0; i < _completedTasks.Count; i++) {
                    SoundTask task;
                    _currentTasks.TryRemove (_completedTasks [i], out task);
                }

                // Pause for a bit.
                Thread.Sleep (1);
            }
        }

        void StartTask (SoundTask task) {
            for (int i = 0; i < _channels.Length; i++) {
                if (!_channels [i].IsAvailable) {
                    continue;
                }

                _channels [i].Start (task);
                break;
            }

            // Tasks which can't find a free channel are silently discarded to prevent backlog.
        }

        #endregion

        #region Music track control

        void NextTrack () {
            _playlist.GetNextFile ();
            if (_playlist.CurrentFile == null) {
                return;
            }
            _player.SetCurrentFile (_playlist.CurrentFile.File);
            _player.Play ();
        }

        void HandlePlayerMessage (object sender, OggPlayerMessageArgs e) {
            if (e.Message != OggPlayerMessageType.PlaybackEndOfFile) {
                return;
            }
            NextTrack ();
        }

        #endregion

        #region Audio clips

        /// <summary>
        /// Registers a sound clip with the given ident and the given resource pattern.
        /// </summary>
        /// <param name="ident"></param>
        /// <param name="name"></param>
        public void RegisterSound (SoundDefinition sound) {

            if (!sound.IsCollection) {
                ResourceFile resource = GameAccess.Resources.SearchResource (sound.Pattern);
                if (resource == null) {
                    GameAccess.Interface.GameConsole.Warning ("Could not load sound {0} since no matching resource file was found.", sound.Ident);
                    return;
                }

                _clips [sound.Ident] = new SoundClip (resource.OpenRead ()) { BaseGain = sound.BaseGain };

            } else {
                List<SoundClip> clips = new List<SoundClip> ();
                foreach (ResourceFile resource in GameAccess.Resources.Search(sound.Pattern)) {
                    clips.Add (new SoundClip (resource.OpenRead ()) { BaseGain = sound.BaseGain });
                }
                if (clips.Count <= 0) {
                    GameAccess.Interface.GameConsole.Warning ("Could not load sound collection {0} since no matching resource files were found.", sound.Ident);
                    return;
                }

                _clips [sound.Ident] = new SoundCollection (clips) { Randomized = sound.IsRandomized };
            }
        }

        /// <summary>
        /// Plays the given sound clip a single time.
        /// </summary>
        /// <param name="ident"></param>
        public void Play (string ident) {
            if (_disabled) {
                return;
            }

            if (!_clips.ContainsKey (ident)) {
                GameAccess.Interface.GameConsole.Audio ("Unable to play a sound with the id '{0}', since it has not been registered.", ident);
                return;
            }

            QueueTask (new SoundTask (ident, _clips [ident].Clip));
        }

        /// <summary>
        /// Starts the given sound clip in a loop.
        /// </summary>
        /// <param name="ident"></param>
        public void Start (string ident) {
            if (_disabled) {
                return;
            }

            if (_currentTasks.ContainsKey (ident) && _currentTasks [ident].IsLooped) {
                return;
            }

            if (!_clips.ContainsKey (ident)) {
                return;
            }

            QueueTask (new SoundTask (ident, _clips [ident].Clip) { IsLooped = true });
        }

        /// <summary>
        /// Stops playback/loop of the given soundclip.
        /// </summary>
        /// <param name="ident"></param>
        public void Stop (string ident) {
            if (_currentTasks.ContainsKey (ident)) {
                _currentTasks [ident].MustFade = true;
            }
        }

        public void ChangeVolume (string ident, float volume) {
            if (_currentTasks.ContainsKey (ident)) {
                _currentTasks [ident].Volume = volume;
            }
        }

        void QueueTask (SoundTask task) {
            _taskLaunch.Enqueue (task);
            _currentTasks [task.Ident] = task;
        }

        #endregion

        public void Stop () {
            if (_player != null) {
                _player.Stop ();
            }
            _isRunning = false;
        }
    }
}
