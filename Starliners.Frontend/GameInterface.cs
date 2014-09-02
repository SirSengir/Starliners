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
using System.Net;
using System.Threading;
using BLibrary;
using BLibrary.Graphics;
using BLibrary.Gui;
using BLibrary.Gui.Data;
using BLibrary.Network;
using BLibrary.Saves;
using BLibrary.Util;
using OpenTK;
using Starliners.Game;
using Starliners.Gui.Interface;
using Starliners.Network;
using Starliners.States;

namespace Starliners {

    /// <summary>
    /// Core class representing the interface ('client') side of the game.
    /// </summary>
    sealed class GameInterface : GameCore, IAccessInterface {

        #region Properties

        public ServerCache ServerCache {
            get;
            private set;
        }

        public bool IsInGame {
            get {
                if (WorldInterface.Instance == null) {
                    return false;
                }

                return ThePlayer != null;
            }
        }

        public Player ThePlayer {
            get {
                return WorldInterface.Instance != null ? WorldInterface.Instance.ThePlayer : null;
            }
        }

        public IWorldAccess Local {
            get {
                return WorldInterface.Instance.Access;
            }
        }

        public Entity HoveredEntity {
            get {
                return MapState.Instance.Map.HoveredEntity;
            }
        }

        public NetInterfaceClient Controller {
            get {
                return MapState.Instance.Controller;
            }
        }

        public Vect2i MousePosition {
            get { return new Vect2i (_scene.Window.Mouse.X, _scene.Window.Mouse.Y); }
        }

        public Vect2i WindowSize {
            get {
                return new Vect2i (_scene.Window.Width, _scene.Window.Height);
            }
        }

        public bool IsConnected {
            get {
                return _client != null && _client.IsConnected;
            }
        }

