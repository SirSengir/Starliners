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
using BLibrary.Gui;
using BLibrary.Util;
using Starliners.Game.Notifications;
using BLibrary.Gui.Widgets;
using BLibrary.Gui.Tooltips;
using BLibrary.Resources;
using BLibrary.Graphics.Text;
using BLibrary.Graphics;
using BLibrary.Audio;
using OpenTK.Input;
using BLibrary.Gui.Backgrounds;

namespace Starliners.Gui.Widgets {
    /// <summary>
    /// Widget to show a notification on screen.
    /// </summary>
    sealed class SlotNotification : Widget {
        static readonly Vect2i ICON_SIZE = new Vect2i (48, 48);

        public Notification Notification {
            get;
            private set;
        }

        bool _isOutdated;

        public bool IsOutdated {
            get { return _isOutdated; }
            set {
                _isOutdated = value;
                UnflagState (ElementState.Hovered);
            }
        }

        Label _header;
        TextBuffer _text;

        public SlotNotification (Vect2i position, Vect2i size, string key, Notification notification)
            : base (position, size, key) {

            Backgrounds = new BackgroundCollection (UIProvider.Backgrounds ["notification"].Copy ());
            Tinting = BackgroundTinting.INSTANCE;
            FixedTooltip = new TooltipSimple (notification.ToString (), new string[] {
                notification.Tooltip.ToString (),
                Localization.Instance ["notification_slot_help"]
            });

            Notification = notification;
            _text = new TextBuffer (Notification.ToString ());
            _text.SetMaxWidth (Size.X);
            _text.HAlign = Alignment.Center;
            _text.VAlign = Alignment.Center;

            Vect2i csize = _text.LocalBounds.Size.Y > ICON_SIZE.Y ? new Vect2i (Size.X, (int)_text.LocalBounds.Size.Y) : new Vect2i (Size.X, ICON_SIZE.Y);
            _text.Box = csize + new Vect2i (16, 16);
            Size = _text.Box + new Vect2i (ICON_SIZE.X + 32, 36);

            _header = new Label (new Vect2i (), new Vect2i (Size.X, 36), Localization.Instance [Notification.Category.Name]) {
                Backgrounds = UIProvider.Style.HeaderStyle.CreateBackgrounds (),
                Style = FontManager.Instance [FontManager.HEADER],
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };

            AddWidget (_header);

            AddWidget (new IconSymbol (new Vect2i (8, 36 + 6), ICON_SIZE, Notification.Category.Icon));
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);
            states.Transform.Translate (PositionRelative + new Vect2i (ICON_SIZE.X + 16, 33));
            _text.Draw (target, states);
        }

        public override bool HandleMouseClick (Vect2i coordinates, MouseButton button) {
            if (!IntersectsWith (coordinates)) {
                return false;
            }

            SoundManager.Instance.Play (SoundKeys.CLICK);
            Window.DoAction (KeysActions.NOTIFICATION_CLICK, GuiManager.Instance.CombineControlState (button), Notification.Serial);
            return true;
        }

        public override bool HandleMouseRelease (Vect2i coordinates, OpenTK.Input.MouseButton button) {
            return IntersectsWith (coordinates);
        }

        public override bool HandleMouseMove (Vect2i coordinates) {
            return IntersectsWith (coordinates);
        }
    }
}

