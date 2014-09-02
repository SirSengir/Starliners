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
using BLibrary.Util;
using BLibrary.Saves;
using Starliners.Network;
using Starliners.Game;
using System.Collections.Generic;
using BLibrary.Serialization;
using BLibrary;
using Starliners.Gui;
using Starliners.Game.Forces;

namespace Starliners {

    public sealed class GameDefinition : IGameDefinition {

        #region Properties

        public string DataAssembly {
            get {
                return "Starliners.Data";
            }
        }

        public string ResourceAssembly {
            get {
                return "Starliners.Resources";
            }
        }

        public int MinZoom {
            get {
                return 0;
            }
        }

        public int MaxZoom {
            get {
                return 4;
            }
        }

        public int DefaultZoom {
            get {
                return 1;
            }
        }

        public ushort IdGuiPopup {
            get {
                return 0;
            }
        }

        public IReadOnlyList<IScenarioProvider> Scenarios {
            get {
                return _scenarios;
            }
        }

        public IReadOnlyList<ParameterOptions> SetupOptions {
            get {
                return _setupoptions;
            }
        }

        public IReadOnlyList<SoundDefinition> Sounds {
            get {
                return _sounds;
            }
        }

        public IReadOnlyList<LogLevel> LogLevels {
            get {
                return _loglevels;
            }
        }

        #endregion

        #region Fields

        List<IScenarioProvider> _scenarios = new List<IScenarioProvider> () { new ScenarioProvider () };

        List<ParameterOptions> _setupoptions = new List<ParameterOptions> () {

            new ParameterOptions (ParameterKeys.NAME, "general_options") { Default = "Default" },
            new ParameterOptions (ParameterKeys.CREATOR, "general_options") { Default = "ThePlayer" },
            new ParameterOptions ("general_options"),

            new ParameterOptions (ParameterKeys.EMPIRE_COUNT, "empire_options",
                new object[] {
                    1, 2, 3, 4, 5, 6, 7, 8
                },
                new string[] {
                    "Single", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight"
                }
            ),
            new ParameterOptions (ParameterKeys.EMPIRE_SIZE, "empire_options",
                new object[] {
                    4, 8, 16
                },
                new string[] {
                    "Small", "Normal", "Large"
                }
            )
        };

        List<SoundDefinition> _sounds = new List<SoundDefinition> () {
            new SoundDefinition (SoundKeys.CLICK, "Sounds.Click.ogg"),
            new SoundDefinition (SoundKeys.BREAK, "Sounds.Break.ogg"),
            new SoundDefinition (SoundKeys.NOTIFICATION, "Sounds.Notification.ogg"),
            new SoundDefinition (SoundKeys.MONEY_0, "Sounds.Money0.ogg"),
            new SoundDefinition (SoundKeys.MONEY_1, "Sounds.Money1.ogg"),
            new SoundDefinition (SoundKeys.HARVEST, "Sounds.Harvest.ogg"),
            new SoundDefinition (SoundKeys.GROW, "Sounds.Grow.ogg"),
            new SoundDefinition (SoundKeys.LASER_FIRE, "Sounds.Laser") {
                IsCollection = true,
                IsRandomized = true
            }
        };

        List<LogLevel> _loglevels = new List<LogLevel> {
            new LogLevel (Battle.LOG_LEVEL) { OwnLog = true, MainLog = false }
        };

        #endregion

        public MetaContainer CreateDefaultParameters () {
            MetaContainer parameters = new MetaContainer ();
            parameters.Set (ParameterKeys.NAME, "Default");
            parameters.Set (ParameterKeys.CREATOR, "System");
            parameters.Set (ParameterKeys.EMPIRE_SIZE, 4);
            parameters.Set (ParameterKeys.EMPIRE_COUNT, 4);
            return parameters;
        }

        public SaveMapper GetSaveMapper (SaveHeader header, GameConsole console) {
            SaveMapper mapper = new SaveMapper (console);
            /*
            if (header.Format < 2) {
                GameAccess.Simulator.GameConsole.Info ("Adding save game mappings to load a save created with version {0}:{1}.", header.Version, header.Format);
                mapper.AddNameToTypeMapping ("BeesGame.Items.MetaGustatory", typeof(MetaProduct));
            }*/

            return mapper;
        }

        public void Tick (IWorldAccess access, TickType ticks) {
        }

        public void InitWorld (IWorldAccess access) {
        }

        public void OnWorldLoad (IWorldAccess access) {
        }

        public void HandleRequest (NetInterfaceServer netInterface, byte request) {
            switch ((RequestIds)request) {
                case RequestIds.Hud:
                    netInterface.Player.OpenGUI ((ushort)GuiIds.Hud);
                    netInterface.Player.OpenGUI ((ushort)GuiIds.Notifications, false);
                    return;
                case RequestIds.OpenChatline:
                    netInterface.Player.OpenGUI ((ushort)GuiIds.Chatline);
                    return;
                case RequestIds.OpenHistory:
                    netInterface.Player.OpenGUI ((ushort)GuiIds.History);
                    return;
                case RequestIds.OpenLoss:
                    netInterface.Player.OpenGUI ((ushort)GuiIds.Elimination);
                    return;
            }
        }

        public bool HandleKeyboardShortcut (KeysU key) {
            switch (key) {
                case KeysU.H:
                    GameAccess.Interface.Controller.RequestAction (RequestIds.OpenHistory);
                    return true;
                case KeysU.L:
                    GameAccess.Interface.Controller.RequestAction (RequestIds.OpenLoss);
                    return true;
                case KeysU.T:
                    GameAccess.Interface.Controller.RequestAction (RequestIds.OpenChatline);
                    return true;
            }
            return false;
        }

    }
}

