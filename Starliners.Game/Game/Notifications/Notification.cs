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

﻿using BLibrary.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BLibrary.Util;

namespace Starliners.Game.Notifications {

    /// <summary>
    /// A player notification.
    /// </summary>
    /// <remarks>Actions are not serialized. Serial is not safe if the player notification list gets cleared during a game.</remarks>
    [Serializable]
    public sealed class Notification : SerializableObject {
        #region Properties

        [GameData (Remote = true, Key = "Inception")]
        public long Inception { get; private set; }

        [GameData (Remote = true, Key = "Category")]
        public NotificationCategory Category {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Handling")]
        public NotificationHandling Handling {
            get;
            private set;
        }

        [GameData (Remote = true, Key = "Tooltip")]
        public TextComposition Tooltip {
            get;
            private set;
        }

        #endregion

        Dictionary<NotificationHandling, bool> _handled = new Dictionary<NotificationHandling, bool> ();
        AutoAction _action;
        [GameData (Remote = true, Key = "Compact")]
        TextComposition _compact;

        #region Constructor

        internal Notification (Player player, NotificationCategory category, AutoAction action, TextComposition compact, TextComposition verbose)
            : base (player.Access) {

            Inception = player.Access.Clock.Ticks;
            Category = category;
            Handling = player.NotificationSettings.GetHandling (category);

            _action = action;
            _compact = compact;
            Tooltip = verbose;
        }

        #endregion

        #region Serialization

        public Notification (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public bool IsHandled (NotificationHandling level) {
            return _handled.ContainsKey (level) && _handled [level];
        }

        public void MarkHandled (NotificationHandling level) {
            _handled [level] = true;
        }

        public void DoAction (Player player) {
            _action.DoAction (player);
        }

        public override string ToString () {
            return _compact.ToString ();
        }
    }
}
