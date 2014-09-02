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

﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using BLibrary;
using BLibrary.Gui.Data;
using BLibrary.Network;
using BLibrary.Saves;
using BLibrary.Serialization;
using BLibrary.Util;
using Starliners.Game.Forces;
using Starliners.Network;

namespace Starliners.Game {

    public sealed class WorldSimulator : WorldHolder {

        #region Properties

        public override bool IsSimulating {
            get { return true; }
        }

        public override GameConsole GameConsole {
            get { return GameAccess.Simulator.GameConsole; }
        }

        public IPopulator Populator {
            get;
            private set;
        }

        #endregion

        #region Fields

        Dictionary<ulong, NetInterfaceServer> _netInterfaces = new Dictionary<ulong, NetInterfaceServer> ();
        // Determines the max frequency of game ticks.
        Timer _gameTimer;
        // Determines the max frequency of gui updated.
        Timer _guiTimer;

        ConcurrentQueue<Entity> _addedEntities = new ConcurrentQueue<Entity> ();
        ConcurrentQueue<Entity> _removedEntities = new ConcurrentQueue<Entity> ();

        ConcurrentQueue<StateObject> _addedStates = new ConcurrentQueue<StateObject> ();
        ConcurrentQueue<StateObject> _removedStates = new ConcurrentQueue<StateObject> ();

        bool _simulationPaused;

        #endregion

        #region Constructor

        public WorldSimulator (MetaContainer parameters, IScenarioProvider scenario) {
            InitSimulator ();
            CreateNewWorld (parameters, scenario);
        }

        public WorldSimulator (FileInfo save, IFormatter formatter) {
            InitSimulator ();
            Load (save, formatter);
        }

        void InitSimulator () {
            _gameTimer = new Timer (Constants.TICKS_HEARTBEAT);
            _guiTimer = new Timer (Constants.TICKS_HEARTBEAT * 3);
        }

        #endregion

        #region Game Initialization & Loading

        LinkedList<ulong> _setupOrder = new LinkedList<ulong> ();

        void SetupAssets () {
            GameConsole.Info ("======== Asset Setup ========");
            _setupOrder.Clear ();
            IEnumerable<Asset> assets = Populator.GetAssets (Access);
            GameConsole.Info ("---------- Transfering Assets ----------");
            foreach (Asset asset in assets) {
                GameConsole.Debug ("Adding asset '{0}' ({1}, {2}) to the world.", asset.Name, asset.GetType (), asset.Serial);
                Access.AddAsset (asset);
                _setupOrder.AddLast (asset.Serial);
            }
            GameConsole.Info ("======== End Asset Setup ========");

        }

        void CreateNewWorld (MetaContainer parameters, IScenarioProvider scenario) {
            GameConsole.Info ("======== Creating A New Game ========");
            GameConsole.Info ("---------- Game Parameters ----------");
            foreach (string line in parameters.GetInfo()) {
                GameConsole.Info (line);
            }

            GameConsole.Info ("---------- Creating World ----------");
            Populator = scenario.CreatePopulator ();
            Access = new WorldData (this, GameConsole, parameters);

            SetupAssets ();

            // Add initial states
            GameConsole.Info ("======== StateObject Init ========");
            foreach (StateObject state in Populator.CreateInitialStates(Access)) {
                GameConsole.Debug ("Adding state object '{0}' ({1}, {2}) to the world.", state.Name, state.GetType (), state.Serial);
                Access.AddState (state);
            }
            GameConsole.Info ("======== End StateObject Init ========");
            Access.OnStatesLoaded ();

            GameConsole.Info ("======== Start Map Generation ========");
            // Create a new map
            Populator.MapGenerator.CreateMap (Access);
            GameConsole.Info ("======== End Map Generation ========");

            GameAccess.Game.InitWorld (Access);
            Access.OnWorldLoaded ();
            GameAccess.Game.OnWorldLoad (Access);
            GameConsole.Info ("======== End World Creation ========");
            GameConsole.Info ("====================================");
        }

