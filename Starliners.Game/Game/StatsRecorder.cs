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
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BLibrary.Util;

namespace Starliners.Game {

    /// <summary>
    /// A simple class to record an arbitrary amount of statistical data points.
    /// </summary>
    [Serializable]
    public sealed class StatsRecorder<T> : ISerializable {

        #region Classes

        public sealed class StatisticSlot {
            public readonly string Category;
            public readonly string Attribute;

            readonly string _format;

            public StatisticSlot (string category, string attribute)
                : this (category, attribute, "{0:0}") {
            }

            public StatisticSlot (string category, string attribute, string format) {
                Category = category;
                Attribute = attribute;
                _format = format;
            }

            public bool IsDisplayed (StatsRecorder<T> statistics) {
                return statistics.Records.ContainsKey (Attribute) && !EqualityComparer<T>.Default.Equals (statistics.Records [Attribute], default(T));
            }

            public string ToString (StatsRecorder<T> statistics) {
                return string.Format (_format, statistics.Records [Attribute]);
            }
        }

        #endregion

        public T this [string key] {
            get {
                return _records.ContainsKey (key) ? _records [key] : default(T);
            }
            set {
                _records [key] = value;
            }
        }

        public ReadOnlyDictionary<string, T> Records {
            get {
                return new ReadOnlyDictionary<string, T> (_records);
            }
        }

        Dictionary<string, T> _records = new Dictionary<string, T> ();

        #region Constructor

        public StatsRecorder () {
        }

        #endregion

        #region Serialization

        public StatsRecorder (SerializationInfo info, StreamingContext context) {
            StringOtherPair<T>[] enumerable = info.GetValue ("Records", typeof(StringOtherPair<T>[])) as StringOtherPair<T>[];
            foreach (StringOtherPair<T> entry in enumerable) {
                _records [entry.Key] = entry.Value;
            }
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            StringOtherPair<T>[] enumerable = _records.Select (p => new StringOtherPair<T> (p.Key, p.Value)).ToArray ();
            info.AddValue ("Records", enumerable, typeof(StringOtherPair<T>[]));
        }

        #endregion

        public void Clear () {
            _records.Clear ();
        }

        public void NoteStat (string key, T change) {
            if (!_records.ContainsKey (key)) {
                _records [key] = default(T);
            }
            _records [key] = (dynamic)_records [key] + (dynamic)change;
        }

        public StatsRecorder<T> Copy () {
            StatsRecorder<T> copy = new StatsRecorder<T> ();
            foreach (KeyValuePair<string, T> entry in _records) {
                copy [entry.Key] = entry.Value;
            }

            return copy;
        }
    }
}

