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
using BLibrary.Serialization;
using System.Runtime.Serialization;
using System.Collections.Generic;
using BLibrary.Util;

namespace Starliners.Game {

    [Serializable]
    public sealed class Bookkeeping : SerializableObject, IUpdateIndicator {
        #region Classes

        [Serializable]
        public sealed class TransactionRecord : ISerializable {
            public readonly long TimeStamp;
            public readonly string Category;
            public readonly decimal Amount;
            public readonly bool Signal;

            public TransactionRecord (IWorldAccess access, string category, decimal amount, bool signal) {
                TimeStamp = access.Clock.Ticks;
                Category = category;
                Amount = amount;
                Signal = signal;
            }

            #region Serialization

            public TransactionRecord (SerializationInfo info, StreamingContext context) {
                TimeStamp = info.GetInt64 ("TimeStamp");
                Category = info.GetString ("Category");
                Amount = info.GetDecimal ("Amount");
                Signal = info.GetBoolean ("Signal");
            }

            public void GetObjectData (SerializationInfo info, StreamingContext context) {
                info.AddValue ("TimeStamp", TimeStamp);
                info.AddValue ("Category", Category);
                info.AddValue ("Amount", Amount);
                info.AddValue ("Signal", Signal);
            }

            #endregion
        }

        #endregion

        #region Properties

        public long LastUpdated {
            get;
            private set;
        }

        [GameData (Key = "Wealth")]
        public decimal Funds {
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

        [GameData (Key = "Records")]
        List<TransactionRecord> _records = new List<TransactionRecord> ();

        #region Constructor

        public Bookkeeping (IWorldAccess access, decimal funds)
            : base (access, access.GetNextSerial ()) {
            Funds = funds;
        }

        #endregion

        #region Serialization

        public Bookkeeping (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        void MarkUpdated () {
            LastUpdated = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Indicates whether the player can bankroll the given amount.
        /// </summary>
        /// <param name="funds"></param>
        /// <returns></returns>
        public bool CanBankroll (decimal funds) {
            return Credited (funds) >= funds;
        }

        /// <summary>
        /// Returns funds or the max amount of credits this player can afford.
        /// </summary>
        /// <param name="funds"></param>
        /// <returns></returns>
        decimal Credited (decimal funds) {
            if (Funds < funds) {
                return 0;
            }
            return funds;
        }

        /// <summary>
        /// Returns and debits funds or the max amount of credits this player can afford.
        /// </summary>
        /// <param name="funds"></param>
        /// <returns></returns>
        public decimal Debit (string category, decimal funds, bool signal) {
            funds = Credited (funds);
            Funds -= funds;
            _records.Add (new TransactionRecord (Access, category, -funds, signal));
            MarkUpdated ();
            return funds;
        }

        /// <summary>
        /// Increases the funds of this player.
        /// </summary>
        /// <param name="funds"></param>
        public void Transfer (string category, decimal funds, bool signal) {
            Funds += funds;
            _records.Add (new TransactionRecord (Access, category, funds, signal));
            MarkUpdated ();
        }
    }
}

