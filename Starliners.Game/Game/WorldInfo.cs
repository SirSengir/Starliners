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
using System.Linq;
using BLibrary.Util;

namespace Starliners.Game {

    [Serializable]
    public sealed class WorldInfo {
        public int Ordinal {
            get;
            private set;
        }

        public string Name {
            get;
            private set;
        }

        public PlayerSlot[] Slots {
            get;
            private set;
        }

        public WorldInfo (int ordinal, WorldSimulator world) {
            Ordinal = ordinal;
            Name = world.Access.Name;

            List<PlayerSlot> slots = new List<PlayerSlot> ();
            foreach (Faction faction in world.Access.States.Values.OfType<Faction>().Where(p => p.IsPlayable)) {
                Player player = world.Access.Players.Values.Where (p => p.MainFaction == faction).FirstOrDefault ();
                slots.Add (new PlayerSlot (faction, player != null ? player.Name : string.Empty));
            }
            Slots = slots.ToArray ();
        }
    }

    [Serializable]
    public sealed class PlayerSlot {

        public ulong Serial {
            get;
            private set;
        }

        public string Name {
            get;
            private set;
        }

        public string FleetIcons {
            get;
            private set;
        }

        public ColourScheme Colours {
            get;
            private set;
        }

        public Blazon Blazon {
            get;
            private set;
        }

        public string PlayerName {
            get;
            set;
        }

        public PlayerSlot (Faction faction, string player) {
            Serial = faction.Serial;
            Name = faction.FullName;
            FleetIcons = faction.FleetIcons;
            Colours = faction.Colours;
            Blazon = faction.Blazon;
            PlayerName = player;
        }
    }

}

