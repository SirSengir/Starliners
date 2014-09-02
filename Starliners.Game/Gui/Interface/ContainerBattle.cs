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

namespace Starliners.Gui.Interface {
    sealed class ContainerBattle : Container {

        public override bool MustClose {
            get {
                return base.MustClose || _battle.IsDead;
            }
            set {
                base.MustClose = value;
            }
        }

        Battle _battle;

        public ContainerBattle (ushort guiId, Player player, Battle battle)
            : base (guiId) {

            Precedence = "battle";
            _battle = battle;

            UpdateFragment (Constants.FRAGMENT_GUI_HEADER, _battle.FullName);
            UpdateFragment (KeysFragments.BATTLE_SERIAL, _battle.Serial);

            Subscribe (_battle);
            OnChanged ();

        }

        protected override void Refresh (object sender, EventArgs e) {
            base.Refresh (sender, e);
            UpdateFragment (KeysFragments.BATTLE_GRID_ATTACKER, _battle.Attacking);
            UpdateFragment (KeysFragments.BATTLE_GRID_DEFENDER, _battle.Defending);

            UpdateFragment (KeysFragments.BATTLE_FLEETS_ATTACKER, _battle.OffensiveFleets.Select (p => p.Serial).ToList ());
            UpdateFragment (KeysFragments.BATTLE_FLEETS_DEFENDER, _battle.DefensiveFleets.Select (p => p.Serial).ToList ());

        }

        public override PrecedenceBehaviour DeterminePrecedence (Container other) {
            ContainerBattle compare = other as ContainerBattle;
            if (compare == null) {
                return PrecedenceBehaviour.None;
            }
            if (base.DeterminePrecedence (other) == PrecedenceBehaviour.None) {
                return PrecedenceBehaviour.None;
            }

            return _battle.Serial == compare._battle.Serial ? PrecedenceBehaviour.KeepExisting : PrecedenceBehaviour.ReplaceExisting;
        }

    }
}

