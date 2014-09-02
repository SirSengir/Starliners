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
using BLibrary.Util;
using System.Collections.Generic;
using BLibrary.Serialization;
using System.Linq;
using Starliners.Game.Notifications;

namespace Starliners.Game.Forces {
    [Serializable]
    public sealed class Battle : StateObject {
        #region Constants

        public const string LOG_LEVEL = "Battle";
        const string NAME_PREFIX = "battle";
        const int UNDEAD_TURNS = 20;

        #endregion

        [GameData (Remote = true, Key = "FullName")]
        public string FullName {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Resolution")]
        public BattleResolution Resolution {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Attacking")]
        public BattleGrid Attacking {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Defending")]
        public BattleGrid Defending {
            get;
            private set;
        }

        public IReadOnlyList<Fleet> OffensiveFleets {
            get {
                return _offensiveFleets;
            }
        }

        public IReadOnlyList<Fleet> DefensiveFleets {
            get {
                return _defensiveFleets;
            }
        }

        [GameData (Remote = true, Key = "Report")]
        public BattleReport Report {
            get;
            private set;
        }

        #region Fields

        [GameData (Remote = true, Key = "Location")]
        Vect2d _location;
        [GameData (Remote = true, Key = "OffensiveFleets")]
        List<Fleet> _offensiveFleets = new List<Fleet> ();
        [GameData (Remote = true, Key = "DefensiveFleets")]
        List<Fleet> _defensiveFleets = new List<Fleet> ();

        [GameData (Remote = true, Key = "Attackers")]
        List<Faction> _attackers = new List<Faction> ();
        [GameData (Remote = true, Key = "Defenders")]
        List<Faction> _defenders = new List<Faction> ();

        [GameData (Remote = true, Key = "Turns")]
        int _turns;
        [GameData (Remote = true, Key = "DeadTurns")]
        int _dead;

        #endregion

        public Battle (IWorldAccess access, string name, Vect2d location, IEnumerable<Fleet> attackers, IEnumerable<Fleet> defenders)
            : base (access, Utils.BuildName (NAME_PREFIX, name.GetHashCode ().ToString ())) {
            IsTickable = true;

            _location = location;
            FullName = name;

            Attacking = new BattleGrid (Access);
            Defending = new BattleGrid (Access);

            Report = new BattleReport (Access, FullName);

            foreach (Fleet fleet in attackers) {
                JoinAttacker (fleet);
            }
            foreach (Fleet fleet in defenders) {
                JoinDefender (fleet);
            }
        }

        #region Serialization

        public Battle (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public override void Tick (TickType ticks) {
            base.Tick (ticks);

            // Spawn random particles on map
            if (Resolution == BattleResolution.None) {
                if (Access.Rand.NextDouble () < 0.2) {
                    Vect2d location = _location + new Vect2d (Access.Rand.NextDouble () * 2, Access.Rand.NextDouble () * 2) - new Vect2d (0.5, 2);
                    Access.Controller.SpawnParticle (new Particle (Access, location, ParticleId.Explosion) {
                        MaxAge = Access.Rand.Next (3) + 2,
                        Seed = Access.Rand.Next (4)
                    });
                }
            }

            if (!ticks.HasFlag (TickType.Partial)) {
                return;
            }

            if (Resolution != BattleResolution.None) {
                _dead++;
                // After a grace period, cleanup completed battles.
                if (_dead >= UNDEAD_TURNS) {
                    IsDead = true;
                }
                return;
            }
            _turns++;

            Access.GameConsole.Log (LOG_LEVEL, "|==== START COMBAT TURN {0} FOR {1} ====|", _turns, FullName);

            Access.GameConsole.Log (LOG_LEVEL, "== FLEET MANAGMENT PHASE ==");
            VerifyFleets (_offensiveFleets);
            VerifyFleets (_defensiveFleets);

            Access.GameConsole.Log (LOG_LEVEL, "== CLEANUP PHASE I ==");
            Attacking.CleanupHulks ();
            Defending.CleanupHulks ();

            Access.GameConsole.Log (LOG_LEVEL, "== FIRE PHASE ==");
            List<Salvo> attack = Attacking.Fire (GameClock.TICKS_PER_PARTIAL);
            List<Salvo> defend = Defending.Fire (GameClock.TICKS_PER_PARTIAL);

            Access.GameConsole.Log (LOG_LEVEL, "== DAMAGE PHASE ==");
            Defending.ReceiveFire (attack, GameClock.TICKS_PER_PARTIAL);
            Attacking.ReceiveFire (defend, GameClock.TICKS_PER_PARTIAL);
            DistributeLoot (Attacking, Defending);
            DistributeLoot (Defending, Attacking);
            Report.RegisterSalvos (Attacking, Defending);

            Access.GameConsole.Log (LOG_LEVEL, "== SUPPORT PHASE ==");
            Defending.DoSupport (GameClock.TICKS_PER_PARTIAL);
            Attacking.DoSupport (GameClock.TICKS_PER_PARTIAL);

            Access.GameConsole.Log (LOG_LEVEL, "== LEVY MAINTENANCE ==");
            VerifyFleets (_offensiveFleets);
            VerifyFleets (_defensiveFleets);

            Access.GameConsole.Log (LOG_LEVEL, "== REENFORCEMENT PHASE ==");
            Access.GameConsole.Log (LOG_LEVEL, "Reenforcing attacking side.");
            Attacking.Reenforce (_offensiveFleets);
            Access.GameConsole.Log (LOG_LEVEL, "Reenforcing defending side.");
            Defending.Reenforce (_defensiveFleets);

            Access.GameConsole.Log (LOG_LEVEL, "== RESOLUTION PHASE ==");
            Resolve ();

            Access.GameConsole.Log (LOG_LEVEL, "|==== END COMBAT TURN {0} FOR {1} ====|", _turns, FullName);

            if (Resolution != BattleResolution.None) {
                WrapBattle ();
            }

            MarkUpdated ();
        }

        void LogFleets (List<Fleet> fleets) {
            foreach (Fleet fleet in fleets) {
                Access.GameConsole.Log (LOG_LEVEL, "--- {0} with {1} levies.", fleet, fleet.Levies.Count);
                foreach (Levy levy in fleet.Levies) {
                    Access.GameConsole.Log (LOG_LEVEL, "\t-- {0} with {1} ships.", levy, levy.Ships.Count);
                }
            }
        }

        void DistributeLoot (BattleGrid attacking, BattleGrid defending) {
            Dictionary<Faction, int> beneficiaries = new Dictionary<Faction, int> ();
            foreach (Salvo salvo in defending.Salvos) {
                if (salvo.Loot <= 0) {
                    continue;
                }

                ShipInstance ship = attacking [salvo.OriginSlot];
                // Gain experience
                ship.GainExperience (salvo.Loot);
                // Note loot for distribution to factions
                Faction faction = ship.Origin.Owner;
                if (!beneficiaries.ContainsKey (faction)) {
                    beneficiaries [faction] = 0;
                }
                beneficiaries [faction] += salvo.Loot;
            }

            foreach (var entry in beneficiaries) {
                entry.Key.AssignLoot (ScoreKeeper.SCORE_COMBAT_SHIP_DESTROYED, entry.Value);
            }
        }

        /// <summary>
        /// Culls dead and depleted fleets in the battle.
        /// </summary>
        /// <param name="fleets">Fleets.</param>
        void VerifyFleets (List<Fleet> fleets) {
            List<Fleet> deadpool = null;
            foreach (Fleet fleet in fleets) {
                fleet.PurgeDepletedLevies ();

                if (!fleet.IsDead && fleet.State != FleetState.Depleted) {
                    continue;
                }
                Attacking.Recall (fleet);
                Defending.Recall (fleet);

                if (deadpool == null) {
                    deadpool = new List<Fleet> ();
                }
                deadpool.Add (fleet);
            }

            if (deadpool == null) {
                return;
            }

            foreach (Fleet fleet in deadpool) {
                RetreatFleet (fleet);
            }
        }

        void Resolve () {
            if (_offensiveFleets.Count <= 0 && _defensiveFleets.Count <= 0) {
                Access.GameConsole.Log (LOG_LEVEL, "Battle was a draw. Both sides were destroyed.");
                Resolution = BattleResolution.Draw;

            } else if (_offensiveFleets.Count <= 0) {
                Access.GameConsole.Log (LOG_LEVEL, "Defenders won. Attacking side was destroyed or retreated.");
                Resolution = BattleResolution.VictoryDefender;

                foreach (Faction faction in _attackers) {
                    NotifyDefeat (faction);
                    faction.Statistics.NoteStat (Faction.STAT_COMBAT_DEFEAT_OFFENSIVE, 1);
                }
                foreach (Faction faction in _defenders) {
                    NotifyVictory (faction);
                    faction.Statistics.NoteStat (Faction.STAT_COMBAT_VICTORY_DEFENSIVE, 1);
                }

            } else if (_defensiveFleets.Count <= 0) {
                Access.GameConsole.Log (LOG_LEVEL, "Attackers won. Defending side was destroyed or retreated.");
                Resolution = BattleResolution.VictoryAttacker;

                foreach (Faction faction in _attackers) {
                    NotifyVictory (faction);
                    faction.Statistics.NoteStat (Faction.STAT_COMBAT_VICTORY_OFFENSIVE, 1);
                }
                foreach (Faction faction in _defenders) {
                    NotifyDefeat (faction);
                    faction.Statistics.NoteStat (Faction.STAT_COMBAT_DEFEAT_DEFENSIVE, 1);
                }

            } else {
                Access.GameConsole.Log (LOG_LEVEL, "Battle continues. There are still {0} attacking and {1} defending fleet(s).", _offensiveFleets.Count, _defensiveFleets.Count);
                Access.GameConsole.Log (LOG_LEVEL, "Attacking fleets:");
                LogFleets (_offensiveFleets);
                Access.GameConsole.Log (LOG_LEVEL, "Defending fleets:");
                LogFleets (_defensiveFleets);
            }
        }

        void NotifyDefeat (Faction faction) {
            NotificationManager.Instance.Notify (faction, NotificationCategories.BATTLE_REPORT, (ushort)GuiIds.Battle, this, new TextComposition ("battle_defeat", new TextComponent (FullName)));
            faction.Statistics.NoteStat (Faction.STAT_COMBAT_DEFEAT, 1);
        }

        void NotifyVictory (Faction faction) {
            NotificationManager.Instance.Notify (faction, NotificationCategories.BATTLE_REPORT, (ushort)GuiIds.Battle, this, new TextComposition ("battle_victory", new TextComponent (FullName)));
            faction.Statistics.NoteStat (Faction.STAT_COMBAT_VICTORY, 1);
            faction.AssignLoot (ScoreKeeper.SCORE_COMBAT_BATTLES_WON, 10);
        }

        void WrapBattle () {
            Access.GameConsole.Log (LOG_LEVEL, "|==== POST BATTLE CLEANUP FOR {0} ====|", FullName);
            Access.GameConsole.Log (LOG_LEVEL, "Reporting the outcome to the historical archives.");
            Report.NoteOutcome (Resolution);
            HistoryTracker.GetForWorld (Access).RegisterIncident (Report);

            Access.GameConsole.Log (LOG_LEVEL, "Calling the tow trucks for remaining wrecks.");
            Attacking.CleanupHulks ();
            Defending.CleanupHulks ();

            Access.GameConsole.Log (LOG_LEVEL, "Sending everyone else home.");
            Disengage ();
            _offensiveFleets.Clear ();
            _defensiveFleets.Clear ();
        }

        public void JoinIfPossible (Fleet fleet) {
            if (_offensiveFleets.Count <= 0 || _defensiveFleets.Count <= 0) {
                return;
            }
            if (IsParticipant (fleet)) {
                return;
            }

            Fleet attacking = _offensiveFleets [0];
            Fleet defending = _defensiveFleets [0];

            if (DetermineAllied (fleet, attacking, defending)) {
                JoinAttacker (fleet);
            } else if (DetermineAllied (fleet, defending, attacking)) {
                JoinDefender (fleet);
            }
        }

        public void RetreatFleet (Fleet fleet) {
            fleet.Owner.Statistics.NoteStat (Faction.STAT_COMBAT_FLEETS_FLED, 1);
            _offensiveFleets.Remove (fleet);
            Attacking.Recall (fleet);

            _defensiveFleets.Remove (fleet);
            Defending.Recall (fleet);

            fleet.FlagEngaged (false);

            MarkUpdated ();
            Access.GameConsole.Log (LOG_LEVEL, "Removed or retreated fleet {0} from {1}.", fleet, this);
        }

        public bool IsParticipant (Fleet fleet) {
            return _offensiveFleets.Any (p => p == fleet) || _defensiveFleets.Any (p => p == fleet);
        }

        void JoinAttacker (Fleet fleet) {
            fleet.FlagEngaged (true);
            _offensiveFleets.Add (fleet);
            Access.GameConsole.Log (LOG_LEVEL, "Fleet {0} joined as an attacker in {1}", fleet, this);
            if (!_attackers.Contains (fleet.Owner)) {
                _attackers.Add (fleet.Owner);
                fleet.Owner.Statistics.NoteStat (Faction.STAT_COMBAT_ENGAGED, 1);
                fleet.Owner.Statistics.NoteStat (Faction.STAT_COMBAT_ATTACKER, 1);
                NotificationManager.Instance.Notify (fleet.Owner, NotificationCategories.BATTLE_REPORT, (ushort)GuiIds.Battle, this, new TextComposition ("battle_joined_offense", new TextComponent (FullName)));
            }
            MarkUpdated ();
        }

        void JoinDefender (Fleet fleet) {
            fleet.FlagEngaged (true);
            _defensiveFleets.Add (fleet);
            Access.GameConsole.Log (LOG_LEVEL, "Fleet {0} joined as a defender in {1}", fleet, this);
            if (!_defenders.Contains (fleet.Owner)) {
                _defenders.Add (fleet.Owner);
                fleet.Owner.Statistics.NoteStat (Faction.STAT_COMBAT_ENGAGED, 1);
                fleet.Owner.Statistics.NoteStat (Faction.STAT_COMBAT_DEFENDER, 1);
                NotificationManager.Instance.Notify (fleet.Owner, NotificationCategories.BATTLE_REPORT, (ushort)GuiIds.Battle, this, new TextComposition ("battle_joined_defense", new TextComponent (FullName)));
            }
            MarkUpdated ();
        }

        void Disengage () {
            foreach (Fleet fleet in _offensiveFleets) {
                fleet.FlagEngaged (false);
            }
            foreach (Fleet fleet in _defensiveFleets) {
                fleet.FlagEngaged (false);
            }
            MarkUpdated ();
        }

        int[] GetSideComposition (List<Fleet> fleets) {
            int[] counts = new int[ShipSizes.VALUES.Length - 1];
            foreach (Fleet fleet in fleets) {
                foreach (Levy levy in fleet.Levies) {
                    levy.Census (counts);
                }
            }
            return counts;
        }

        /// <summary>
        /// Determines whether the given fleet is allied with the given second fleet against the given third fleet.
        /// </summary>
        /// <returns><c>true</c>, if allied was determined, <c>false</c> otherwise.</returns>
        /// <param name="fleet">Fleet.</param>
        /// <param name="allied">Allied.</param>
        /// <param name="enemy">Enemy.</param>
        public static bool DetermineAllied (Fleet fleet, Fleet allied, Fleet enemy) {
            return fleet.DetermineRelation (allied) == FactionRelation.Allied && fleet.DetermineRelation (enemy) == FactionRelation.Hostile;
        }
    }
}

