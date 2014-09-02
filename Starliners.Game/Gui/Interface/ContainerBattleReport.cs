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
using Starliners.Game.Forces;
using Starliners.Game;

namespace Starliners.Gui.Interface {

    sealed class ContainerBattleReport : Container {

        BattleReport _report;

        public ContainerBattleReport (ushort guiId, Player player, BattleReport report)
            : base (guiId) {

            Precedence = "report";
            _report = report;

            UpdateFragment (Constants.FRAGMENT_GUI_HEADER, report.FullName);
            OnChanged ();
        }

        protected override void Refresh (object sender, EventArgs e) {
            base.Refresh (sender, e);
            UpdateFragment (KeysFragments.REPORT_BATTLE, _report);
        }

        public override PrecedenceBehaviour DeterminePrecedence (Container other) {
            ContainerBattleReport compare = other as ContainerBattleReport;
            if (compare == null) {
                return PrecedenceBehaviour.None;
            }
            if (base.DeterminePrecedence (other) == PrecedenceBehaviour.None) {
                return PrecedenceBehaviour.None;
            }

            return _report.Serial == compare._report.Serial ? PrecedenceBehaviour.KeepExisting : PrecedenceBehaviour.ReplaceExisting;
        }

    }
}

