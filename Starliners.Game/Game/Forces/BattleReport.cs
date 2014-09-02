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
using System.Runtime.Serialization;
using System.Collections.Generic;
using BLibrary.Serialization;

namespace Starliners.Game.Forces {
    [Serializable]
    public sealed class BattleReport : SerializableObject, IIncident {

        [GameData (Remote = true, Key = "FullName")]
        public string FullName {
            get;
            private set;
        }

        public string Description {
            get {
                return FullName;
            }
        }

        [GameData (Remote = true, Key = "HistoryTick")]
        public long HistoryTick {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "EndTick")]
        public long EndTick {
            get;
            private set;
        }

        public IReadOnlyList<Faction> Attackers {
            get { return _attackers.Listing; }
        }

        public IReadOnlyList<Faction> Defenders {
            get { return _defenders.Listing; }
        }

        [GameData (Remote = true, Key = "Outcome")]
        public BattleResolution Outcome {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Attackers")]
        IdList<Faction> _attackers;
        [GameData (Remote = true, Key = "Defenders")]
        IdList<Faction> _defenders;

        [GameData (Remote = true, Key = "Attacking")]
        Dictionary<ulong, ShipReport> _attacking = new Dictionary<ulong, ShipReport> ();
        [GameData (Remote = true, Key = "Defending")]
        Dictionary<ulong, ShipReport> _defending = new Dictionary<ulong, ShipReport> ();
        [GameData (Remote = true, Key = "Cumulative")]
        Dictionary<ulong, ShipReport> _cumulative = new Dictionary<ulong, ShipReport> ();

        public BattleReport (IWorldAccess access, string name)
            : base (access) {
            FullName = name;
            _attackers = new IdList<Faction> (access);
            _defenders = new IdList<Faction> (access);

            HistoryTick = access.Clock.Ticks;
            EndTick = -1;
        }

        #region Serialization

        public BattleReport (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public void RegisterSalvos (BattleGrid attacking, BattleGrid defending) {
            foreach (Salvo salvo in defending.Salvos) {
                RegisterSalvo (attacking [salvo.OriginSlot].ShipClass, defending [salvo.TargetSlot].ShipClass, salvo, false);
            }
            foreach (Salvo salvo in attacking.Salvos) {
                RegisterSalvo (defending [salvo.OriginSlot].ShipClass, attacking [salvo.TargetSlot].ShipClass, salvo, true);
            }
        }

        public void NoteOutcome (BattleResolution outcome) {
            Outcome = outcome;
            EndTick = Access.Clock.Ticks;
        }

        void RegisterSalvo (ShipClass origin, ShipClass target, Salvo salvo, bool defensive) {
            NoteAttack (origin, salvo, defensive ? _defending : _attacking);
            NoteAttack (origin, salvo, _cumulative);
            NoteDefense (target, salvo, defensive ? _attacking : _defending);
            NoteDefense (target, salvo, _cumulative);
        }

        void NoteAttack (ShipClass attacker, Salvo salvo, Dictionary<ulong, ShipReport> reports) {
            AssertReport (attacker, reports);
            ShipReport report = reports [attacker.Serial];
            report.NoteStat (ShipReport.SHOTS_FIRED, 1);

            if (salvo.Damage.NoEffect) {
                report.NoteStat (ShipReport.SHOTS_MISSED, 1);
            } else {
                switch (salvo.Shot.Kind) {
                    case DamageKind.Kinetic:
                        report.NoteStat (ShipReport.ATTACK_KINETIC_DELIVERED, salvo.Damage.Delivered);
                        report.NoteStat (ShipReport.ATTACK_KINETIC_RESISTED, salvo.Damage.Resisted);
                        break;
                    case DamageKind.Heat:
                        report.NoteStat (ShipReport.ATTACK_HEAT_DELIVERED, salvo.Damage.Delivered);
                        report.NoteStat (ShipReport.ATTACK_HEAT_RESISTED, salvo.Damage.Resisted);
                        break;
                    case DamageKind.Radiation:
                        report.NoteStat (ShipReport.ATTACK_RADIATION_DELIVERED, salvo.Damage.Delivered);
                        report.NoteStat (ShipReport.ATTACK_RADIATION_RESISTED, salvo.Damage.Resisted);
                        break;
                }
            }

            if (salvo.Damage.Type.HasFlag (HitType.Final)) {
                report.NoteStat (ShipReport.KILLS, 1);
                report.NoteStat (ShipReport.LOOT_WON, salvo.Loot);
            }
        }

        void NoteDefense (ShipClass defender, Salvo salvo, Dictionary<ulong, ShipReport> reports) {
            AssertReport (defender, reports);
            ShipReport report = reports [defender.Serial];
            report.NoteStat (ShipReport.SHOTS_RECEIVED, 1);

            if (salvo.Damage.NoEffect) {
                report.NoteStat (ShipReport.SHOTS_DODGED, 1);
            } else {
                report.NoteStat (ShipReport.DMG_SHIELD_DAMAGE, salvo.Damage.ShieldDamage);
                report.NoteStat (ShipReport.DMG_SHIELD_RESISTED, salvo.Damage.ShieldResisted);
                report.NoteStat (ShipReport.DMG_ARMOUR_DAMAGE, salvo.Damage.ArmourDamage);
                report.NoteStat (ShipReport.DMG_ARMOUR_RESISTED, salvo.Damage.ArmourResisted);
                report.NoteStat (ShipReport.DMG_STRUCTURE_DAMAGE, salvo.Damage.StructureDamage);
                report.NoteStat (ShipReport.DMG_STRUCTURE_RESISTED, salvo.Damage.StructureResisted);
            }

            if (salvo.Damage.Type.HasFlag (HitType.Final)) {
                report.NoteStat (ShipReport.LOSSES, 1);
                report.NoteStat (ShipReport.LOOT_LOST, salvo.Loot);
            }
        }

        void AssertReport (ShipClass ship, Dictionary<ulong, ShipReport> reports) {
            if (reports.ContainsKey (ship.Serial)) {
                return;
            }

            reports [ship.Serial] = new ShipReport ();
        }
    }
}