        void Load (FileInfo save, IFormatter formatter) {

            GameConsole.Info ("========================================");
            GameConsole.Info ("Loading world from {0}.", save.FullName);

            using (Package packed = Package.Open (save.FullName, FileMode.Open, FileAccess.Read)) {

                // Retrieve the save header.
                SaveHeader header = (SaveHeader)formatter.Deserialize (ZipUtils.GetStream (packed, "header", formatter));
                // Create a save mapper.
                formatter.Binder = GameAccess.Game.GetSaveMapper (header, GameConsole);

                // Retrieve the populator
                GameConsole.Debug ("Deserializing the populator...");
                Populator = (IPopulator)formatter.Deserialize (ZipUtils.GetStream (packed, "populator", formatter));

                // Create a fresh WorldData object and setup its assets.
                Access = new WorldData (this, GameConsole);
                formatter.Context = new StreamingContext (StreamingContextStates.File, Access);
                Access.LastSerial = (ulong)formatter.Deserialize (ZipUtils.GetStream (packed, "lastuid", formatter));
                SetupAssets ();

                // Refresh actual variable game data from the stored WorldData object into the newly created one.
                GameConsole.Debug ("Loading players and entities from save...");

                formatter.Deserialize (ZipUtils.GetStream (packed, "persist", formatter));
            }

            Access.OnStatesLoaded ();
            Access.OnWorldLoaded ();
            GameAccess.Game.OnWorldLoad (Access);
            GameConsole.Info ("======== World '{0}' loaded. ========", Access.Name);

        }

        #endregion

        #region  Saving

        /// <summary>
        /// Initiates an auto save.
        /// </summary>
        public FileInfo AutoSave () {

            FileInfo save = GameAccess.Folders [Constants.PATH_SAVES, Access.File + ".autosave" + Constants.SAVE_SUFFIX];
            Save (save, new BinaryFormatter ());
            return save;
        }

        public bool Save (bool doPause) {
            if (doPause) {
                PauseSimulation ();
            }

            FileInfo save = GameAccess.Folders [Constants.PATH_SAVES, Access.File + Constants.SAVE_SUFFIX];
            GameAccess.Simulator.GameConsole.Info ("Saving " + Access.Name + " to " + save.FullName + "...");

            BinaryFormatter formatter = new BinaryFormatter ();
            Save (save, formatter);
            GameAccess.Simulator.GameConsole.Info ("Saved " + Access.Name + ".");
            if (doPause) {
                UnpauseSimulation ();
            }
            return true;
        }

        /// <summary>
        /// Saves the galaxy.
        /// </summary>
        /// <param name="save"></param>
        /// <param name="formatter"></param>
        /// <returns></returns>
        public bool Save (FileInfo save, IFormatter formatter) {

            // Create header
            SaveHeader header = new SaveHeader (
                                    PlatformUtils.GetEXEVersion (), GameAccess.Resources.GetIdentHash (), Access.Name, Access.File, Access.Clock.Ticks, GameAccess.Resources.GetSavePlugins ());

            // Create the context
            formatter.Context = new StreamingContext (StreamingContextStates.File, Access);
            SaveMapper mapper = GameAccess.Game.GetSaveMapper (header, GameConsole);
            formatter.Binder = mapper;
            GameConsole.Info ("Serializing world to " + save.FullName + ".");

            using (Package packed = Package.Open (save.FullName, FileMode.Create)) {

                // Persistent data
                ZipUtils.SaveToPackage (packed, "header", formatter, header);
                ZipUtils.SaveToPackage (packed, "lastuid", formatter, Access.LastSerial);
                ZipUtils.SaveToPackage (packed, "persist", formatter, Access);
                ZipUtils.SaveToPackage (packed, "populator", formatter, Populator);
                ZipUtils.SaveToPackageAsText (packed, "types.txt", mapper.GetBoundTypeList ());
            }

            GameConsole.Info ("World '" + Access.Name + "' saved.");
            return true;
        }

        #endregion

        #region Main tick loop

