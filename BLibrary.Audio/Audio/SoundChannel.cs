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

﻿using System;
using OpenTK.Audio.OpenAL;
using csvorbis;
using Starliners;

namespace BLibrary.Audio {

    sealed class SoundChannel : IDisposable {
        #region Constants

        const int BIGENDIAN_READMODE = 0;
        const int WORD_READMODE = 2;
        const int SGNED_READMODE = 1;

        #endregion

        public bool IsAvailable {
            get { return _currentTask == null; }
        }

        SoundTask _currentTask;
        VorbisFileInstance _playedClip;
        ALFormat _currentFormat;
        int _currentRate;
        int _alSourceId;
        int[] _buffers;
        byte[] _bytebuf;
        bool _finished;

        public SoundChannel (int bufferCount, int bufferSize) {
            _alSourceId = AL.GenSource ();
            _buffers = AL.GenBuffers (bufferCount);
            _bytebuf = new byte[bufferSize];
        }

        #region IDisposable

        ~SoundChannel () {
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
                AL.DeleteSource (_alSourceId);
                AL.DeleteBuffers (_buffers);
            } else {
                Console.Out.WriteLine ("Warning: SoundChannel leaked!");
            }
        }

        #endregion

        public void Start (SoundTask task) {
            CleanupBuffers ();
            _currentTask = task;
            StartClip ();
        }

        void StartClip () {
            _playedClip = _currentTask.Clip.makeInstance ();
            _finished = false;

            Info info = _playedClip.vorbisFile.getInfo () [0];
            _currentFormat = info.channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
            _currentRate = info.rate;

            // Read initial audio
            int buffersUsed = 0;
            for (int i = 0; i < _buffers.Length; i++) {

                int readBytes = ReadFromClip ();
                if (readBytes > 0) {
                    // Copy buffer data to the AL resource.
                    AL.BufferData (_buffers [i], _currentFormat, _bytebuf, readBytes,
                        _currentRate);

                    buffersUsed++;
                } else if (readBytes == 0) {
                    OnDataEnd ();
                    break;
                }
            }

            SetProperties (_currentTask);
            AL.SourceQueueBuffers (_alSourceId, buffersUsed, _buffers);
            AL.SourcePlay (_alSourceId);

        }

        void StopOrLoop () {
            if (_currentTask.IsLooped) {
                AL.SourceStop (_alSourceId);
                CleanupBuffers ();
                StartClip ();
            } else {
                FreeChannel ();
            }
        }

        void FreeChannel () {
            AL.SourceStop (_alSourceId);
            _currentTask.IsCompleted = true;
            _currentTask = null;
            _playedClip = null;
            CleanupBuffers ();
        }

        public void Update () {
            if (_playedClip == null) {
                return;
            }

            if (ApplyEffects (_currentTask)) {
                FreeChannel ();
                return;
            }

            SetProperties (_currentTask);

            int buffersQueued;
            AL.GetSource (_alSourceId, ALGetSourcei.BuffersQueued, out buffersQueued);

            int processedBuffers;
            AL.GetSource (_alSourceId, ALGetSourcei.BuffersProcessed, out processedBuffers);

            if (_finished) {
                if (buffersQueued <= processedBuffers) {
                    StopOrLoop ();
                }
                return;
            }

            if (buffersQueued - processedBuffers > 0 && AL.GetError () == ALError.NoError) {
                if (AL.GetSourceState (_alSourceId) != ALSourceState.Playing) {
                    StopOrLoop ();
                    return;
                }
            }

            for (int i = 0; i < processedBuffers; i++) {
                int removedBuffer = 0;
                AL.SourceUnqueueBuffers (_alSourceId, 1, ref removedBuffer);

                if (_finished) {
                    continue;
                }

                int readBytes = ReadFromClip ();
                if (readBytes > 0) {
                    AL.BufferData (removedBuffer, _currentFormat, _bytebuf, readBytes,
                        _currentRate);
                    AL.SourceQueueBuffer (_alSourceId, removedBuffer);
                } else if (readBytes == 0) {
                    OnDataEnd ();
                }

                ALError error = AL.GetError ();
                if (error != ALError.NoError) {
                    StopOrLoop ();
                    GameAccess.Interface.GameConsole.Warning ("An error occured while playing audio: " + error.ToString ());
                    break;
                }
            }

        }

        bool ApplyEffects (SoundTask task) {
            if (task.MustFade) {
                task.Volume -= 1f / 255;
                if (task.Volume <= 0) {
                    return true;
                }
            }

            return false;
        }

        void SetProperties (SoundTask task) {
            AL.Source (_alSourceId, ALSourcef.Gain, task.Volume);
            task.PropertiesChanged = false;
        }

        void CleanupBuffers () {
            int processedBuffers;
            AL.GetSource (_alSourceId, ALGetSourcei.BuffersProcessed, out processedBuffers);

            int[] removedBuffers = new int[processedBuffers];
            AL.SourceUnqueueBuffers (_alSourceId, processedBuffers, removedBuffers);
        }

        int ReadFromClip () {
            return _playedClip.read (_bytebuf, _bytebuf.Length,
                BIGENDIAN_READMODE, WORD_READMODE, SGNED_READMODE, null);
        }

        void OnDataEnd () {
            _finished = true;
        }
    }
}

