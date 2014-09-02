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
using BLibrary.Json;
using System.Runtime.Serialization;
using BLibrary.Serialization;
using Starliners.Game.Notifications;
using System.Linq;
using System.Collections.Generic;
using Starliners.Game.Forces;
using Starliners.Game.Planets;

namespace Starliners.Game {
    [Serializable]
    public sealed class Faction : StateObject, IHeralded, IFleetBacker, INotifiable {
        #region Constants

        public const string NAME_PREFIX = "faction";

        const string STAT_TICK_INCORPORATED = "TickIncorporated";
        const string STAT_TICK_ELIMINATED = "TickEliminated";

        const string STAT_INCOME_EARNED = "IncomeEarned";
        const string STAT_LOOT_EARNED = "LootEarned";

        public const string STAT_COMBAT_FLEETS_FLED = "CombatsFled";
        public const string STAT_COMBAT_ENGAGED = "CombatsEngaged";
        public const string STAT_COMBAT_ATTACKER = "CombatsAttacker";
        public const string STAT_COMBAT_DEFENDER = "CombatsDefender";
        public const string STAT_COMBAT_VICTORY = "CombatsVictory";
        public const string STAT_COMBAT_DEFEAT = "CombatsDefeat";
        public const string STAT_COMBAT_VICTORY_OFFENSIVE = "CombatsVictoryOffensive";
        public const string STAT_COMBAT_DEFEAT_OFFENSIVE = "CombatsDefeatOffensive";
        public const string STAT_COMBAT_VICTORY_DEFENSIVE = "CombatsVictoryDefensive";
        public const string STAT_COMBAT_DEFEAT_DEFENSIVE = "CombatsDefeatDefensive";

        public const string STAT_PLANETS_CONQUERED = "PlanetsConquered";
        public const string STAT_PLANETS_LOST = "PlanetsLost";

        public static readonly StatsRecorder<int>.StatisticSlot[] INFO_SLOTS = new StatsRecorder<int>.StatisticSlot[] {
            new StatsRecorder<int>.StatisticSlot ("existence", STAT_TICK_INCORPORATED),
            new StatsRecorder<int>.StatisticSlot ("existence", STAT_TICK_ELIMINATED),

            new StatsRecorder<int>.StatisticSlot ("loot", STAT_INCOME_EARNED),
            new StatsRecorder<int>.StatisticSlot ("loot", STAT_LOOT_EARNED),

            new StatsRecorder<int>.StatisticSlot ("conquests", STAT_PLANETS_CONQUERED),
            new StatsRecorder<int>.StatisticSlot ("conquests", STAT_PLANETS_LOST),

            new StatsRecorder<int>.StatisticSlot ("combat", STAT_COMBAT_FLEETS_FLED),
            new StatsRecorder<int>.StatisticSlot ("combat", STAT_COMBAT_ENGAGED),
            new StatsRecorder<int>.StatisticSlot ("combat", STAT_COMBAT_ATTACKER),
            new StatsRecorder<int>.StatisticSlot ("combat", STAT_COMBAT_DEFENDER),
            new StatsRecorder<int>.StatisticSlot ("combat", STAT_COMBAT_VICTORY),
            new StatsRecorder<int>.StatisticSlot ("combat", STAT_COMBAT_DEFEAT),
            new StatsRecorder<int>.StatisticSlot ("combat", STAT_COMBAT_VICTORY_OFFENSIVE),
            new StatsRecorder<int>.StatisticSlot ("combat", STAT_COMBAT_DEFEAT_OFFENSIVE),
            new StatsRecorder<int>.StatisticSlot ("combat", STAT_COMBAT_VICTORY_DEFENSIVE),
            new StatsRecorder<int>.StatisticSlot ("combat", STAT_COMBAT_DEFEAT_DEFENSIVE),
        };

        #endregion

        #region Properties

        public Faction FleetOwner {
            get {
                return this;
            }
        }

