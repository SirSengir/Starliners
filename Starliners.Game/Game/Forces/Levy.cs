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
using System.Runtime.Serialization;
using System.Collections.Generic;
using BLibrary.Serialization;
using System.Linq;
using BLibrary.Resources;
using BLibrary.Network;
using Starliners.Network;

namespace Starliners.Game.Forces {

    [Serializable]
    public sealed class Levy : StateObject {
        #region Constants

        const string NAME_PREFIX = "levy";

        #endregion

        #region Classes

        [Serializable]
        public sealed class SquadInfo : ISerializable {

            public int Cap {
                get;
                set;
            }

            public float LastSupply {
                get;
                set;
            }

            public SquadInfo () {
            }

            public SquadInfo (int cap) {
                Cap = cap;
            }

            public SquadInfo (SerializationInfo info, StreamingContext context) {
                Cap = info.GetInt32 ("Cap");
                LastSupply = info.GetSingle ("LastSupply");
            }

            public void GetObjectData (SerializationInfo info, StreamingContext context) {
                info.AddValue ("Cap", Cap);
                info.AddValue ("LastSupply", LastSupply);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this levy is raised.
        /// </summary>
        /// <value><c>true</c> if this instance is raised; otherwise, <c>false</c>.</value>
        [GameData (Key = "IsRaised")]
        public bool IsRaised {
            get;
            private set;
        }

        public LevyState State {
            get {
                if (IsDepleted ()) {
                    return LevyState.Depleted;
                }
                if (IsRaised) {
                    return _isEngaged ? LevyState.Engaged : LevyState.Raised;
                }
                return LevyState.Available;
            }
        }

        /// <summary>
        /// Gets the levy's origin.
        /// </summary>
        /// <value>The origin.</value>
        [GameData (Remote = true, Key = "Origin")]
        public ILevyProvider Origin {
            get;
            private set;
        }

        public IReadOnlyDictionary<ulong, ShipInstance> Ships {
            get {
                return _ships;
            }
        }

        public IReadOnlyDictionary<ulong, SquadInfo> SquadInfos {
            get {
                return _squadinfos;
            }
        }

        /// <summary>
        /// Gets a value indicating the levy's weight.
        /// </summary>
        /// <remarks>Calculated as the sum of (squadron.Strength * ShipSize).</remarks>
        /// <value>The weight.</value>
        public int Weight {
            get {
                int weight = 0;
                foreach (ShipInstance ship in _ships.Values) {
                    weight += (int)ship.ShipClass.Size;
                }
                return weight;
            }
        }

        public string Description {
            get {
                return Localization.Instance ["levy_name", Origin.Name];
            }
        }

        #endregion

        [GameData (Key = "IsEngaged")]
        bool _isEngaged;
        [GameData (Remote = true, Key = "Ships")]
        Dictionary<ulong, ShipInstance> _ships = new Dictionary<ulong, ShipInstance> ();
        Dictionary<ulong, SquadInfo> _squadinfos = new Dictionary<ulong, SquadInfo> ();

        public Levy (IWorldAccess access, string name, ILevyProvider origin)
            : base (access, Utils.BuildName (NAME_PREFIX, name)) {
            //IsTickable = true;
            Origin = origin;
        }

        #region Serialization

        public Levy (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        bool IsDepleted () {
            return _ships.Count <= 0;
        }

        public IList<string> GetInformation (Player player) {
            List<string> info = new List<string> ();

            /*
            int[] current = new int[ShipSizes.VALUES.Length - 1];
            int[] max = new int[ShipSizes.VALUES.Length - 1];
            foreach (Squadron squadron in Squadrons.Values) {
                current [(int)squadron.ShipClass.Size - 1] += (int)squadron.Strength;
                max [(int)squadron.ShipClass.Size - 1] += squadron.Cap;
            }

            for (int i = 0; i < ShipSizes.VALUES.Length - 1; i++) {
                info.Add (string.Format ("{0}: {1}/{2}", Localization.Instance [string.Format ("sclass_{0}", ShipSizes.VALUES [i + 1].ToString ().ToLowerInvariant ())], current [i], max [i]));
            }*/

            return info;
        }

        /// <summary>
        /// Stands the levy down without dissolving it.
        /// </summary>
        public void StandDown () {
            IsRaised = false;
            _isEngaged = false;
            MarkUpdated (UpdateMarker.Update1);
        }

        /// <summary>
        /// Destroys the ships in this levy, essentially resetting it.
        /// </summary>
        public void Reset () {
            // Marks existing ships as destroyed, to remove them from battles.
            foreach (var entry in _ships) {
                entry.Value.Destroy ();
            }
            _ships.Clear ();
        }

        /// <summary>
        /// Disbands the levy and removes it from the game.
        /// </summary>
        /// <remarks>Must not be called on planetary levies.</remarks>
        public void Disband () {
            StandDown ();
            IsDead = true;
        }

        /// <summary>
        /// Raises the levy, creating a new fleet.
        /// </summary>
        /// <param name="port">Port.</param>
        public Fleet Raise (INavPoint port) {
            IsRaised = true;
            Fleet fleet = new Fleet (Access, string.Format ("levyfleet{0}", Serial), port, Origin.Owner, new List<Levy> { this });
            Access.Controller.QueueState (fleet);
            MarkUpdated (UpdateMarker.Update1);
            return fleet;
        }

        public Fleet Raise (Vect2d location, IFleetBacker backer) {
            IsRaised = true;
            Fleet fleet = new Fleet (Access, string.Format ("levyfleet{0}", Serial), location, backer, new List<Levy> { this });
            Access.Controller.QueueState (fleet);
            MarkUpdated (UpdateMarker.Update1);
            return fleet;
        }

        /// <summary>
        /// Flags the levy as engaged in combat or not.
        /// </summary>
        /// <param name="flag">If set to <c>true</c> flag.</param>
        public void FlagEngaged (bool flag) {
            _isEngaged = flag;
            MarkUpdated ();
        }

        /// <summary>
        /// Resets the levy to its maximum strength.
        /// </summary>
        public void Rejuvenate () {
            Reenforce (true);
        }

        public void ReenforcementTick () {
            // Reenforce this levy if unraised.
            if (!IsDead && !_isEngaged) {
                Reenforce (false);
            }
        }

        /// <summary>
        /// Reenforce the levy.
        /// </summary>
        /// <param name="maximize">If set to <c>true</c> maximize.</param>
        void Reenforce (bool maximize) {

            Dictionary<ShipSize, List<ShipClass>> squadronSetup = new Dictionary<ShipSize, List<ShipClass>> ();
            IEnumerable<ShipClass> candidates = ShipClass.GetClassesForWorld (Access);

            foreach (ShipSize size in ShipSizes.VALID_VALUES) {
                foreach (ShipClass sclass in candidates.Where(p => p.Size == size && p.NeedsBudget(Origin))) {
                    if (!squadronSetup.ContainsKey (sclass.Size)) {
                        squadronSetup [sclass.Size] = new List<ShipClass> ();
                    }
                    squadronSetup [sclass.Size].Add (sclass);
                }
            }

            // Check squadrons and populate them
            foreach (KeyValuePair<ShipSize, List<ShipClass>> squadConfig in squadronSetup) {
                ShipModifiers modifiers = Origin.CreateShipModifiers (squadConfig.Key);

                // Use the budget for each ship class
                foreach (ShipClass sclass in squadConfig.Value) {
                    int cap = sclass.DetermineMaxLevy (Origin);
                    if (cap <= 0) {
                        NoteSquadInfo (sclass, 0, 0);
                        continue;
                    }
                    int existing = CountExistingShips (sclass);
                    if (existing >= cap) {
                        NoteSquadInfo (sclass, cap - existing, 0);
                        continue;
                    }

                    // Determine levy growth
                    float growth = 0;
                    if (maximize) {
                        growth = cap - existing;
                    } else if (existing < cap) {
                        growth = sclass.DetermineLevyGrowth (Origin);
                        growth = growth <= cap - existing ? growth : cap - existing;
                    }

                    NoteSquadInfo (sclass, cap, growth);
                    // No growth, skip this entry.
                    if (growth <= 0) {
                        continue;
                    }

                    bool updated = false;
                    while (growth > 0) {
                        ShipInstance hull = _ships.Values.FirstOrDefault (p => p.ShipClass == sclass && p.State == ShipState.UnderConstruction);

                        // Create a squadron for this class and modifiers if none exist yet.
                        bool shipAdded = false;
                        if (hull == null) {
                            hull = new ShipInstance (Access, this, sclass, modifiers);
                            Access.GameConsole.Debug ("Creating new ship {0} in levy {1}.", hull, this);
                            SpawnShip (hull);
                            shipAdded = true;
                        } else {
                            hull.ResetProperties (modifiers);
                        }

                        growth = hull.ApplyConstruction (growth);
                        updated = updated || shipAdded || hull.State != ShipState.UnderConstruction;
                    }

                    // Mark as updated if we had a visible change.
                    if (updated) {
                        MarkUpdated (UpdateMarker.Update0);
                    }
                }
            }
        }

        void NoteSquadInfo (ShipClass sclass, int cap, float growth) {
            if (cap > 0) {
                if (!_squadinfos.ContainsKey (sclass.Serial)) {
                    _squadinfos [sclass.Serial] = new SquadInfo ();
                }
                _squadinfos [sclass.Serial].Cap = cap;
                _squadinfos [sclass.Serial].LastSupply = growth;
            } else {
                _squadinfos.Remove (sclass.Serial);
            }
        }

        public void SpawnShip (ShipInstance ship) {
            _ships [ship.Serial] = ship;
        }

        /// <summary>
        /// Counts the existing ships of the given class in the levy.
        /// </summary>
        /// <returns>The existing ships.</returns>
        /// <param name="sclass">Sclass.</param>
        int CountExistingShips (ShipClass sclass) {
            return _ships.Values.Count (p => p.ShipClass == sclass);
        }

        /// <summary>
        /// Determines whether the given squadron is part of this levy.
        /// </summary>
        /// <returns><c>true</c> if this instance is child squadron the specified squadron; otherwise, <c>false</c>.</returns>
        /// <param name="squadron">Squadron.</param>
        public bool IsMember (ShipInstance ship) {
            return _ships.ContainsKey (ship.Serial);
        }

        public void Census (int[] counts) {
            foreach (ShipInstance ship in _ships.Values) {
                counts [(int)ship.ShipClass.Size - 1]++;
            }
        }

        public void OnShipLoss (ShipInstance ship) {
            Origin.OnShipLoss (ship.ShipClass);
            _ships.Remove (ship.Serial);
            MarkUpdated (UpdateMarker.Update0);
        }

        public override void HandleNetworkUpdate (IPacketMarked packet) {
            if (packet.Marker == UpdateMarker.Update0) {
                _ships = ((PacketUpdateStream)packet).DeserializeContent<Dictionary<ulong, ShipInstance>> (Access);
            } else if (packet.Marker == UpdateMarker.Update1) {
                PacketUpdatePayload pack = (PacketUpdatePayload)packet;
                IsRaised = pack.Payload.GetValue<bool> (0);
                _isEngaged = pack.Payload.GetValue<bool> (1);
            } else {
                base.HandleNetworkUpdate (packet);
            }
        }

        public override IPacketMarked GetUpdatePacket () {
            if (UpdateSchedule.HasFlag (UpdateMarker.Update0)) {
                UnscheduleUpdate (UpdateMarker.Update0);
                return new PacketUpdateStream (PacketId.UpdateStream, this, UpdateMarker.Update0, _ships);
            } else if (UpdateSchedule.HasFlag (UpdateMarker.Update1)) {
                UnscheduleUpdate (UpdateMarker.Update1);
                return new PacketUpdatePayload (PacketId.UpdatePayload, this, UpdateMarker.Update1, IsRaised, _isEngaged);
            } else {
                return base.GetUpdatePacket ();
            }
        }
    }
}