        public LaunchDefinition Launch {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the frames rendered per second.
        /// </summary>
        /// <value>The frames per second.</value>
        public float FramesPerSecond {
            get;
            set;
        }

        public Dictionary<string, int> GLOperationsPerFrame {
            get { return _glOperationStats; }
        }

        #endregion

        IInterfaceDefinition _uiProvider;
        RenderScene _scene;
        List<IGuiCreator> _guiCreators = new List<IGuiCreator> ();
        Dictionary<string, int> _glOperationStats = new Dictionary<string, int> ();

        NetworkingClient _client;

        #region Constructor

        public GameInterface (IInterfaceDefinition uiProvider, IGuiCreator guiHandler)
            : base (new SLConsoleInterface (GameAccess.Folders [Constants.PATH_LOG], GameAccess.Game.LogLevels.ToArray ())) {
            _guiCreators.Add (guiHandler);
            packetHandlers.Add (new PacketHandlerClient ());
            _uiProvider = uiProvider;
            _scene = new RenderScene ();
            ServerCache = new ServerCache ();
        }

        #endregion

        public object GetGuiElement (Container container) {
            foreach (IGuiCreator handler in _guiCreators) {
                GuiWindow element = handler.GetGui (container);
                if (element != null) {
                    return element;
                }
            }

            return null;
        }

        public object GetTooltip (ushort id, params object[] args) {
            foreach (IGuiCreator handler in _guiCreators) {
                Tooltip element = handler.GetTooltip (id, args);
                if (element != null) {
                    return element;
                }
            }

            return null;
        }

        /// <summary>
        /// Changes resolution and window mode of the game window.
        /// </summary>
        /// <param name="resolution">Resolution to change to.</param>
        /// <param name="fullscreen">If set to <c>true</c> fullscreen.</param>
        public void ChangeResolution (Vect2i resolution, bool fullscreen) {
            DisplayResolution selected = null;
            IList<DisplayResolution> validModes = DisplayDevice.Default.AvailableResolutions;
            for (int i = 0; i < validModes.Count; i++) {
                if (validModes [i].Width != resolution.X) {
                    continue;
                }
                if (validModes [i].Height != resolution.Y) {
                    continue;
                }

                if (selected != null) {
                    if (validModes [i].BitsPerPixel <= selected.BitsPerPixel)
                        continue;
                    if (validModes [i].RefreshRate <= selected.RefreshRate)
                        continue;
                }
                selected = validModes [i];
            }

            if (selected != null) {
                WindowState wstate = fullscreen ? WindowState.Fullscreen : WindowState.Normal;
                bool modechange = false;
                if (modechange = wstate != _scene.Window.WindowState) {
                    _scene.Window.WindowState = wstate;
                }

                if (wstate == WindowState.Fullscreen) {
                    // Changing to fullscreen or resolution in fullscreen requires a change to the display device's resolution and the window size.
                    if (modechange || (selected.Width != _scene.Window.Width || selected.Height != _scene.Window.Height)) {
                        DisplayDevice.Default.ChangeResolution (selected);
                        _scene.Window.Size = new System.Drawing.Size (selected.Width, selected.Height);
                        _scene.Window.Location = new System.Drawing.Point (0, 0);
                    }
                } else {
                    // Restore previous resolution if our mode changed.
                    if (modechange) {
                        DisplayDevice.Default.RestoreResolution ();
                    }
                    // Set the new window size if it doesn't match the old one.
                    if (selected.Width != _scene.Window.Width || selected.Height != _scene.Window.Height) {
                        _scene.Window.Size = new System.Drawing.Size (selected.Width, selected.Height);
                        _scene.Window.Location = new System.Drawing.Point ((DisplayDevice.Default.Width - selected.Width) / 2,
                            (DisplayDevice.Default.Height - selected.Height) / 2);
                    }
                }

                _scene.OnResized (_scene.Window);
            }

        }

        public void CreateGame (MetaContainer parameters, IScenarioProvider scenario) {
            Launch = new LaunchDefinition (new Credentials (Globals.Login, Globals.SessionID), new EmbeddedServer (parameters, scenario));
            SwitchTo (new LoadState (_scene));
        }

        public void CreateGame (SaveGame save) {
            Launch = new LaunchDefinition (new Credentials (Globals.Login, Globals.SessionID), new EmbeddedServer (save));
            SwitchTo (new LoadState (_scene));
        }

        public void JoinGame (IPAddress address, int port) {
            Launch = new LaunchDefinition (new Credentials (Globals.Login, Globals.SessionID), new RemoteServer (address, port));
            SwitchTo (new LoadState (_scene));
        }

        public void SwitchTo (object state) {
            _scene.Window.SwitchTo (state);
        }

        public void OpenMainMenu () {
            GuiManager.Instance.OpenGui (new GuiMenu ());
        }

        public override void Work () {

            // We need to create this before our main window,
            // since otherwise it would block input events on
            // OS X.
            NativeWindow secondaryWindow = new NativeWindow ();
            // Get the initial resolution
            Vect2i resolution = Vect2i.Parse (GameAccess.Settings.Get<string> ("video", "resolution"));

            using (_scene.Window = new DisplayWindow (_scene, _uiProvider, resolution.X, resolution.Y, "fullscreen".Equals (GameAccess.Settings.Get<string> ("video", "mode"))) { SecondaryWindow = secondaryWindow }) {
                _scene.Window.Run ();
            }

            // Save gui settings.
            GuiManager.Instance.SaveGuiPositions ();
            // Shutdown networking if any.
            StopNetworking ();

            // Halt the simulator if it was started.
            if (GameAccess.Simulator.ThreadMachina != null) {
                GameConsole.Info ("Simulator running, shutting down...");
                GameAccess.Simulator.ShouldStop = true;
                GameConsole.Info ("Waiting for simulator to shut down...");
                GameAccess.Simulator.ThreadMachina.Join ();
            }
        }

        public void Retire () {
            StopNetworking ();
            Launch.Abort ();
            SwitchTo (new MenuState (_scene));
        }

        public void Close () {
            _scene.Window.Exit ();
            ShouldStop = true;
        }

        public void ConnectTo (IPEndPoint endpoint) {
            _client = new NetworkingClient (new List<IPacketReader> { new PacketReader () }, packetHandlers, endpoint);
            _client.ConnectionClosed += OnConnectionAbort;
            ThreadNetworking = new Thread (_client.Connect) { Name = "ClientNet" };
            ThreadNetworking.Start ();
        }

        public void HandleNetworking () {
            if (_client != null) {
                _client.DispatchPackets ();
            }
        }

        void OnConnectionAbort (Networking sender, CustomEventArgs<string> args) {
            GameConsole.Info ("Connection to server closed: {0}", args.Argument);

            _client.Stop ();
            _client = null;
            Launch.Abort ();
            GuiManager.Instance.OpenMessageBox (new TextComponent ("msg_connection_lost") { IsLocalizable = true }, new TextComponent (args.Argument), new Widget.ClickCustom (delegate {
                SwitchTo (new MenuState (_scene));
            }));

        }

        void Stop () {
            StopNetworking ();
        }

        void StopNetworking () {
            if (_client == null) {
                return;
            }

            GameConsole.Debug ("Disconnecting from server...");
            if (IsInGame) {
                WorldInterface.Instance.PrepareDisconnect ();
            }
            GameConsole.Debug ("Shutting down client networking...");
            _client.Stop ();
            GameConsole.Debug ("Waiting for client networking to exit...");
            ThreadNetworking.Join ();
            GameAccess.Simulator.GameConsole.Debug ("Client networking exited...");

            _client = null;
        }

        public void CenterView (Vect2f worldCoords) {
            MapState.Instance.Map.CenterView (worldCoords);
        }

        /// <summary>
        /// ControlState on the Keyboard in the current frame.
        /// </summary>
        public ControlState KeyboardState {
            get { return GuiManager.Instance.KeyboardState; }
        }

        public void BindSecondaryContext () {
            _scene.Window.BindSecondaryContext ();
        }

        public void UnbindSecondaryContext () {
            _scene.Window.UnbindSecondaryContext ();
        }
    }
}
