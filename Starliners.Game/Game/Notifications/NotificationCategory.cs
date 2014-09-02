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
using System.Linq;
using BLibrary.Serialization;
using BLibrary.Util;

namespace Starliners.Game.Notifications {

    [Serializable]
    public sealed class NotificationCategory : Asset {
        public const string NAME_PREFIX = "ncategory";

        #region Properties

        [GameData (Remote = true)]
        public string Icon {
            get;
            set;
        }

        #endregion

        #region Constructor

        public NotificationCategory (IWorldAccess access, string name, AssetKeyMap keyMap)
            : base (access, Utils.BuildName (NAME_PREFIX, name), keyMap) {

            Icon = "ncat-default";
        }

        #endregion

        #region Serialization

        public NotificationCategory (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public static NotificationCategory GetCategoryForWorld (IWorldAccess access, string key) {
            string name = Utils.BuildName (NAME_PREFIX, key);
            return (NotificationCategory)access.Assets.Values.First (p => p.Name.Equals (name));
        }
    }
}

