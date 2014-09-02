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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using BLibrary;
using System.Collections.Concurrent;

namespace BLibrary.Serialization {

    /// <summary>
    /// Serializes ISerializedLinked objects.
    /// </summary>
    public sealed class SerializationComposer {

        public ConcurrentQueue<ISerializedLinked> Added {
            get { return _added; }
        }

        #region Fields

        IIdObjectAccess _access;
        ConcurrentQueue<ISerializedLinked> _added = new ConcurrentQueue<ISerializedLinked> ();

        #endregion

        public SerializationComposer (IIdObjectAccess access) {
            _access = access;
        }

        /// <summary>
        /// Serializes the given object to the given serialization info according to <see cref="GameSimulator.Game.GameData"/> attributes.
        /// </summary>
        /// <param name='obj'>
        /// Object.
        /// </param>
        /// <param name='info'>
        /// Info.
        /// </param>
        /// <param name='context'>
        /// Context.
        /// </param>
        public void Serialize (ISerializedLinked obj, SerializationInfo info, StreamingContext context) {

            Type type = obj.GetType ();
            if (!type.IsSerializable) {
                throw new ArgumentException (type.ToString () + " is not serializable.");
            }

            bool isPersistent = IsPersistent (context);

            foreach (SerializableMember serializable in GetSerializable(type).Values) {
                if (!isPersistent && !serializable.IsRemote) {
                    continue;
                }
                if (isPersistent && !serializable.IsPersistent) {
                    continue;
                }
                serializable.Serialize (_access, obj, info, context);
            }
        }

        /// <summary>
        /// Deserializes to the given object from the given serialization info according to <see cref="GameSimulator.Game.GameData"/> attributes.
        /// </summary>
        /// <param name='obj'>
        /// Object.
        /// </param>
        /// <param name='info'>
        /// Info.
        /// </param>
        /// <param name='context'>
        /// Context.
        /// </param>
        public void Deserialize (ISerializedLinked obj, SerializationInfo info, StreamingContext context) {

            Type type = obj.GetType ();
            if (!type.IsSerializable) {
                throw new ArgumentException (type.ToString () + " is not deserializable.");
            }

            // Clear cache in object
            if (obj.CacheSerializables != null) {
                obj.CacheSerializables.Clear ();
            }

            Dictionary<string, SerializableMember> serializables = GetSerializable (type);
            SerializationInfoEnumerator enumerator = info.GetEnumerator ();
            while (enumerator.MoveNext ()) {
                // Skip if we do not know how to handle the given key.
                if (!serializables.ContainsKey (enumerator.Name)) {
                    //_access.GameConsole.Warning ("Object information for {0} contained unhandled field or property with the key {1}.", type.ToString (), enumerator.Name);
                    continue;
                }

                serializables [enumerator.Name].Deserialize (_access, obj, info, context);
            }
        }

        /// <summary>
        /// Called after the object was deserialized.
        /// </summary>
        /// <param name='obj'>
        /// Object.
        /// </param>
        public void OnDeserialized (ISerializedLinked obj) {

            Type type = obj.GetType ();
            if (!type.IsSerializable) {
                throw new ArgumentException (type.ToString () + " is not deserializable.");
            }

            foreach (SerializableMember serializable in GetSerializable(type).Values) {
                if (obj.CacheSerializables == null) {
                    continue;
                }
                if (!obj.CacheSerializables.HasKey (serializable.Key)) {
                    continue;
                }
                serializable.OnDeserialized (_access, obj);
            }

            _added.Enqueue (obj);
        }

        Dictionary<Type, Dictionary<string, SerializableMember>> _cachedSerializables = new Dictionary<Type, Dictionary<string, SerializableMember>> ();

        /// <summary>
        /// Create the list of serializable members in this type or return the cached list.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Dictionary<string, SerializableMember> GetSerializable (Type type) {
            if (_cachedSerializables.ContainsKey (type)) {
                return _cachedSerializables [type];
            }

            Dictionary<string, SerializableMember> serializables = new Dictionary<string, SerializableMember> ();

            Type search = type;
            while (search != null) {

                foreach (FieldInfo field in search.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                    if (!field.IsDefined (typeof(GameData), true)) {
                        continue;
                    }

                    FieldWrapper wrapper = new FieldWrapper (field);
                    if (serializables.ContainsKey (wrapper.BaseKey)) {
                        continue;
                    }
                    SerializableMember serializable = CreateSerializable (wrapper);
                    serializables [serializable.Key] = serializable;
                }

                search = search.BaseType;
            }

            search = type;
            while (search != null) {
                foreach (PropertyInfo property in search.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                    if (!property.IsDefined (typeof(GameData), true)) {
                        continue;
                    }

                    PropertyWrapper wrapper = new PropertyWrapper (property);

                    // We can only serialize properties which have an accessible set constructor.
                    if (property.GetSetMethod (true) == null) {
                        _access.Log ("Serialization", "Ignoring property {0} in class {1} since it does not have an accessible set method.", wrapper.BaseKey, search.ToString ());
                        continue;
                    }

                    if (serializables.ContainsKey (wrapper.BaseKey)) {
                        continue;
                    }
                    SerializableMember serializable = CreateSerializable (wrapper);
                    serializables [serializable.Key] = serializable;
                }

                search = search.BaseType;
            }

            _cachedSerializables [type] = serializables;
            return serializables;
        }

        SerializableMember CreateSerializable (MemberWrapper wrapped) {
            if (typeof(IIdIdentifiable).IsAssignableFrom (wrapped.MemberType)) {
                return new SerializableIdObject (wrapped);
            }
            if (typeof(IList).IsAssignableFrom (wrapped.MemberType)) {
                if (wrapped.MemberType.GetGenericArguments ().Length > 0 && typeof(IIdIdentifiable).IsAssignableFrom (wrapped.MemberType.GetGenericArguments () [0])) {
                    return new SerializableIdList (wrapped);
                }
            }
            if (typeof(IDictionary).IsAssignableFrom (wrapped.MemberType)) {
                if (typeof(IIdIdentifiable).IsAssignableFrom (wrapped.MemberType.GetGenericArguments () [1])) {
                    return new SerializableIdDictValue (wrapped);
                }
                if (typeof(IIdIdentifiable).IsAssignableFrom (wrapped.MemberType.GetGenericArguments () [0])) {
                    throw new SystemException ("Cannot use IIdIdentifiable as a key in a dictionary.");
                }
            }
            if (typeof(HashSet<string>).IsAssignableFrom (wrapped.MemberType)) {
                return new SerializableHashSet<string> (wrapped);
            }

            return new SerializablePlain (wrapped);
        }

        bool IsPersistent (StreamingContext context) {
            if (context.State == StreamingContextStates.File
                || context.State == StreamingContextStates.Persistence) {
                return true;
            }

            return false;
        }
    }
}
