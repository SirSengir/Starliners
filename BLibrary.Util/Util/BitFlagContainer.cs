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
using System.Collections;
using System.Runtime.Serialization;
using System.Text;

namespace BLibrary.Util {

    [Serializable]
    public sealed class BitFlagContainer<T> : ISerializable, IEquatable<BitFlagContainer<T>> where T : struct, IConvertible, IComparable, IFormattable {

        public bool this [int flag] {
            get {
                return flag < _flags.Length ? _flags [flag] : false;
            }
            set {
                if (flag >= _flags.Length) {
                    ResetLength (flag + 1);
                }
                _flags [flag] = value;
            }
        }

        public int Length {
            get {
                return _flags.Length;
            }
        }

        public int CountSetFlags {
            get {
                int count = 0;
                for (int i = 0; i < _flags.Length; i++) {
                    if (_flags [i]) {
                        count++;
                    }
                }
                return count;
            }
        }

        #region Fields

        BitArray _flags;

        #endregion

        #region Constructor

        public BitFlagContainer ()
            : this (Enum.GetValues (typeof(T)).Length) {
        }

        public BitFlagContainer (int length) {
            _flags = new BitArray (length);
        }

        #endregion

        #region Serialization

        public BitFlagContainer (SerializationInfo info, StreamingContext context) {
            _flags = (BitArray)info.GetValue ("Flags", typeof(BitArray));
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Flags", _flags);
        }

        #endregion

        void ResetLength (int newLength) {
            BitArray converted = new BitArray (newLength);
            for (int i = 0; i < _flags.Length; i++) {
                converted [i] = _flags [i];
            }
            _flags = converted;
        }

        public BitFlagContainer<T> Copy () {
            BitFlagContainer<T> copy = new BitFlagContainer<T> (_flags.Length);
            copy |= this;
            return copy;
        }

        public override int GetHashCode () {
            return _flags.GetHashCode ();
        }

        public override bool Equals (object obj) {
            return Equals (obj as BitFlagContainer<T>);
        }

        public bool Equals (BitFlagContainer<T> other) {
            if (object.ReferenceEquals (other, null)) {
                return false;
            }

            for (int i = 0; i < _flags.Length; i++) {
                if (_flags [i] != other._flags [i]) {
                    return false;
                }
            }

            return true;
        }

        public override string ToString () {
            StringBuilder builder = new StringBuilder ();
            for (int i = 0; i < _flags.Length; i++) {
                if (_flags [i]) {
                    if (builder.Length > 0) {
                        builder.Append (", ");
                    }
                    builder.Append (((T)(object)(i)).ToString ());
                }
            }
            return builder.ToString ();
        }

        #region Operators

        public static bool operator == (BitFlagContainer<T> lhs, BitFlagContainer<T> rhs) {
            return object.ReferenceEquals (lhs, rhs) ||
            !object.ReferenceEquals (lhs, null) && lhs.Equals (rhs);
        }

        public static bool operator != (BitFlagContainer<T> lhs, BitFlagContainer<T> rhs) {
            return !(lhs == rhs);
        }

        public static BitFlagContainer<T> operator | (BitFlagContainer<T> lhs, BitFlagContainer<T> rhs) {
            lhs._flags = lhs._flags.Or (rhs._flags);
            return lhs;
        }

        public static BitFlagContainer<T> operator & (BitFlagContainer<T> lhs, BitFlagContainer<T> rhs) {
            lhs._flags = lhs._flags.And (rhs._flags);
            return rhs;
        }

        public static BitFlagContainer<T> operator | (BitFlagContainer<T> lhs, int rhs) {
            if (rhs == 0) {
                return lhs;
            }
            lhs._flags.Set (rhs, true);
            return lhs;
        }

        #endregion
    }
}
