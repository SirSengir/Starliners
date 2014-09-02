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

namespace BLibrary.Serialization {
    /// <summary>
    /// Wraps around reflection information to allow serialization to get and set values in a uniform fashion.
    /// </summary>
    abstract class MemberWrapper {

        public abstract GameData GameData { get; }

        public abstract string BaseKey { get; }

        public abstract Type MemberType { get; }

        public abstract object GetValue (object obj);

        public abstract void SetValue (object obj, object val);
    }

}
