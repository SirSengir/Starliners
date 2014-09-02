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

namespace BLibrary.Util {

    [Serializable]
    public sealed class TextComposition : ISerializable, ITextProvider {

        public long LastUpdated {
            get {
                return 0;
            }
        }

        ITextProvider[] _components;

        public TextComposition (string text)
            : this (new TextComponent (text) { IsLocalizable = true }) {
        }

        public TextComposition (string text, ITextProvider component)
            : this (new TextComponent (text) { IsLocalizable = true }, component) {
        }

        public TextComposition (params ITextProvider[] components) {
            _components = components;
        }

        public TextComposition (ITextProvider component, params ITextProvider[] components) {

            _components = new ITextProvider[1 + components.Length];
            _components [0] = component;
            Array.Copy (components, 0, _components, 1, components.Length);
        }


        #region Serialization

        public TextComposition (SerializationInfo info, StreamingContext context) {
            _components = (ITextProvider[])info.GetValue ("Components", typeof(ITextProvider[]));
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("Components", _components, typeof(ITextProvider[]));
        }

        #endregion

        object[] _args;

        public override string ToString () {
            if (_components.Length <= 0) {
                return string.Empty;
            }
            if (_components.Length == 1) {
                return _components [0].ToString ();
            }
            if (_components.Length == 2) {
                return string.Format (_components [0].ToString (), _components [1]);
            }

            if (_args == null) {
                _args = new object[_components.Length - 1];
                Array.Copy (_components, 1, _args, 0, _args.Length);
            }
            return string.Format (_components [0].ToString (), _args);
        }
    }
}
