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
using BLibrary.Serialization;
using BLibrary;
using BLibrary.Util;
using System.Linq;
using BLibrary.Saves;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace Starliners.Game {

    [Serializable]
    public sealed class WorldData : IWorldEditor, ISerializable {

        #region Constants

        public static readonly Vect2i CHUNK_SIZE = new Vect2i (16, 16);

        #endregion

        #region Properties

        public string Name {
            get;
            private set;
        }

        public string File {
            get;
            private set;
        }

        public Random Seed {
            get;
            private set;
        }

        public Random Rand {
            get;
            private set;
        }

        public bool CoinToss {
            get {
                return Rand.NextDouble () < 0.5;
            }
        }

        public GameClock Clock {
            get;
            private set;
        }

        public GameConsole GameConsole {
            get;
            private set;
        }

        public SerializationComposer SerializationHelper {
            get;
            private set;
        }

        public ulong LastSerial {
            get;
            set;
        }

        public WorldHolder Controller {
            get;
            private set;
        }

        #endregion

        #region Fields

        MetaContainer _parameters = new MetaContainer ();

        Dictionary<ulong, Player> _players = new Dictionary<ulong, Player> ();
        Dictionary<ulong, Asset> _assets = new Dictionary<ulong, Asset> ();
        Dictionary<ulong, Entity> _entities = new Dictionary<ulong, Entity> ();
        Dictionary<ulong, StateObject> _states = new Dictionary<ulong, StateObject> ();
        List<Particle> _particles = new List<Particle> ();

        #endregion

        #region Constructor

        public WorldData (WorldHolder simulator, GameConsole console) {

            Controller = simulator;
            GameConsole = console;

            Rand = new Random ();

            SerializationHelper = new SerializationComposer (this);

        }

        public WorldData (WorldHolder simulator, GameConsole console, MetaContainer parameters) {

            Controller = simulator;
            _parameters = parameters;
            GameConsole = console;

            Name = parameters.Get<string> (ParameterKeys.NAME);
            File = SaveUtils.CreateSaveName (GameAccess.Folders [Constants.PATH_SAVES].Location, Constants.SAVE_SUFFIX, Name);

            Seed = new Random ();
            Rand = new Random ();

            Clock = new GameClock ();

            LastSerial = 1000;
            SerializationHelper = new SerializationComposer (this);

        }

        #endregion

        #region Serialization

        public WorldData (SerializationInfo info, StreamingContext context) {

            Rand = new Random ();

            WorldData referred = (WorldData)context.Context;

            referred._parameters = (MetaContainer)info.GetValue ("Parameters", typeof(MetaContainer));
            referred.Name = info.GetString ("Name");
            referred.File = info.GetString ("File");
            referred.Seed = (Random)info.GetValue ("Seed", typeof(Random));
            referred.Clock = (GameClock)info.GetValue ("GameClock", typeof(GameClock));

            referred.GameConsole.Serialization ("Deserializing {0} players.", info.GetInt32 ("Player.Count"));
            for (int i = 0; i < info.GetInt32 ("Player.Count"); i++) {
                Player content = (Player)info.GetValue ("Player." + i, typeof(Player));
                referred.AddPlayer (info.GetUInt64 ("Player." + i + ".Id"), content);
                referred.GameConsole.Serialization ("Deserialized player {0}.", content.ToString ());
            }

            referred.GameConsole.Serialization ("Deserializing {0} entities.", info.GetInt32 ("Entity.Count"));
            for (int i = 0; i < info.GetInt32 ("Entity.Count"); i++) {
                Entity content = (Entity)info.GetValue ("Entity." + i, typeof(Entity));
                referred.AddEntity (info.GetUInt64 ("Entity." + i + ".Id"), content);
                referred.GameConsole.Serialization ("Deserialized entity {0}.", content.ToString ());
            }

            referred.GameConsole.Serialization ("Deserializing {0} states.", info.GetInt32 ("States.Count"));
            for (int i = 0; i < info.GetInt32 ("States.Count"); i++) {
                StateObject content = (StateObject)info.GetValue ("States." + i, typeof(StateObject));
                referred.AddState (info.GetUInt64 ("States." + i + ".Id"), content);
                referred.GameConsole.Serialization ("Deserialized state {0}.", content.ToString ());
            }

        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {

            GameConsole.Serialization ("Serializing world '{0}'...", Name);

            info.AddValue ("Parameters", _parameters, typeof(MetaContainer));
            info.AddValue ("Name", Name);
            info.AddValue ("File", File);
            info.AddValue ("Seed", Seed);
            info.AddValue ("GameClock", Clock);

            GameConsole.Serialization ("Serializing {0} players...", _players.Count);
            info.AddValue ("Player.Count", _players.Count);
            Player[] players = Enumerable.ToArray (_players.Values); 
            for (int i = 0; i < _players.Count; i++) {
                Player player = players [i];
                info.AddValue ("Player." + i + ".Id", player.Serial);
                info.AddValue ("Player." + i, player);
            }

            GameConsole.Serialization ("Serializing {0} entities...", _entities.Count);
            info.AddValue ("Entity.Count", _entities.Count);
            Entity[] entities = Enumerable.ToArray (_entities.Values); 
            for (int i = 0; i < _entities.Count; i++) {
                Entity entity = entities [i];
                info.AddValue ("Entity." + i + ".Id", entity.Serial);
                info.AddValue ("Entity." + i, entity);
            }

            GameConsole.Serialization ("Serializing {0} states...", _states.Count);
            info.AddValue ("States.Count", _states.Count);
            StateObject[] states = Enumerable.ToArray (_states.Values); 
            for (int i = 0; i < _states.Count; i++) {
                StateObject state = states [i];
                info.AddValue ("States." + i + ".Id", state.Serial);
                info.AddValue ("States." + i, state);
            }
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Called when assets and states are loaded. Map generation has not yet run on new maps.
        /// </summary>
        public void OnStatesLoaded () {
            foreach (StateObject state in States.Values) {
                state.OnStatesLoaded ();
            }
        }

        /// <summary>
        /// Called when the creation of the world or the loading of a savegame is complete.
        /// </summary>
        /// <remarks> Allows StateObjects to run remaining initialization where they can assume all other objects to exist and asset init to be complete. Only called by the simulator.</remarks>
        public void OnWorldLoaded () {
        }

        #endregion

        public ulong GetNextSerial () {
            LastSerial++;
            return LastSerial;
        }

        public IIdIdentifiable RequireIDObject (ulong serial) {
            if (_assets.ContainsKey (serial)) {
                return _assets [serial];
            }
            if (_states.ContainsKey (serial)) {
                return _states [serial];
            }
            if (_players.ContainsKey (serial)) {
                return _players [serial];
            }
            if (_entities.ContainsKey (serial)) {
                return _entities [serial];
            }

            throw new SystemException ("Out of synch! Required non-existent idobject with serial: " + serial);
        }

        Vect2i GetChunkCoordinates (Vect2f coordinates) {
            return new Vect2i (coordinates.X / CHUNK_SIZE.X, coordinates.Y / CHUNK_SIZE.Y);
        }

        public void Log (string level, string message, params object[] args) {
            GameConsole.Log (level, message, args);
        }

        public T GetParameter<T> (string key) {
            return _parameters.Get<T> (key);
        }

        #region Players

        public IReadOnlyDictionary<ulong, Player> Players {
            get {
                return _players;
            }
        }

        public void AddPlayer (Player player) {
            AddPlayer (player.Serial, player);
        }

        void AddPlayer (ulong serial, Player player) {
            _players [serial] = player;
        }

        public Player RequirePlayer (ulong serial) {
            if (!_players.ContainsKey (serial)) {
                throw new ArgumentOutOfRangeException ("Out of synch! Tried to fetch invalid player id: " + serial);
            }
            return _players [serial];
        }

        #endregion

        #region Assets

        public IReadOnlyDictionary<ulong, Asset> Assets {
            get {
                return _assets;
            }
        }

        public void AddAsset (Asset asset) {
            _assets [asset.Serial] = asset;
            asset.OnAddition ();
        }

        public Asset RequireAsset (ulong serial) {
            if (_assets.ContainsKey (serial)) {
                return _assets [serial];
            }
            throw new SystemException ("Out of synch! Tried to fetch invalid asset id: " + serial);
        }

        public T RequireAsset<T> (ulong serial) where T : Asset {
            return (T)RequireAsset (serial);
        }

        #endregion

        #region Entities

        public IReadOnlyDictionary<ulong, Entity> Entities {
            get {
                return _entities;
            }
        }

        public void AddEntity (Entity entity) {
            AddEntity (entity.Serial, entity);
        }

        void AddEntity (ulong serial, Entity entity) {
            _entities [serial] = entity;
            entity.OnPlaced ();
        }

        public void RemoveEntity (Entity entity) {
            _entities.Remove (entity.Serial);

            // Ensure that selected entities are deselected.
            foreach (Player player in _players.Values) {
                if (player.SelectedEntity != null && player.SelectedEntity.Serial == entity.Serial) {
                    player.SelectedEntity = null;
                }
            }
        }

        public Entity GetEntity (ulong serial) {
            return _entities.ContainsKey (serial) ? _entities [serial] : null;
        }

        public Entity RequireEntity (ulong serial) {
            if (_entities.ContainsKey (serial)) {
                return _entities [serial];
            }
            throw new SystemException ("Out of synch! Tried to fetch invalid entity id: " + serial);
        }

        public T RequireEntity<T> (ulong serial) where T : Entity {
            return (T)RequireEntity (serial);
        }

        public IList<Entity> GetEntitiesWithin (BLibrary.Util.Vect2f coordinates, int radius) {
            return Entities.Values.Where (p => MathUtils.GetDistanceBetween (coordinates.X, coordinates.Y, p.Location.X, p.Location.Y) <= radius).ToList ();
        }

        #endregion

        #region States

        public IReadOnlyDictionary<ulong, StateObject> States {
            get {
                return _states;
            }
        }

        public void AddState (StateObject state) {
            AddState (state.Serial, state);
        }

        void AddState (ulong serial, StateObject state) {
            _states [serial] = state;
        }

        public void RemoveState (StateObject state) {
            _states.Remove (state.Serial);
        }

        public StateObject GetState (ulong uid) {
            if (_states.ContainsKey (uid)) {
                return _states [uid];
            }
            return null;
        }

        public StateObject RequireState (ulong uid) {
            if (_states.ContainsKey (uid)) {
                return _states [uid];
            }
            throw new SystemException ("Out of synch! Tried to fetch invalid state id: " + uid);
        }

        public T RequireState<T> (ulong uid) where T : StateObject {
            return (T)_states [uid];
        }

        #endregion

        public IReadOnlyList<Particle> Particles {
            get {
                return _particles;
            }
        }

        public void AddParticle (Particle particle) {
            _particles.Add (particle);
        }

        public void RemoveParticle (Particle particle) {
            _particles.Remove (particle);
        }
    }
}

