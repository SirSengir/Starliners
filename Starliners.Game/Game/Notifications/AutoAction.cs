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

﻿using BLibrary.Util;

namespace Starliners.Game.Notifications {

    public abstract class AutoAction {
        /// <summary>
        /// Do the notification-associated action for the given player.
        /// </summary>
        /// <param name="player"></param>
        public abstract void DoAction (Player player);
    }

    /// <summary>
    /// No action
    /// </summary>
    sealed class AutoActionNone : AutoAction {
        public override void DoAction (Player player) {
        }
    }

    /*
    /// <summary>
    /// Open a machine gui and center on the machine's location.
    /// </summary>
    sealed class AutoActionMachine : AutoAction {
        ushort _guiId;
        Vect2f _location;
        object[] _args;

        public AutoActionMachine (ushort guiId, Vect2f location, params object[] args) {
            _guiId = guiId;
            _location = location;
            _args = args;
        }

        public override void DoAction (Player player) {
            player.OpenGUI (_guiId, _args);
            //player.Access.Controller.SetView (player, _location);
        }
    }
    */

    /// <summary>
    /// Open a gui with the passed arguments.
    /// </summary>
    sealed class AutoActionGui : AutoAction {
        ushort _guiId;
        object[] _args;

        public AutoActionGui (ushort guiId, params object[] args) {
            _guiId = guiId;
            _args = args;
        }

        public override void DoAction (Player player) {
            player.OpenGUI (_guiId, false, _args);
        }
    }
}
