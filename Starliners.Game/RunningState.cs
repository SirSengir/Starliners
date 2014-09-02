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
using System.Runtime.Serialization.Formatters.Binary;
using BLibrary.Saves;
using BLibrary.Util;
using Starliners.Game;

namespace Starliners {

    class RunningState : IGameState {
        #region Instance

        static RunningState instance;

        public static RunningState Instance {
            get { return instance; }
        }

        #endregion

        List<WorldSimulator> _worlds = new List<WorldSimulator> ();

        #region Properties

        public IList<WorldSimulator> Worlds { get { return _worlds.AsReadOnly (); } }

        #endregion

        public RunningState (SaveGame save) {
            instance = this;

            WorldSimulator world = new WorldSimulator (save.File, new BinaryFormatter ());
            _worlds.Add (world);
        }

        public RunningState (MetaContainer parameters, IScenarioProvider scenario) {
            instance = this;
            WorldSimulator world = new WorldSimulator (parameters, scenario);
            _worlds.Add (world);
        }

        public void Tick () {
            Timer timer = null;
            foreach (WorldSimulator world in _worlds) {
                Timer worldtimer = world.Tick ();
                if (timer == null || timer.Remaining > worldtimer.Remaining) {
                    timer = worldtimer;
                }
            }

            while (timer.IsDelayed) {
                System.Threading.Thread.Sleep (10);
            }
        }

        public void OnExit () {
            GameAccess.Simulator.GameConsole.Info ("Exiting running state...");
            SaveAll ();
            GameAccess.Simulator.GameConsole.Info ("Exited running state.");
        }

        void SaveAll () {
            GameAccess.Simulator.GameConsole.Info ("Saving all worlds to " + GameAccess.Folders [Constants.PATH_SAVES].Location.FullName);

            foreach (WorldSimulator world in _worlds) {
                world.Save (false);
            }
        }
    }
}
