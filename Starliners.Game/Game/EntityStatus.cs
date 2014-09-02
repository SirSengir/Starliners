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

namespace Starliners.Game {

    [Serializable]
    public sealed class EntityStatus : ISerializable, IEquatable<EntityStatus> {
        public static readonly EntityStatus NONE = new EntityStatus ();

        public enum StatusLevel : byte {
            None = 0,
            Warning = 1,
            Error = 2
        }

        public enum StatusSymbol : byte {
            None,
            Error,
            Sleep,
            Temperature,
            Rain
        }

        public string Category {
            get;
            private set;
        }

        public StatusLevel Level { get; private set; }

        public StatusSymbol Symbol { get; set; }

        public string Message { get; private set; }

        #region Constructor

        public EntityStatus (StatusLevel level, StatusSymbol symbol, string message)
            : this (level, symbol, message, message) {
        }

        public EntityStatus (StatusLevel level, StatusSymbol symbol, string message, string category) {
            Level = level;
            Symbol = symbol;
            Message = message;
            Category = category;
        }

        public EntityStatus () {
            Level = StatusLevel.None;
        }

        #endregion

        #region Serialization

        public EntityStatus (SerializationInfo info, StreamingContext context) {
            Level = (StatusLevel)info.GetValue ("Level", typeof(StatusLevel));
            Symbol = (StatusSymbol)info.GetValue ("Symbol", typeof(StatusSymbol));
            Message = info.GetString ("Message");
            Category = info.GetString ("Category");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Level", Level, typeof(StatusLevel));
            info.AddValue ("Symbol", Symbol, typeof(StatusSymbol));
            info.AddValue ("Message", Message);
            info.AddValue ("Category", Category);
        }

        #endregion

        public override bool Equals (object obj) {
            EntityStatus other = obj as EntityStatus;
            if (other == null) {
                return false;
            }
            return Level == other.Level && string.Equals (Message, other.Message) && string.Equals (Category, other.Category);
        }

        public bool Equals (EntityStatus other) {
            if (other == null) {
                return false;
            }
            return Level == other.Level && string.Equals (Message, other.Message) && string.Equals (Category, other.Category);
        }

        public override int GetHashCode () {
            return Level.GetHashCode () ^ Symbol.GetHashCode () ^ Message.GetHashCode ();
        }

        public override string ToString () {
            return string.Format ("[{0}]: {1}", Level, Message);
        }
    }
}