        /// <summary>
        /// The good stuff.
        /// </summary>
        /// <returns>The amount of milliseconds until the next tick needs to run.</returns>
        public Timer Tick () {

            _gameTimer.Reset ();

            // Verify player connection states and pause if needed.
            var deadConnections = _netInterfaces.Where (p => p.Value.HasDisconnect).ToDictionary (p => p.Key, p => p.Value);
            foreach (var entry in deadConnections) {
                DisconnectPlayer (Access.RequirePlayer (entry.Key));
            }

            // We only run the simulation if at least one player is online.
            if (_netInterfaces.Count <= 0) {
                PauseSimulation ();
            } else {
                UnpauseSimulation ();
            }

            // Do not actually tick if we are paused.
            // The tick limit is due to queued states and entities.
            if (_simulationPaused && Access.Clock.Ticks > 1001) {
                return _gameTimer;
            }

            HandleInsanity ();

            TickType ticks = Access.Clock.Advance ();
            GameAccess.Game.Tick (Access, ticks);

            if (ticks.HasFlag (TickType.Partial)) {
                SynchCalendarAndWeather ();
            }

            // Autosave
            if (ticks.HasFlag (TickType.Cycle)) {
                PauseSimulation ();
                GameConsole.Info ("Server is autosaving world {0}.", Access.Name);
                FileInfo save = AutoSave ();
                GameConsole.Info ("Saved world {0} to {1}.", Access.Name, save.FullName);
                UnpauseSimulation ();
            }

            // Remove entities which are scheduled for removal
            if (_removedEntities.Count > 0) {
                GameConsole.Debug ("Removing {0} dead entities.", _removedEntities.Count);
                while (_removedEntities.Count > 0) {
                    Entity entity = null;
                    bool didDequeue = _removedEntities.TryDequeue (out entity);
                    if (didDequeue) {
                        RemoveEntity (entity);
                    }
                }
            }
            // Remove states which are scheduled for removal
            if (_removedStates.Count > 0) {
                GameConsole.Debug ("Removing {0} dead states.", _removedStates.Count);
                while (_removedStates.Count > 0) {
                    StateObject state = null;
                    bool didDequeue = _removedStates.TryDequeue (out state);
                    if (didDequeue) {
                        RemoveState (state);
                    }
                }
            }

            // Tick entities
            foreach (Entity entity in Access.Entities.Values) {
                if (!entity.IsDead) {
                    if (entity.IsTickable) {
                        entity.Tick (ticks);
                    }
                } else {
                    Access.Controller.QueueEntityRemoval (entity);
                }
            }

            // Tick tickable assets
            foreach (Asset asset in Access.Assets.Values.Where(p => p.IsTickable)) {
                asset.Tick (ticks);
            }

            // Tick tickable states
            foreach (StateObject state in Access.States.Values) {
                if (!state.IsDead) {
                    if (state.IsTickable) {
                        state.Tick (ticks);
                    }
                } else {
                    Access.Controller.QueueStateRemoval (state);
                }
            }

            // Tick players and synch player state information as needed
            foreach (Player player in Access.Players.Values) {
                player.Tick (ticks);
            }

            // Add entities which were queued
            while (_addedEntities.Count > 0) {
                Entity entity = null;
                bool didDequeue = _addedEntities.TryDequeue (out entity);
                if (didDequeue) {
                    AddEntity (entity, true);
                }
            }

            // Add states which were queued
            while (_addedStates.Count > 0) {
                StateObject state = null;
                bool didDequeue = _addedStates.TryDequeue (out state);
                if (didDequeue) {
                    AddState (state, true);
                }
            }

            // Update entities
            foreach (Entity entity in Access.Entities.Values) {
                if (entity.IsDead || !entity.NeedsUpdate) {
                    continue;
                }
                UpdateIDObject (entity);
            }

            // Update states
            foreach (StateObject state in Access.States.Values) {
                if (state.IsDead || !state.NeedsUpdate) {
                    continue;
                }
                UpdateIDObject (state);
            }

            // Update players
            foreach (Player player in Access.Players.Values) {
                if (!player.NeedsUpdate) {
                    continue;
                }
                UpdateIDObject (player);
            }

            SignalHeartbeat ();

            // Tick gui.
            TickGui (ticks);

            return _gameTimer;
        }

        void SignalHeartbeat () {
            foreach (KeyValuePair<ulong, NetInterfaceServer> entry in _netInterfaces)
                entry.Value.SignalHeartbeat ();

        }

        void PauseSimulation () {
            if (_simulationPaused) {
                return;
            }

            _simulationPaused = true;
            foreach (KeyValuePair<ulong, NetInterfaceServer> entry in _netInterfaces) {
                entry.Value.SignalSimulationPaused ();
            }
            GameConsole.Info ("Paused simulation.");
        }

