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
using System.Reflection;

namespace BLibrary.Serialization {
    sealed class PropertyWrapper : MemberWrapper {
        public override GameData GameData {
            get { return _attr; }
        }

        public override string BaseKey {
            get { return GameData.GetKey (_property.Name); }
        }

        public override Type MemberType {
            get { return _property.PropertyType; }
        }

        PropertyInfo _property;
        GameData _attr;

        public PropertyWrapper (PropertyInfo property) {
            _property = property;
            _attr = (GameData)Attribute.GetCustomAttribute (property, typeof(GameData));
        }

        public override object GetValue (object obj) {
            return _property.GetValue (obj, null);
        }

        public override void SetValue (object obj, object val) {
            _property.GetSetMethod (true).Invoke (obj, new object[] { val });
            ;
        }
    }
}
