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

namespace BLibrary.Gui.Data {

    public struct UITicket : IEquatable<UITicket> {

        public string Key;

        public int ContainerId { get; set; }

        #region Constructor

        public UITicket (string key)
            : this () {
            Key = key;
            ContainerId = -1;
        }

        #endregion

        public override int GetHashCode () {
            int hash = 17;
            hash = hash * 23 + Key.GetHashCode ();
            hash = hash * 23 + ContainerId.GetHashCode ();
            return hash;
        }

        public bool Equals (UITicket other) {
            return ContainerId == other.ContainerId && string.Equals (Key, other.Key);
        }

    }
}

