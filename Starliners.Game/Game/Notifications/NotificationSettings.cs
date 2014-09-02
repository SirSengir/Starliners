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

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using BLibrary.Serialization;
using BLibrary.Network;

namespace Starliners.Game.Notifications {

    [Serializable]
    public sealed class NotificationSettings : StateObject {
        #region Constants

        public const string TRACKER_KEY = "NotificationSettings";

        #endregion

        [GameData (Remote = true, Key = "Settings")]
        Dictionary<ulong, NotificationHandling> _settings = new Dictionary<ulong, NotificationHandling> ();

        #region Constructor

        public NotificationSettings (IWorldAccess access, string name)
            : base (access, name) {
        }

        #endregion

        #region Serialization

        public NotificationSettings (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public void UnsetHandling (NotificationCategory category, NotificationHandling flag) {
            _settings [category.Serial] &= ~flag;
        }

        public void SetHandling (NotificationCategory category, NotificationHandling flag) {
            _settings [category.Serial] |= flag;
        }

        public void ToggleHandling (NotificationCategory category, NotificationHandling flag) {
            NotificationHandling handling = GetHandling (category);
            if (handling.HasFlag (flag)) {
                UnsetHandling (category, flag);
            } else {
                SetHandling (category, flag);
            }

            MarkUpdated (UpdateMarker.Update0);
        }

        public NotificationHandling GetHandling (NotificationCategory category) {
            if (!_settings.ContainsKey (category.Serial)) {
                _settings [category.Serial] = NotificationManager.Instance.GetDefaultHandling (category);
            }
            return _settings [category.Serial];
        }
    }
}

