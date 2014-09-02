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

    public delegate T DataLinkHandler<T> ();

    /// <summary>
    /// Allows a custom link to a data point.
    /// </summary>
    public sealed class DataLink<T>: IDataReference<T>, ITextProvider {

        public T Value {
            get {
                return _link ();
            }
        }

        public long LastUpdated {
            get {
                return 0;
            }
        }

        DataLinkHandler<T> _link;

        public DataLink (DataLinkHandler<T> link) {
            _link = link;
        }

        public override string ToString () {
            return Value.ToString ();
        }
    }
}

