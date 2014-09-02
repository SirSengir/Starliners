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
using Starliners.Game.Planets;
using System.Runtime.Serialization;
using BLibrary.Json;

namespace Starliners.Game {
    abstract class Trigger : SerializableObject {

        #region Constructor

        public Trigger (IWorldAccess access)
            : base (access) {
        }

        #endregion

        #region Serialization

        public Trigger (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public abstract bool IsTripped (Planet planet);

        public abstract bool IsTripped (ILevyProvider planet);

        public static Trigger InstantiateTrigger (IWorldAccess access, IPopulator populator, JsonObject json) {
            AssetHolder<Type> classmaps = (AssetHolder<Type>)populator.Holders [AssetKeys.CLASSMAPS];
            string ident = string.Format ("{0}.{1}", "trigger", json ["type"].GetValue<string> ());
            return (Trigger)Activator.CreateInstance (classmaps [ident], new object[] { access, populator, json });
        }
    }
}

