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
using BLibrary.Util;
using BLibrary.Gui;
using System.Collections.Generic;
using Starliners.Game.Notifications;
using Starliners.Gui.Widgets;

namespace Starliners.Gui.Interface {
    sealed class GuiNotifications : GuiRemote {
        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (192, 768);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_notifications", WINDOW_SIZE, Positioning.UpperLeft, false) {
            Group = ScreenGroup.Hud,
            Style = string.Empty
        };
        const int NOTIFICATION_SPACING = 4;

        #endregion

        List<SlotNotification> _notifies = new List<SlotNotification> ();

        #region Constructor

        public GuiNotifications (int containerId)
            : base (WINDOW_SETTING, containerId) {
            IsDisembodied = true;
            IsCloseable = false;
        }

        #endregion

        protected override void Regenerate () {
            base.Regenerate ();
            RefreshNotifies ();
        }

        void RefreshNotifies () {
            Notification[] notifications = DataProvider.GetValue<Notification[]> (KeysFragments.NOTIFICATION_LIST);

            // Mark existing but outdated notifies.
            for (int i = 0; i < _notifies.Count; i++) {
                bool persists = false;
                for (int j = 0; j < notifications.Length; j++) {
                    if (_notifies [i].Notification.Serial != notifications [j].Serial)
                        continue;

                    persists = true;
                    break;
                }
                if (!persists) {
                    _notifies [i].IsOutdated = true;
                    _notifies [i].Relocate (new Vect2i (-30000, _notifies [i].PositionRelative.Y), Easing.Linear);
                }
            }

            // Create notifies for new notifications.
            for (int i = 0; i < notifications.Length; i++) {
                bool exists = false;
                for (int j = 0; j < _notifies.Count; j++) {
                    if (notifications [i].Serial != _notifies [j].Notification.Serial) {
                        continue;
                    }

                    exists = true;
                    break;
                }
                if (exists) {
                    continue;
                }

                SlotNotification notification = new SlotNotification (new Vect2i (-Size.X, 0), new Vect2i (Size.X, 24), "notification." + notifications [i].Serial, notifications [i]);
                notification.Relocate (new Vect2i (0, 0), Easing.ElasticEaseOut);
                _notifies.Add (notification);
            }

            ClearWidgets ();

            foreach (SlotNotification notify in _notifies) {
                notify.IsDead = false;
                AddWidget (notify);
            }
        }

        List<SlotNotification> _dead = new List<SlotNotification> ();
        List<SlotNotification> _dying = new List<SlotNotification> ();

        public override void Update () {
            base.Update ();

            // Remove dead slots.
            _dead.Clear ();
            for (int i = 0; i < _dying.Count; i++) {
                if (_dying [i].PositionAbsolute.X < -_dying [i].Size.X) {
                    _dead.Add (_dying [i]);
                }
            }

            for (int i = 0; i < _notifies.Count; i++) {
                if (_notifies [i].IsOutdated && !_dying.Contains (_notifies [i])) {
                    _dying.Add (_notifies [i]);
                }
            }

            for (int i = 0; i < _dead.Count; i++) {
                _notifies.Remove (_dead [i]);
                _dying.Remove (_dead [i]);
            }

            // Set new positions:
            int yShift = 0;
            for (int i = 0; i < _notifies.Count; i++) {
                if (_notifies [i].PositionRelativeFinal.Y != yShift) {
                    _notifies [i].RelocateY (yShift, Easing.ElasticEaseOut);
                }
                //float factor = ((float)(_notifies [i].Size.X + _notifies [i].PositionRelative.X) / _notifies [i].Size.X);
                float factor = 1.0f;
                yShift += (int)((_notifies [i].Size.Y + NOTIFICATION_SPACING) * factor);
            }

            if (_dead.Count <= 0 && !IsUpdated) {
                return;
            }

            RefreshNotifies ();
        }
    }
}

