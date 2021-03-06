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
using Starliners.Game.Notifications;

namespace Starliners.Game.Scenario {
    sealed class CreatorNotifications : AssetCreator {
        public CreatorNotifications (int weight)
            : base (weight) {
        }

        public override IEnumerable<AssetHolder> CreateAssets (IWorldAccess access, IPopulator populator) {
            AssetHolder<NotificationCategory> holder = new AssetHolder<NotificationCategory> (Weight, AssetKeys.NOTIFICATION_CATEGORIES);
            holder [NotificationCategories.DEBUG_NOTIFY] = new NotificationCategory (access, NotificationCategories.DEBUG_NOTIFY, populator.KeyMap);
            holder [NotificationCategories.BATTLE_REPORT] = new NotificationCategory (access, NotificationCategories.BATTLE_REPORT, populator.KeyMap);
            holder [NotificationCategories.CONQUEST_REPORT] = new NotificationCategory (access, NotificationCategories.CONQUEST_REPORT, populator.KeyMap);
            holder [NotificationCategories.FACTION_ELIMINATED] = new NotificationCategory (access, NotificationCategories.FACTION_ELIMINATED, populator.KeyMap);

            return new List<AssetHolder> () { holder };
        }
    }
}

