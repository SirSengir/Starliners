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
using BLibrary.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace Starliners.Game.Forces {
    [Serializable]
    public sealed class BattleGrid : SerializableObject {

        #region Constants

        public const int MAX_COLUMNS = 5;
        public const int ROW_COUNT = 9;

        static readonly int[] COLUMN_MAP = new int[] {
            2,
            2, 3, 3, 3, 2, 1, 1, 1,
            2, 3, 4, 4, 4, 4, 4,
            3, 2, 1, 0, 0, 0, 0, 0, 1,

            2, 2, 1, 1, 3, 3,
            2, 2, 3, 3,
            4, 4, 4, 4,
            1, 1, 0, 0,
            0, 0
        };

        #endregion

        public ShipInstance this [int index] {
            get {
                return _grid [index];
            }
            set {
                _grid [index] = value;
            }
        }

        public int MaxCount {
            get {
                return _grid.Length;
            }
        }

        public int ShipCount {
            get {
                int count = 0;
                for (int i = 0; i < _grid.Length; i++) {
                    if (_grid [i] != null) {
                        count++;
                    }
                }
                return count;
            }
        }

        public IReadOnlyList<Salvo> Salvos {
            get {
                return _salvos;
            }
        }

        public IReadOnlyList<Regen> Support {
            get {
                return _regens;
            }
        }

        #region Fields

        [GameData (Remote = true, Key = "Grid")]
        ShipInstance[] _grid = new ShipInstance[MAX_COLUMNS * ROW_COUNT];
        [GameData (Remote = true, Key = "Salvos")]
        List<Salvo> _salvos = new List<Salvo> ();
        [GameData (Remote = true, Key = "Support")]
        List<Regen> _regens = new List<Regen> ();
        [GameData (Remote = true, Key = "Queued")]
        Queue<PendingShip> _queued = new Queue<PendingShip> ();

        #endregion

        public BattleGrid (IWorldAccess access)
            : base (access) {
        }

        #region Serialization

        public BattleGrid (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        void Push (ShipInstance ship) {
            if (ship == null) {
                throw new ArgumentNullException ();
            }

            ship.LastJoined = Access.Clock.Ticks;
            for (int i = 0; i < _grid.Length; i++) {
                if (_grid [i] != null) {
                    continue;
                }

                _grid [i] = ship;
                return;
            }

            throw new SystemException ("Cannot add the given ship, since the grid is already full.");
        }

        /// <summary>
        /// Removes destroyed ships from the battle.
        /// </summary>
        public void CleanupHulks () {
            // Clean up destroyed hulks from last time.
            for (int i = 0; i < _grid.Length; i++) {
                if (_grid [i] == null) {
                    continue;
                }
                if (_grid [i].State != ShipState.Wreck) {
                    continue;
                }

                Access.GameConsole.Log (Battle.LOG_LEVEL, "Removing destroyed hulk {0}.", _grid [i]);
                RemoveShip (i);
            }
        }

        /// <summary>
        /// Recalls the specified fleet from the battle, removing existing ship instances.
        /// </summary>
        /// <param name="fleet">Fleet.</param>
        public void Recall (Fleet fleet) {
            foreach (Levy levy in fleet.Levies) {
                for (int i = 0; i < _grid.Length; i++) {
                    if (_grid [i] == null) {
                        continue;
                    }
                    if (levy.IsMember (_grid [i])) {
                        RemoveShip (i);
                    }
                }
            }
            VerifyQueue (fleet);
        }

        void RemoveShip (int slot) {
            _grid [slot].OnRemovedFromBattle (_grid [slot].State == ShipState.Wreck);
            _grid [slot] = null;
        }

        /// <summary>
        /// Has the grid fire on the opposing side, returning the shots.
        /// </summary>
        public List<Salvo> Fire (int timespan) {
            List<Salvo> salvos = new List<Salvo> ();
            for (int i = 0; i < _grid.Length; i++) {
                if (_grid [i] == null) {
                    continue;
                }

                for (int k = 0; k < DamageKinds.VALID_VALUES.Length; k++) {
                    Volley volley = _grid [i].Fire (DamageKinds.VALID_VALUES [k]);
                    if (volley.Damage > 0) {
                        salvos.Add (new Salvo (Access.Clock.Ticks + Access.Rand.Next (timespan), i, volley, _grid [i].Origin.Owner.Colours.Weapons));
                    }
                }
            }

            return salvos;
        }

        /// <summary>
        /// Has the ships on the grid absorb the enemy fire.
        /// </summary>
        /// <param name="volleys">Volleys.</param>
        public void ReceiveFire (List<Salvo> salvos, int timespan) {

            _salvos.Clear ();

            var candidates = _grid.Select ((ship, index) => new {ship, index, column = COLUMN_MAP [index]}).Where (p => p.ship != null && p.ship.State != ShipState.Wreck).ToList ();
            List<int> distribution = new List<int> ();
            RegenDistribution (distribution, candidates.Select (p => p.column));

            foreach (Salvo salvo in salvos) {
                // Abort if we run out of targets
                if (candidates.Count <= 0) {
                    break;
                }

                // Retrieve a random target and apply the volley
                int column = distribution [Access.Rand.Next (distribution.Count)];
                var target = candidates.Where (p => p.column == column).OrderBy (p => Access.Rand.Next ()).First ();
                DamageReport damage = target.ship.AbsorbVolley (salvo.Shot);
                salvo.RegisterHit (target.index, damage, damage.Type.HasFlag (HitType.Final) ? target.ship.ShipClass.Loot : 0);

                // Record the damage
                _salvos.Add (salvo);

                // Skip ineffectual volleys
                if (salvo.Damage.NoEffect) {
                    Access.GameConsole.Log (Battle.LOG_LEVEL, "NO DAMAGE\n\t\t{0}\n\t\t{1}.", salvo.Shot, target);
                    continue;
                }

                // If the ship was destroyed, we need to remove it from the list of possible targets.
                if (target.ship.State == ShipState.Wreck) {
                    // Remove as valid candidate.
                    candidates.Remove (target);
                    // Reset the distribution list.
                    RegenDistribution (distribution, candidates.Select (p => p.column));
                    Access.GameConsole.Log (Battle.LOG_LEVEL, "DESTROYED\n\t\t{0}\n\t\t{1}\n\t\t{2}.", salvo.Shot, salvo.Damage, target);
                } else {
                    Access.GameConsole.Log (Battle.LOG_LEVEL, "DAMAGED\n\t\t{0}\n\t\t{1}\n\t\t{2}.", salvo.Shot, target.ship, salvo.Damage);
                }
            }

        }

        public void DoSupport (int timespan) {
            _regens.Clear ();
            List<Regen> regens = new List<Regen> ();

            // Collect the support "shots"
            for (int i = 0; i < _grid.Length; i++) {
                if (_grid [i] == null) {
                    continue;
                }

                for (int k = 0; k < StructureLayers.VALID_VALUES.Length; k++) {
                    int support = _grid [i].Support (StructureLayers.VALID_VALUES [k]);
                    if (support > 0) {
                        regens.Add (new Regen (Access.Clock.Ticks + Access.Rand.Next (timespan), i, support, StructureLayers.VALID_VALUES [k]));
                    }
                }
            }

            // Distribute support to ships requiring it.
            var candidates = _grid.Select ((ship, index) => new {ship, index, column = COLUMN_MAP [index]}).Where (p => p.ship != null && p.ship.State != ShipState.Wreck).ToList ();
            foreach (Regen regen in regens) {
                var target = candidates.OrderBy (p => Access.Rand.Next ()).Where (p => p.ship.RequiresHealing (regen.Layer) > 0).FirstOrDefault ();
                // No proper target found.
                if (target == null) {
                    continue;
                }
                regen.RegisterEffected (target.index, target.ship.RequiresHealing (regen.Layer));
                target.ship.ApplyHealing (regen);
                _regens.Add (regen);
            }
        }

        public void Reenforce (List<Fleet> supply) {
            int available = MaxCount - ShipCount;
            if (available <= 0) {
                return;
            }

            int maxreenforce = 4 < available ? 4 : available;
            int reenforced = 0;
            while (_queued.Count > 0 && reenforced < maxreenforce) {
                ShipInstance ship = RecruitShip (_queued.Dequeue ());
                if (ship != null) {
                    Push (ship);
                    reenforced++;
                }
            }

            while (reenforced < maxreenforce) {
                PendingShip pending = GetPending (supply, ShipSizes.VALID_VALUES [Access.Rand.Next (ShipSizes.VALID_VALUES.Length)], ShipRole.Combat);
                if (pending != null) {
                    _queued.Enqueue (pending);
                }
                reenforced++;
            }
            /*
            int[] gridcomposition = grid.GetCurrentComposition ();
            int[] fleetcomposition = GetSideComposition (supply);
            int[] expected = grid.GetMatchingComposition (supply.Sum (p => p.ShipCount), fleetcomposition);

            for (int i = 0; i < gridcomposition.Length; i++) {
                if (gridcomposition [i] >= expected [i]) {
                    continue;
                }

                ShipSize size = (ShipSize)(i + 1);
                int required = expected [i] - gridcomposition [i];
                Access.GameConsole.Log (LOG_LEVEL, "Looking to reenforce with {0} vessels of {1} size.", required, size);
                for (int j = 0; j < required; j++) {
                    ShipInstance ship = GetMatchingShip (supply, size);
                    if (ship != null) {
                        Access.GameConsole.Log (LOG_LEVEL, "Pushing ship instance {0} to grid.", ship);
                        grid.Push (ship);
                    } else {
                        Access.GameConsole.Log (LOG_LEVEL, "Failed to fetch a ship of size {0}.", size);
                    }
                }
            }
            */
        }

        ShipInstance RecruitShip (PendingShip pending) {
            Levy levy = (Levy)Access.States.Values.Where (p => p.Serial == pending.Levy).FirstOrDefault ();
            return levy.Ships.ContainsKey (pending.Ship) ? levy.Ships [pending.Ship] : null;
        }

        PendingShip GetPending (List<Fleet> supply, ShipSize size, ShipRole role) {
            foreach (Fleet fleet in supply) {
                foreach (Levy levy in fleet.Levies) {
                    foreach (ShipInstance ship in levy.Ships.Values.Where(p => p.State != ShipState.UnderConstruction)) {
                        // Must match size and role
                        if (ship.ShipClass.Size != size || ship.ShipClass.Role != role) {
                            continue;
                        }
                        // Must not be deployed yet.
                        if (_grid.Any (p => p != null && p.Serial == ship.Serial)) {
                            continue;
                        }
                        return new PendingShip (levy, ship);
                    }

                }
            }

            return null;
        }

        void VerifyQueue (Fleet fleet) {
            if (_queued.Count <= 0) {
                return;
            }
            Queue<PendingShip> cleaned = new Queue<PendingShip> ();
            while (_queued.Count > 0) {
                PendingShip pending = _queued.Dequeue ();
                bool invalid = false;
                foreach (Levy levy in fleet.Levies) {
                    if (levy.Serial != pending.Levy) {
                        continue;
                    }
                    /*
                    if (levy.Squadrons.ContainsKey (pending.Squadron)) {
                        invalid = true;
                        break;
                    }*/
                }
                if (!invalid) {
                    cleaned.Enqueue (pending);
                }
            }

            _queued = cleaned;
        }

        void RegenDistribution (List<int> distribution, IEnumerable<int> columns) {
            distribution.Clear ();
            for (int i = 0; i < MAX_COLUMNS; i++) {
                int count = columns.Count (p => p == i);
                if (count <= 0) {
                    continue;
                }
                for (int j = 0; j < (i + 1) * count; j++) {
                    distribution.Add (i);
                }
            }
        }

        public int[] GetMatchingComposition (int max, int[] composition) {
            int[] counts = new int[ShipSizes.VALUES.Length - 1];

            bool valid = false;
            float[] fracts = new float[composition.Length];
            for (int i = 0; i < fracts.Length; i++) {
                fracts [i] = (float)composition [i] / max;
                if (fracts [i] > 0) {
                    valid = true;
                }
            }
            // Abort if not a single ship can be supplied.
            if (!valid) {
                return counts;
            }

            int used = 0;
            for (int i = 0; i < fracts.Length; i++) {
                counts [i] = (int)(fracts [i] * MaxCount);
                used += counts [i];
            }

            int x = 0;
            while (used < MaxCount) {
                if (fracts [x] > 0) {
                    counts [x]++;
                    used++;
                }
                x++;
                if (x >= counts.Length) {
                    x = 0;
                }
            }

            return counts;
        }

        /// <summary>
        /// Gets an array indicating the amount of ships by ship size in this fleet.
        /// </summary>
        /// <returns>The fleet composition.</returns>
        public int[] GetCurrentComposition () {
            int[] counts = new int[ShipSizes.VALUES.Length - 1];
            for (int i = 0; i < _grid.Length; i++) {
                if (_grid [i] == null) {
                    continue;
                }
                counts [(int)_grid [i].ShipClass.Size - 1]++;
            }
            return counts;
        }


    }
}