        [GameData (Remote = true, Key = "FullName")]
        public string FullName {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "IsPlayable")]
        public bool IsPlayable {
            get;
            set;
        }

        [GameData (Remote = true, Key = "IsEliminated")]
        public bool IsEliminated {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Colours")]
        public ColourScheme Colours {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Blazon")]
        public Blazon Blazon {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "FleetStyle")]
        public string FleetStyle {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "FleetIcons")]
        public string FleetIcons {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "CombatProperties")]
        public CombatProperties CombatProperties {
            get;
            private set;
        }

        [GameData (Key = "Statistics")]
        public StatsRecorder<int> Statistics {
            get;
            private set;
        }

        /// <summary>
        /// Flags set on this faction.
        /// </summary>
        public HashSet<string> Flags {
            get {
                return _preset.Flags;
            }
        }

        public IReadOnlyList<Culture> Cultures {
            get {
                return _preset.Cultures;
            }
        }

        #endregion

        [GameData (Remote = true, Key = "Preset")]
        FactionPreset _preset;

        public Faction (IWorldAccess access, string name, FactionPreset preset)
            : base (access, Utils.BuildName (NAME_PREFIX, name)) {

            IsTickable = true;
            Statistics = new StatsRecorder<int> ();
            Statistics.NoteStat (STAT_TICK_INCORPORATED, (int)access.Clock.Ticks);

            FullName = preset.FullName;
            Colours = preset.Colours;
            Blazon = preset.Blazon;
            FleetStyle = preset.FleetStyle;
            FleetIcons = preset.FleetIcons;

            _preset = preset;
            CombatProperties = new CombatProperties (Access.Seed);
        }

        #region Serialization

        public Faction (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public override void Tick (TickType ticks) {
            base.Tick (ticks);

            if (ticks.HasFlag (TickType.Partial)) {

                if (!IsEliminated) {
                    if (IsPlayable && !Access.States.Values.OfType<Planet> ().Any (p => p.Owner == this)) {
                        Eliminate ();
                        NotificationManager.Instance.Notify (this, NotificationCategories.FACTION_ELIMINATED, new TextComposition ("faction_eliminated", new TextComponent (FullName)));
                        foreach (Player player in Access.Players.Values.Where(p => p.MainFaction == this)) {
                            player.OpenGUI ((ushort)GuiIds.Elimination, this);
                        }
                    }
                }
            }
            if (ticks.HasFlag (TickType.Orbit)) {
                if (!IsEliminated) {
                    foreach (Player player in Access.Players.Values.Where (p => p.HasPermission (this, PermissionKeys.LOOT_SHARE))) {
                        player.HighScore.Transfer (ScoreKeeper.SCORE_SURVIVAL_TIME, 2);
                    }
                }
            }
        }

        void Eliminate () {
            IsEliminated = true;
            Statistics.NoteStat (STAT_TICK_ELIMINATED, (int)Access.Clock.Ticks);
        }

        public void OnFleetDisolution (Fleet fleet) {
        }

        public IEnumerable<Player> GetListeningPlayers () {
            return Access.Players.Values.Where (p => p.HasPermission (this, PermissionKeys.FACTION_NOTIFICATIONS));
        }

        public void AssignLoot (string category, int loot) {
            Statistics.NoteStat (STAT_LOOT_EARNED, loot);
            Statistics.NoteStat (STAT_INCOME_EARNED, loot * 20);
            foreach (Player player in Access.Players.Values.Where (p => p.HasPermission (this, PermissionKeys.LOOT_SHARE))) {
                player.Bookkeeping.Transfer (category, loot * 20, true);
                player.HighScore.Transfer (category, loot);
            }
        }

        public void AssignIncome (string category, int mining) {
            Statistics.NoteStat (STAT_INCOME_EARNED, mining);
            foreach (Player player in Access.Players.Values.Where (p => p.HasPermission (this, PermissionKeys.LOOT_SHARE))) {
                player.Bookkeeping.Transfer (category, mining, false);
            }
        }
    }
}

