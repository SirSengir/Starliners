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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace BLibrary.Serialization {
    public class SaveMapper : SerializationBinder {

        GameConsole _console;

        List<Type> _boundTypes = new List<Type> ();

        Dictionary<string, Type> _nameToTypeMap = new Dictionary<string, Type> ();
        Dictionary<Type, string> _typeToNameMap = new Dictionary<Type, string> ();

        public SaveMapper (GameConsole console) {
            _console = console;
        }

        public void AddNameToTypeMapping (string name, Type type) {
            _nameToTypeMap [name] = type;
        }

        public override void BindToName (Type serializedType, out string assemblyName, out string typeName) {
            if (!_boundTypes.Contains (serializedType))
                _boundTypes.Add (serializedType);

            if (!_typeToNameMap.ContainsKey (serializedType)) {
                base.BindToName (serializedType, out assemblyName, out typeName);
                return;
            }
            assemblyName = serializedType.Assembly.FullName;
            typeName = _typeToNameMap [serializedType];
        }

        public override Type BindToType (string assemblyName, string typeName) {
            if (!_nameToTypeMap.ContainsKey (typeName)) {
                return Type.GetType (String.Format ("{0}, {1}", typeName, assemblyName));
            }

            _console.Debug ("Converting type {0} to {1}.", typeName, _nameToTypeMap [typeName]);
            return Type.GetType (String.Format ("{0}, {1}", _nameToTypeMap [typeName], assemblyName));
        }

        public string GetBoundTypeList () {
            StringBuilder builder = new StringBuilder ();
            foreach (Type type in _boundTypes.OrderBy(p => p.FullName))
                builder.AppendLine (type.FullName);
            return builder.ToString ();
        }
    }
}
