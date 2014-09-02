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
using Starliners.Game.Planets;
using Starliners.Game.Forces;
using BLibrary.Util;
using System.Collections.Generic;

namespace Starliners.Gui.Interface {
    sealed class ContainerPlanet : Container {

        public override bool MustClose {
            get {
                return base.MustClose || _planet.IsDead;
            }
            set {
                base.MustClose = value;
            }
        }

        EntityPlanet _planet;
        //Player _player;

        public ContainerPlanet (ushort guiId, Player player, EntityPlanet planet)
            : base (guiId) {
            Precedence = "planet";
            _planet = planet;
            //_player = player;

            ActionHandling += LevyRaise;
            ActionHandling += BuildingSell;
            ActionHandling += BuildingPurchase;
            ActionHandling += FleetMerge;

            UpdateFragment (Constants.FRAGMENT_GUI_HEADER, _planet.PlanetData.FullName);
            UpdateFragment (KeysFragments.PLANET_SERIAL, _planet.Serial);

            Subscribe (_planet);
            Subscribe (_planet.PlanetData);
            Subscribe (_planet.PlanetData.Levy);
            OnChanged ();
        }

        protected override void Refresh (object sender, EventArgs e) {
            base.Refresh (sender, e);
            UpdateFragment (KeysFragments.PLANET_ATTRIBUTES, _planet.PlanetData.Attributes);
            UpdateFragment (KeysFragments.PLANET_LEVY_STATUS, _planet.PlanetData.Levy.State);
            UpdateFragment (KeysFragments.PLANET_LEVY_SERIAL, _planet.PlanetData.Levy.Serial);
            UpdateFragment (KeysFragments.PLANET_LOYALITY, (float)_planet.PlanetData.Loyality / Planet.LOYALITY_MAX);
            UpdateFragment (KeysFragments.PLANET_SHIPS, _planet.PlanetData.Levy.Ships.Values.ToList ());
            UpdateFragment (KeysFragments.PLANET_SQUADS, _planet.PlanetData.Levy.SquadInfos);
            UpdateFragment (KeysFragments.PLANET_FLEETS, _planet.PlanetData.Orbit.Fleets.Select (p => p.Serial).ToList ());
            UpdateFragment (KeysFragments.PLANET_BUILDINGS, _planet.PlanetData.GetBuildingList ());
        }

        public override PrecedenceBehaviour DeterminePrecedence (Container other) {
            ContainerPlanet compare = other as ContainerPlanet;
            if (compare == null) {
                return PrecedenceBehaviour.None;
            }
            if (base.DeterminePrecedence (other) == PrecedenceBehaviour.None) {
                return PrecedenceBehaviour.None;
            }

            return _planet.Serial == compare._planet.Serial ? PrecedenceBehaviour.KeepExisting : PrecedenceBehaviour.ReplaceExisting;
        }

        void LevyRaise (Player player, Container container, string key, Payload payload) {
            if (!KeysActions.PLANET_LEVY_RAISE.Equals (key)) {
                return;
            }
            if (!player.HasPermission (_planet.Owner, PermissionKeys.PLANET_MANAGMENT)) {
                return;
            }
            _planet.RaiseLevy ();
        }

        void BuildingSell (Player player, Container container, string key, Payload payload) {
            if (!KeysActions.PLANET_BUILDING_SELL.Equals (key)) {
                return;
            }
            if (!player.HasPermission (_planet.Owner, PermissionKeys.PLANET_MANAGMENT)) {
                return;
            }
            Improvement improvement = _planet.Access.RequireAsset<Improvement> (payload.GetValue<ulong> (0));
            _planet.PlanetData.RemoveImprovement (improvement);
            player.Bookkeeping.Transfer ("building.sold", improvement.Cost.DetermineSellPrice (), true);
        }

        void BuildingPurchase (Player player, Container container, string key, Payload payload) {
            if (!KeysActions.PLANET_BUILDING_BUY.Equals (key)) {
                return;
            }
            if (!player.HasPermission (_planet.Owner, PermissionKeys.PLANET_MANAGMENT)) {
                return;
            }
            Improvement improvement = _planet.Access.RequireAsset<Improvement> (payload.GetValue<ulong> (0));
            int cost = improvement.Cost.DeterminePurchasePrice (_planet.PlanetData.CountImprovement (improvement));
            if (!player.Bookkeeping.CanBankroll (cost)) {
                return;
            }
            player.Bookkeeping.Debit ("building.purchase", cost, true);
            _planet.PlanetData.AddImprovement (improvement);
        }

        void FleetMerge (Player player, Container container, string key, Payload payload) {
            if (!KeysActions.PLANET_FLEET_MERGE.Equals (key)) {
                return;
            }

            // Determine the first fleet we can manage.
            Fleet target = null;
            foreach (Fleet fleet in _planet.Orbit.Fleets) {
                if (!player.HasPermission (fleet.Owner, PermissionKeys.FLEET_MANAGMENT)) {
                    continue;
                }
                if (fleet.State != FleetState.Available) {
                    continue;
                }
                target = fleet;
                break;
            }
            if (target == null) {
                player.SendChat (Colour.Crimson, "no_mergeable_fleets");
                return;
            }

            // Now merge in fleets which have the same owner.
            List<Fleet> mergeable = new List<Fleet> ();
            foreach (Fleet fleet in _planet.Orbit.Fleets) {
                if (fleet == target) {
                    continue;
                }
                if (fleet.Owner != target.Owner) {
                    continue;
                }
                if (fleet.State != FleetState.Available) {
                    continue;
                }
                mergeable.Add (fleet);
            }

            if (mergeable.Count <= 0) {
                player.SendChat (Colour.Crimson, "no_mergeable_fleets");
                return;
            }
            foreach (Fleet fleet in mergeable) {
                target.Incorporate (fleet);
            }
        }
    }
}

