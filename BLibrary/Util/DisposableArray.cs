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

namespace BLibrary.Util {

    public class DisposableArray<T> : IDisposable where T : IDisposable {
        #region Index

        public T this [int index] {
            get { return _array [index]; }
            set { _array [index] = value; }
        }

        #endregion

        T[] _array;

        public DisposableArray (int length) {
            _array = new T[length];
        }

        #region IDisposable

        bool _disposed = false;

        public void Dispose () {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        void Dispose (bool manual) {
            if (_disposed) {
                return;
            }

            if (manual) {
                for (int i = 0; i < _array.Length; i++) {
                    if (_array [i] == null) {
                        continue;
                    }
                    _array [i].Dispose ();
                }
            }
            _disposed = true;
        }

        #endregion

        public void Clear () {
            for (int i = 0; i < _array.Length; i++) {
                _array [i] = default(T);
            }
        }
    }
}

