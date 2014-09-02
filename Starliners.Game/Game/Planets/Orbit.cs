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
using System.Runtime.Serialization;
using BLibrary.Serialization;
using Starliners.Game.Forces;

namespace Starliners.Game.Planets {
    [Serializable]
    public sealed class Orbit : SerializableObject {

        public IReadOnlyList<Fleet> Fleets {
            get {
                return _fleets;
            }
        }

        [GameData (Remote = true, Key = "Battle")]
        public Battle Battle {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Fleets")]
        List<Fleet> _fleets = new List<Fleet> ();
        [GameData (Remote = true, Key = "BattleCount")]
        int _battleCount;

        public Orbit (IWorldAccess access)
            : base (access) {
        }

        #region Serialization

        public Orbit (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        /// <summary>
        /// Orbits the specified fleet around this planet.
        /// </summary>
        /// <param name="fleet">Fleet.</param>
        public void Insert (Planet planet, Fleet fleet) {
            _fleets.Add (fleet);
            InitiateOrJoinBattle (planet, fleet);
        }

        /// <summary>
        /// Removes the specified fleet from the orbit.
        /// </summary>
        /// <param name="fleet">Fleet.</param>
        public void Leave (Planet planet, Fleet fleet) {
            _fleets.Remove (fleet);
            if (Battle != null) {
                Battle.RetreatFleet (fleet);
            }
        }

        public void CheckBattle () {
            if (Battle != null && Battle.Resolution != BattleResolution.None) {
                Access.GameConsole.Debug ("Removing battle {0} from orbit.", Battle);
                Battle = null;
            }
        }

        void InitiateOrJoinBattle (Planet planet, Fleet initiator) {
            // Only ever one battle at a planet.
            if (Battle != null) {
                Battle.JoinIfPossible (initiator);
                return;
            }

            // Initiate battle if necessary
            foreach (Fleet existing in _fleets) {
                if (initiator.DetermineRelation (existing) != FactionRelation.Hostile) {
                    continue;
                }

                _battleCount++;
                Battle = new Battle (Access, string.Format ("Battle of {0} #{1}", planet.FullName, _battleCount), planet.Location, GetAllies (initiator, existing), GetAllies (existing, initiator));
                Access.Controller.QueueState (Battle);
                break;
            }

        }

        IList<Fleet> GetAllies (Fleet combatant, Fleet exclusion) {
            List<Fleet> party = new List<Fleet> ();
            foreach (Fleet fleet in _fleets) {
                if (Battle.DetermineAllied (fleet, combatant, exclusion)) {
                    party.Add (fleet);
                }
            }
            return party;
        }

        /// <summary>
        /// Determines whether this instance has fleets with a matching relation to the specified faction.
        /// </summary>
        /// <returns><c>true</c> if this instance has related fleets the specified relation faction; otherwise, <c>false</c>.</returns>
        /// <param name="relation">Relation.</param>
        /// <param name="faction">Faction.</param>
        public bool HasRelatedFleets (FactionRelation relation, Faction faction) {
            for (int i = 0; i < _fleets.Count; i++) {
                if (_fleets [i].DetermineRelation (faction) == relation) {
                    return true;
                }
            }

            return false;
        }

    }
}

