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
using Starliners.Game;
using BLibrary.Gui.Data;
using BLibrary.Util;
using Starliners.Game.Notifications;
using System.Linq;
using BLibrary.Network;

namespace Starliners.Gui.Interface {
    sealed class ContainerNotifications : Container {
        Player _player;

        public ContainerNotifications (ushort guiId, Player player)
            : base (guiId) {
            Precedence = KeysPrecedents.NOTIFICATIONS;
            Tags.Add (TAG_NOTIFICATION);
            _player = player;
            OnChanged ();
        }

        protected override void Refresh (object sender, EventArgs e) {
            base.Refresh (sender, e);
            UpdateFragment (KeysFragments.NOTIFICATION_LIST, _player.Notifications.Where (p => !p.IsHandled (NotificationHandling.Notify) && p.Handling.HasFlag (NotificationHandling.Notify) && _player.Access.Clock.Ticks - p.Inception < 1600).OrderByDescending (p => p.Serial).Take (10).ToArray ());
        }

        public override void UpdateTick () {
            base.UpdateTick ();
            if (_player.Access.Clock.Ticks % 100 == 0)
                OnChanged ();
        }

        public override bool HandleAction (Player player, string key, Payload args) {

            if (KeysActions.NOTIFICATION_CLICK.Equals (key)) {

                ControlState control = args.GetValue<ControlState> (0);
                Notification notification = _player.Notifications.Where (p => p.Serial == args.GetValue<ulong> (1)).FirstOrDefault ();
                if (notification != null) {
                    if (control.HasFlag (ControlState.MouseRight)) {
                        notification.MarkHandled (NotificationHandling.Notify);
                    } else {
                        notification.DoAction (player);
                    }
                }
                OnChanged ();
                return true;

            } else
                return base.HandleAction (player, key, args);
        }
    }
}