        void UnpauseSimulation () {
            if (!_simulationPaused) {
                return;
            }

            _simulationPaused = false;
            foreach (KeyValuePair<ulong, NetInterfaceServer> entry in _netInterfaces) {
                entry.Value.SignalSimulationUnpaused ();
            }
            GameConsole.Info ("Unpaused simulation.");
        }

        #endregion

        #region Gui managment

        // Temporary list for containers which are marked for closing.
        List<int> _containerClosed = new List<int> ();
        LinkedList<DataFragment> _synchFields = new LinkedList<DataFragment> ();

        void TickGui (TickType ticks) {

            bool updateTick = !_guiTimer.IsDelayed;
            if (!_guiTimer.IsDelayed) {
                _guiTimer.Reset ();
            }

            // Synch containers as needed.
            foreach (Player player in Access.Players.Values.Where(p => p.IsOnline)) {

                foreach (KeyValuePair<int, Container> entry in player.ContainerManager.Containers.OrderByDescending(p => p.Value.MustClose)) {

                    // Handles containers that have to close because of game events.
                    if (entry.Value.MustClose) {
                        _netInterfaces [player.Serial].CloseGui (entry.Key);
                        _containerClosed.Add (entry.Key);
                        continue;
                    }

                    // Open as yet unopened guis
                    bool changedState = false;
                    if (entry.Value.NeedsOpening) {
                        _netInterfaces [player.Serial].OpenGui (entry.Value.GuiId, entry.Key);
                        entry.Value.NeedsOpening = false;
                        changedState = true;
                    }

                    // Mark containers which needs maximizing for synch
                    if (entry.Value.BringToFront) {
                        _netInterfaces [player.Serial].OpenGui (entry.Value.GuiId, entry.Key);
                        entry.Value.BringToFront = false;
                    }

                    // Container updates are delayed
                    if (changedState || updateTick) {
                        entry.Value.UpdateTick ();
                    }

                    // Synch the container
                    foreach (DataFragment dataField in entry.Value.DataFragments.Values) {
                        if (dataField.IsDirty) {
                            _synchFields.AddLast (dataField);
                            dataField.IsDirty = false;
                        }
                    }
                    if (_synchFields.Count > 0 || changedState) {
                        _netInterfaces [player.Serial].SynchContainerData (entry.Key, entry.Value.NeedsOpening, entry.Value.MustClose, _synchFields);
                    }
                    _synchFields.Clear ();
                }

                // Close containers
                if (_containerClosed.Count > 0) {
                    foreach (int entry in _containerClosed) {
                        ClosedGui (player.Serial, entry);
                    }

                    _containerClosed.Clear ();
                }

            }

        }

        public void ClosedGui (ulong playerId, int windowId) {
            Player player = Access.RequirePlayer (playerId);
            player.ContainerManager.CloseGui (player, windowId);
        }

        #endregion

        void UpdateIDObject (IdObject idobject) {
            while (idobject.HasPendingUpdates) {
                Packet packet = (Packet)idobject.GetUpdatePacket ();
                foreach (NetInterfaceServer netInterface in _netInterfaces.Values) {
                    netInterface.SendPacket (packet);
                }
            }
        }

        #region Entities

        void AddEntity (Entity entity, bool synch) {
            GameConsole.Debug ("Adding entity '{0}'.", entity);
            Access.AddEntity (entity);
            if (synch) {
                SynchIDObject<Entity> (entity);
            }
        }

        void RemoveEntity (Entity entity) {
            GameConsole.Debug ("Removing dead entity '{0}'.", entity);
            Access.RemoveEntity (entity);
            foreach (NetInterfaceServer netInterface in _netInterfaces.Values) {
                netInterface.RemoveIdObject (entity);
            }
        }

        #endregion

        #region States

        void AddState (StateObject state, bool synch) {
            GameConsole.Debug ("Adding state '{0}'.", state);
            Access.AddState (state);
            if (synch) {
                SynchIDObject<StateObject> (state);
            }
        }

