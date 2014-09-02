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

namespace Starliners.Game {

    public sealed class ParameterOptions {

        #region Enums

        public enum InputType {
            None,
            Text,
            Switchable,
            Scenario
        }

        #endregion

        public readonly string Key;
        public readonly string Category;
        public readonly object[] Values;
        public readonly string[] Readables;

        public object Default {
            get;
            set;
        }

        public InputType Type {
            get;
            private set;
        }

        public ParameterOptions (string category)
            : this (InputType.Scenario, ParameterKeys.SCENARIO, category) {
        }

        public ParameterOptions (string key, string category)
            : this (InputType.Text, key, category) {
        }

        public ParameterOptions (string key, string category, object[] values, string[] readables)
            : this (InputType.Switchable, key, category) {
            if (values.Length != readables.Length) {
                throw new ArgumentException ("Setting lengths must match.");
            }

            Values = values;
            Readables = readables;
        }

        private ParameterOptions (InputType type, string key, string category) {
            Type = type;
            Key = key;
            Category = category;
        }

    }
}

