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
using Starliners;
using BLibrary.Graphics.Sprites;
using BLibrary.Gui.Tooltips;
using BLibrary.Gui.Widgets;
using BLibrary.Resources;
using BLibrary.Util;
using Starliners.Game;
using BLibrary.Gui;

namespace Starliners.Gui.Widgets {

    public class MenuClock : MenuWidget {

        GameClock _clock;

        protected override int SpriteCount {
            get {
                return 3;
            }
        }

        public MenuClock (Vect2i position, Vect2i size, string key)
            : base (position, size, key, "clock") {
            IsSensitive = true;

            _clock = GameAccess.Interface.Local.Clock;

        }

        protected override string GenerateInfoText () {
            return _clock.FileFormat;//Localization.Instance ["season_" + _clock.Season.ToString ().ToLowerInvariant ()];
        }

        protected override Sprite GetSymbolSprite (int index) {
            //if (index == 0) {
            return base.GetSymbolSprite (index);
            //}

            /*
            if (index == 1) {
                return GetClockPointer (_iconIndeces [1], ((float)_clock.TicksThisHour / _clock.TicksPerHour) * 360);
            } else {
                long tickselapsed = _clock.TicksToday;
                tickselapsed = tickselapsed > _clock.TicksPerDay / 2 ? tickselapsed - (_clock.TicksPerDay / 2) : tickselapsed;
                return GetClockPointer (_iconIndeces [2], ((float)tickselapsed / (_clock.TicksPerDay / 2)) * 360);
            }*/
        }

        Sprite GetClockPointer (uint iconIndex, float rotate) {
            rotate = Math.Abs (rotate - 360);
            Sprite temp = SpriteManager.Instance [iconIndex];

            temp.Position = new Vect2f (16, 16);
            temp.Origin = new Vect2f (16, 16);
            temp.Rotation = rotate;
            return temp;
        }

    }
}