        void RemoveState (StateObject state) {
            GameConsole.Debug ("Removing dead states '{0}'.", state);
            Access.RemoveState (state);
            foreach (NetInterfaceServer netInterface in _netInterfaces.Values) {
                netInterface.RemoveIdObject (state);
            }
        }

        #endregion

        #region Players

        public void AddPlayer (Player player) {
            Access.AddPlayer (player);
        }

        /// <summary>
        /// Synchronizes the game setup to the specified player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="netInterface"></param>
        public void JoinPlayer (Player player, NetInterfaceServer netInterface) {
            _netInterfaces [player.Serial] = netInterface;

            // Synch assets in the beginning since WorldData only serializes the mutable part of the world.
            foreach (uint uid in _setupOrder) {
                SynchIDObject<Asset> (Access.RequireAsset (uid));
            }
            // Synch the mutable part of the world in one go to prevent cyclic issues.
            netInterface.SendWorld ((WorldData)Access);

            netInterface.MarkThePlayer (player);
            netInterface.SignalSetupSent ();

            GameConsole.Info ("Player {0} joined the game.", new TextComponent (player.Name));
            player.IsOnline = true;
            SendChat (new TextComposition (new TextComponent ("Player {0} joined the game."), new TextComponent (player.Name)));
        }

        void DisconnectPlayer (Player player) {
            player.IsOnline = false;
            player.ContainerManager.Clear ();
            _netInterfaces.Remove (player.Serial);
            GameConsole.Info ("Player {0} left the game.", new TextComponent (player.Name));
            SendChat (new TextComposition (new TextComponent ("Player {0} left the game."), new TextComponent (player.Name)));
        }

        /// <summary>
        /// Determines whether the given player is currently online.
        /// </summary>
        /// <returns><c>true</c> if this instance is online the specified player; otherwise, <c>false</c>.</returns>
        /// <param name="player">Player.</param>
        public bool IsOnline (Player player) {
            return _netInterfaces.ContainsKey (player.Serial);
        }

        /// <summary>
        /// Synchronizes the currently held object to the player.
        /// </summary>
        /// <param name="player"></param>
        public override void SynchHeld (Player player) {
            _netInterfaces [player.Serial].UpdateHeldObject (player);
        }

        /// <summary>
        /// Updates the held object on the given player, synching it if needed.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="holdable"></param>
        public override void UpdateHeld (Player player, IHoldable holdable) {
            player.HeldObject = holdable;
            SynchHeld (player);
        }

        /// <summary>
        /// Sets the view center on the given player to the given coordinates.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="center"></param>
        public override void SetView (Player player, Vect2f center) {
            player.Location = center;
            _netInterfaces [player.Serial].SendViewSet (center);
        }

        #endregion

        #region Synching

        /// <summary>
        /// Synchronizes the calendar to all connected players.
        /// </summary>
        void SynchCalendarAndWeather () {
            foreach (NetInterfaceServer netInterface in _netInterfaces.Values)
                netInterface.SynchCalendar (Access.Clock);
        }

        /// <summary>
        /// Synchronizes the map to the given player connection.
        /// </summary>
        /// <param name="netInterface"></param>
        void SynchMap (NetInterfaceServer netInterface) {
            /*
            CellData[] map = new CellData[Access.MaxX * Access.MaxY];
            for (int i = Access.MinX; i < Access.MaxX; i++) {
                for (int j = Access.MinY; j < Access.MaxY; j++) {
                    map [(i * Access.MaxX) + j] = Access.Map [i, j];
                }
            }
            netInterface.SendMapUpdate (map);
            */
        }

        /// <summary>
        /// Synchronizes an IDObject to all connected players.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idobject"></param>
        void SynchIDObject<T> (T idobject) where T : IdObject {
            foreach (NetInterfaceServer netInterface in _netInterfaces.Values) {
                netInterface.SynchIDObject (idobject);
            }
        }

        #endregion

        #region Input handling

        public void EntityTargeted (ulong entityUID, Player player, ControlState control) {
            Entity entity = Access.GetEntity (entityUID);
            if (entity != null) {
                GameConsole.Debug ("Player {0} activated entity {1} at {2}.", player.Name, entity.Name, entity.Location);
                player.TargetEntity (entity, control);
            }
        }

