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
using BLibrary.Json;
using System.Runtime.Serialization;
using BLibrary.Util;
using System.Collections.Generic;
using BLibrary.Serialization;

namespace Starliners.Game {

    [Serializable]
    public sealed class Culture : Asset {
        #region Constants

        const string NAME_PREFIX = "culture";

        #endregion

        public Culture (IWorldAccess access, string name, IPopulator populator, JsonObject json)
            : base (access, Utils.BuildName (NAME_PREFIX, name), populator.KeyMap) {
        }

        #region Serialization

        public Culture (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion
    }
}

