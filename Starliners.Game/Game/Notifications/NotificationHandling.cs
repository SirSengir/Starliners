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

namespace Starliners.Game.Notifications {

    [Flags]
    public enum NotificationHandling {
        Unknown = 0,
        /// <summary>
        /// List this notification in the log.
        /// </summary>
        Log = 1 << 0,
        /// <summary>
        /// Show notification to the player in the map state.
        /// </summary>
        Notify = 1 << 1,
        /// <summary>
        /// Throw open a popup to make sure the player _really_ saw it.
        /// </summary>
        Popup = 1 << 2
    }

    public static class NotificationHandlings {
        public static readonly NotificationHandling[] VALUES = (NotificationHandling[])Enum.GetValues (typeof(NotificationHandling));

        public const NotificationHandling NOTIFY = NotificationHandling.Log | NotificationHandling.Notify;
        public const NotificationHandling POPUP = NotificationHandling.Log | NotificationHandling.Notify | NotificationHandling.Popup;
    }
}

