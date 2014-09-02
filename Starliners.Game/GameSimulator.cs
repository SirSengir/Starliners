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

﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BLibrary.Saves;
using BLibrary.Util;
using BLibrary.Network;
using BLibrary;
using BLibrary.Gui.Data;
using Starliners.Game;
using Starliners.Network;


namespace Starliners {

    /// <summary>
    /// Core class representing the simulating ('server') side of the game.
    /// </summary>
    public sealed class GameSimulator : GameCore, IAccessSimulator {
        NetworkingServer _server;
        IGameState _gameState;
        SaveGame _save;
        MetaContainer _parameters;
        IScenarioProvider _scenario;
        List<IContainerCreator> _containerCreators = new List<IContainerCreator> ();
        List<IActionHandler> _actionHandlers = new List<IActionHandler> ();

        public bool AcceptsClients {
            get {
                return _server != null && _server.AcceptsConnections
                && RunningState.Instance != null;
            }
        }

        #region Constructor

        public GameSimulator (IContainerCreator guiHandler, IActionHandler actionHandler)
            : base (new SLConsoleSimulator (GameAccess.Folders [Constants.PATH_LOG], GameAccess.Game.LogLevels.ToArray ())) {
            _containerCreators.Add (guiHandler);
            _actionHandlers.Add (actionHandler);
            packetHandlers.Add (new PacketHandler ());
        }

        #endregion

        public void SetWorldParameters (MetaContainer parameters, IScenarioProvider scenario) {
            _parameters = parameters;
            _scenario = scenario;
        }

        void Reset () {
            ShouldStop = false;

            _gameState = null;
            _save = null;
            _parameters = null;
            _scenario = null;
        }

        public void SetSaveToLoad (SaveGame save) {
            _save = save;
        }

        public Container GetGuiContainer (ushort id, Player player, params object[] args) {
            foreach (IContainerCreator handler in _containerCreators) {
                Container container = handler.GetContainer (id, player, args);
                if (container != null) {
                    return container;
                }
            }

            return null;
        }

        public bool HandleGuiAction (Player player, Container container, string key, Payload args) {
            for (int i = 0; i < _actionHandlers.Count; i++) {
                if (_actionHandlers [i].HandleAction (player, container, key, args)) {
                    return true;
                }
            }
            return false;
        }

        public override void Work () {
            //try {
            _server = new NetworkingServer (new List<IPacketReader> { new PacketReader () }, packetHandlers);

            // Start to listen for network activity.
            ThreadNetworking = new Thread (_server.Listen) { Name = "ServerNet" };
            ThreadNetworking.Start ();

            while (!ShouldStop || _server.HasPendingPackets) {

                // Dispatch network packages if needed
                _server.DispatchPackets ();

                if (_gameState != null)
                    _gameState.Tick ();
                else if (_save != null)
                    _gameState = new RunningState (_save);
                else if (_parameters != null && _scenario != null)
                    _gameState = new RunningState (_parameters, _scenario);

            }

            if (_gameState != null) {
                _gameState.OnExit ();
            }

            GameAccess.Simulator.GameConsole.Debug ("Shutting down server networking...");
            _server.Stop ();
            GameAccess.Simulator.GameConsole.Debug ("Waiting for server networking to exit...");
            ThreadNetworking.Join ();
            GameAccess.Simulator.GameConsole.Debug ("Server networking exited...");

            Reset ();
            //} catch (Exception ex) {
            //    GameAccess.CrashReporter.ReportCrash (ex);
            //    if (GameAccess.Interface != null) {
            //        GameAccess.Interface.Close ();
            //    }
            //throw;
            //}
        }
    }
}
