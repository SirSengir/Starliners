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
using BLibrary.Gui.Data;
using Starliners.Game;
using System.Linq;
using BLibrary.Network;
using Starliners.Game.Forces;
using BLibrary.Util;

namespace Starliners.Gui.Interface {
    sealed class ContainerFleet : Container {

        public override bool MustClose {
            get {
                return base.MustClose || _fleet.IsDead;
            }
            set {
                base.MustClose = value;
            }
        }

        Fleet _fleet;

        public ContainerFleet (ushort guiId, Player player, Fleet fleet)
            : base (guiId) {

            Precedence = "fleet";
            _fleet = fleet;

            ActionHandling += delegate(Player plyr, Container container, string key, Payload payload) {
                if (!KeysActions.FLEET_DISBAND.Equals (key)) {
                    return;
                }
                if (!plyr.HasPermission (_fleet.Owner, PermissionKeys.FLEET_MANAGMENT)) {
                    plyr.SendChat (Colour.Crimson, "permission_denied_fleet_disband");
                    return;
                }
                _fleet.Dissolve (true);
            };
            ActionHandling += delegate(Player plyr, Container container, string key, Payload payload) {
                if (!KeysActions.FLEET_RELOCATE.Equals (key)) {
                    return;
                }
                if (!plyr.HasPermission (_fleet.Owner, PermissionKeys.FLEET_MANAGMENT)) {
                    plyr.SendChat (Colour.Crimson, "permission_denied_fleet_relocate");
                    return;
                }
                plyr.Access.Controller.UpdateHeld (plyr, new FleetRelocator (plyr.Access, _fleet));
            };

            UpdateFragment (Constants.FRAGMENT_GUI_HEADER, _fleet.FullName);
            UpdateFragment (KeysFragments.FLEET_SERIAL, _fleet.Serial);

            Subscribe (_fleet);
            OnChanged ();

        }

        protected override void Refresh (object sender, EventArgs e) {
            base.Refresh (sender, e);
            UpdateFragment (KeysFragments.FLEET_LEVIES, _fleet.Levies.Select (p => p.Serial).ToList ());
        }

        public override PrecedenceBehaviour DeterminePrecedence (Container other) {
            ContainerFleet compare = other as ContainerFleet;
            if (compare == null) {
                return PrecedenceBehaviour.None;
            }
            if (base.DeterminePrecedence (other) == PrecedenceBehaviour.None) {
                return PrecedenceBehaviour.None;
            }

            return _fleet.Serial == compare._fleet.Serial ? PrecedenceBehaviour.KeepExisting : PrecedenceBehaviour.ReplaceExisting;
        }

    }
}

