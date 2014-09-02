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
using System.Collections.Generic;
using BLibrary.Util;
using BLibrary.Graphics;
using BLibrary;
using BLibrary.Graphics.Sprites;
using Starliners.Game;
using BLibrary.Audio;
using Starliners.States;
using Starliners.Map;

namespace Starliners {

    sealed class WorldInterface : WorldHolder {
        #region Instance

        public static WorldInterface Instance {
            get;
            set;
        }

        #endregion

        #region WorldHolder

        public override bool IsSimulating {
            get {
                return false;
            }
        }

        public override GameConsole GameConsole {
            get {
                return GameAccess.Interface.GameConsole;
            }
        }

        public bool Heartbeat {
            get;
            set;
        }

        public bool HasGameData {
            get;
            set;
        }

        public bool IsRunning {
            get;
            set;
        }

        /// <summary>
        /// The client side player.
        /// </summary>
        public Player ThePlayer {
            get;
            set;
        }

        #endregion

        #region Constructor

        public WorldInterface () {
            Access = new WorldData (this, GameConsole);
        }

        #endregion

        #region Fake world ticks

        SoundScenery _sounds = new SoundScenery ();
        LinkedList<Particle> _deadParticles = new LinkedList<Particle> ();

        public void MockTick () {

            HandleInsanity ();
            if (!Heartbeat) {
                return;
            }
            Heartbeat = false;

            // Clock
            Access.Clock.Advance ();

            // Entities
            foreach (Entity entity in Access.Entities.Values) {
                if (entity.IsTickable) {
                    entity.RenderTick ();
                }
            }
            // States
            foreach (StateObject state in Access.States.Values) {
                if (state.IsTickable) {
                    state.RenderTick ();
                }
            }

            _sounds.Clear ();

            for (int i = 0; i < Access.Particles.Count; i++) {
                if (!Access.Particles [i].HasSound || !MapState.Instance.Map.DrawnArea.IsWithinRenderedArea (Access.Particles [i].Location)) {
                    continue;
                }
                if (_sounds.IsMaxed (Access.Particles [i].Sound, 0.75f)) {
                    continue;
                }

                float distance = MathUtils.GetDistanceBetween (GameAccess.Interface.ThePlayer.Location, Access.Particles [i].Location);
                if (distance > 5f && distance < -5f) {
                    continue;
                }

                float volume = 0.3f * (1f / distance);
                _sounds.Add (Access.Particles [i].Sound, volume < 0.1f ? volume : 0.1f, 0.75f);
            }
            _sounds.Update ();

        }

        public void OnFrameStart (double elapsedTime) {

            // Particles
            _deadParticles.Clear ();
            foreach (Particle particle in Access.Particles) {
                particle.Update (elapsedTime);
                if (particle.Age > particle.MaxAge)
                    _deadParticles.AddLast (particle);
            }

            foreach (Particle particle in _deadParticles) {
                Access.RemoveParticle (particle);
            }

        }

        public void Halt () {
            _sounds.Clear ();
            _sounds.Update ();
        }

        #endregion

        #region Map

        public override void SignalMapDirtied (Vect2d location, string reason) {
            MapState.Instance.Map.MarkDirty (location, reason);
        }

        #endregion

        #region Synching

        public void PrepareDisconnect () {
        }

        public void SynchPlayer (Player player) {
            GameConsole.Debug ("Adding player '{0}' ({1}, {2}).", player.Name, player.Serial, player.GetType ());
            Access.AddPlayer (player);
        }

        public void SynchAsset (Asset asset) {
            GameConsole.Debug ("Adding asset '{0}' ({1}, {2}).", asset.Name, asset.Serial, asset.GetType ());
            Access.AddAsset (asset);
        }

        public void SynchState (StateObject state) {
            GameConsole.Debug ("Adding state '{0}' ({1}, {2}).", state.Name, state.Serial, state.GetType ());
            if (state is ISpriteDeclarant) {
                state.MakeSane ();
                ((ISpriteDeclarant)state).RegisterIcons (SpriteManager.Instance);
            }
            Access.AddState (state);
        }

        public void SynchEntity (Entity entity) {
            GameConsole.Debug ("Adding entity '{0}' ({1}, {2}).", entity.Name, entity.Serial, entity.GetType ());
            if (entity is ISpriteDeclarant) {
                entity.MakeSane ();
                ((ISpriteDeclarant)entity).RegisterIcons (SpriteManager.Instance);
            }
            Access.AddEntity (entity);
            MapState.Instance.Map.MarkDirty (entity.Location, string.Format ("Entity was added: {0}", entity.ToString ()));
        }

        public void RemoveEntity (Entity entity) {
            if (entity == null) {
                return;
            }

            Access.RemoveEntity (entity);
            entity.IsDead = true;
            MapRendering.Instance.OnRenderableRemoved (entity);
            GameConsole.Debug ("Removed entity '{0}' ({1}, {2}).", entity.Name, entity.Serial, entity.GetType ());
            MapState.Instance.Map.MarkDirty (entity.Location, string.Format ("Entity was removed: {0}", entity.ToString ()));
        }

        public void RemoveState (StateObject state) {
            Access.RemoveState (state);
            state.IsDead = true;
            GameConsole.Debug ("Removed state '{0}' ({1}, {2}).", state.Name, state.Serial, state.GetType ());
        }

        #endregion
    }
}
