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
using BLibrary.Serialization;
using System.Collections.Generic;

namespace Starliners.Game {
    [Serializable]
    public sealed class ScoreKeeper : SerializableObject, IUpdateIndicator {
        #region Constants

        public const string SCORE_COMBAT_SHIP_DESTROYED = "CombatShipDestroyed";
        public const string SCORE_COMBAT_BATTLES_WON = "CombatBattlesWon";
        public const string SCORE_CONQUEST_PLANETS = "ConquestPlanets";
        public const string SCORE_SURVIVAL_TIME = "SurvivalTime";

        public static readonly ScoreSlot[] INFO_SLOTS = new ScoreSlot[] {
            new ScoreSlot ("accumulated", SCORE_SURVIVAL_TIME),
            new ScoreSlot ("accumulated", SCORE_COMBAT_SHIP_DESTROYED)
        };

        #endregion

        #region Classes

        [Serializable]
        public sealed class TransactionRecord : ISerializable {
            public readonly long TimeStamp;
            public readonly string Category;
            public readonly int Amount;

            public TransactionRecord (IWorldAccess access, string category, int amount) {
                TimeStamp = access.Clock.Ticks;
                Category = category;
                Amount = amount;
            }

            #region Serialization

            public TransactionRecord (SerializationInfo info, StreamingContext context) {
                TimeStamp = info.GetInt64 ("TimeStamp");
                Category = info.GetString ("Category");
                Amount = info.GetInt32 ("Amount");
            }

            public void GetObjectData (SerializationInfo info, StreamingContext context) {
                info.AddValue ("TimeStamp", TimeStamp);
                info.AddValue ("Category", Category);
                info.AddValue ("Amount", Amount);
            }

            #endregion
        }

        public sealed class ScoreSlot {
            public readonly string Category;
            public readonly string Attribute;

            readonly string _format;

            public ScoreSlot (string category, string attribute)
                : this (category, attribute, "{0:0}") {
            }

            public ScoreSlot (string category, string attribute, string format) {
                Category = category;
                Attribute = attribute;
                _format = format;
            }

            public bool IsDisplayed (ScoreKeeper score) {
                return score._categorized.ContainsKey (Attribute) && score._categorized [Attribute] > 0;
            }

            public string ToString (ScoreKeeper score) {
                return string.Format (_format, score._categorized [Attribute]);
            }
        }

        #endregion

        #region Properties

        public long LastUpdated {
            get;
            private set;
        }

        [GameData (Key = "Score")]
        public int Score {
            get;
            private set;
        }

        /// <summary>
        /// Gets the transaction records.
        /// </summary>
        /// <value>The records.</value>
        public IReadOnlyCollection<TransactionRecord> Records {
            get {
                return _records;
            }
        }

        #endregion

        List<TransactionRecord> _records = new List<TransactionRecord> ();
        [GameData (Remote = true, Key = "Categorized")]
        Dictionary<string, int> _categorized = new Dictionary<string, int> ();

        #region Constructor

        public ScoreKeeper (IWorldAccess access)
            : base (access, access.GetNextSerial ()) {
        }

        #endregion

        #region Serialization

        public ScoreKeeper (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        void MarkUpdated () {
            LastUpdated = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Increases the funds of this player.
        /// </summary>
        /// <param name="funds"></param>
        public void Transfer (string category, int score) {
            Score += score;
            _records.Add (new TransactionRecord (Access, category, score));
            if (!_categorized.ContainsKey (category)) {
                _categorized [category] = 0;
            }
            _categorized [category] += score;

            MarkUpdated ();
        }
    }
}