        /// <summary>
        /// Handles a entity click by a player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="entityId"></param>
        public void EntityClick (ulong entityUID, Player player, ControlState control) {
            player.ResetTarget ();

            Entity entity = Access.GetEntity (entityUID);
            if (entity != null) {
                GameConsole.Debug ("Player {0} clicked entity {1} at {2}.", player.Name, entity.Name, entity.Location);
                entity.OnClicked (player, control);
                _netInterfaces [player.Serial].MarkSelected (entity);
            }
        }

        public void MapTargeted (Player player, Vect2f coordinates, ControlState control) {
        }

        public void MapClicked (Player player, Vect2f coordinates, ControlState control) {
            player.ResetTarget ();

            if (player.HeldObject != null) {
                UpdateHeld (player, player.HeldObject.OnMapClick (player, coordinates, control));
            }
        }

        #endregion

        /// <summary>
        /// Spawns the given particle in the world and synchs it to all connected players.
        /// </summary>
        /// <param name="particle"></param>
        public override void SpawnParticle (Particle particle) {
            foreach (NetInterfaceServer netInterface in _netInterfaces.Values) {
                netInterface.SendParticle (particle);
            }
        }

        /*
        /// <summary>
        /// Spawns the given text as a particle in the world and synchs it to all connected players.
        /// </summary>
        /// <param name="particle"></param>
        public override void SpawnParticle (string text, Vect2f coordinates) {
            SpawnParticle (new ParticleText (Access, coordinates, text));
        }

        /// <summary>
        /// Spawns the given text and icon as a framed particle and synchs it to all connected players.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="icon">Icon.</param>
        /// <param name="colour">Colour.</param>
        /// <param name="xCoord">X coordinate.</param>
        /// <param name="yCoord">Y coordinate.</param>
        public override void SpawnParticle (string text, string icon, Colour colour, Vect2f coordinates) {
            SpawnParticle (new ParticleIcon (Access, coordinates, text, icon) { Colour = colour });
        }
        */

        public override EntityFleet SpawnFleetEntity (Fleet fleet) {
            EntityFleet entity = Populator.CreateEntityFleet (Access, fleet);
            entity.Location = (Vect2f)fleet.Location;
            QueueEntity (entity);
            return entity;
        }

        public override void SendChat (Player player, TextComposition chat) {
            _netInterfaces [player.Serial].SendChat (chat);
        }

        public void SendChat (TextComposition chat) {
            foreach (NetInterfaceServer netInterface in _netInterfaces.Values) {
                netInterface.SendChat (chat);
            }
        }

        #region Sound

        /// <summary>
        /// Plays a sound for all players at the given coordinates.
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="xCoord"></param>
        /// <param name="yCoord"></param>
        public override void PlaySound (string sound, Vect2f coordinates) {
            foreach (NetInterfaceServer netInterface in _netInterfaces.Values) {
                netInterface.PlaySound (sound, coordinates);
            }
        }

        /// <summary>
        /// Plays the sound for the given player at the given coordinates.
        /// </summary>
        /// <param name="sound">Sound.</param>
        /// <param name="player">Player.</param>
        /// <param name="coordinates">Coordinates.</param>
        public override void PlaySound (string sound, Player player, Vect2f coordinates) {
            if (_netInterfaces.ContainsKey (player.Serial)) {
                _netInterfaces [player.Serial].PlaySound (sound, coordinates);
            }
        }

        /// <summary>
        /// Plays the given sound for the given player.
        /// </summary>
        /// <param name="sound">Sound.</param>
        /// <param name="player">Player.</param>
        public override void PlaySound (string sound, Player player) {
            if (_netInterfaces.ContainsKey (player.Serial)) {
                _netInterfaces [player.Serial].PlaySound (sound);
            }
        }

        #endregion

        #region Entities

        public override void QueueEntity (Entity entity) {
            _addedEntities.Enqueue (entity);
        }

        public override void QueueEntityRemoval (Entity entity) {
            _removedEntities.Enqueue (entity);
        }

        #endregion

        #region States

        public override void QueueState (StateObject state) {
            _addedStates.Enqueue (state);
        }

        public override void QueueStateRemoval (StateObject state) {
            _removedStates.Enqueue (state);
        }

        #endregion


    }
}
