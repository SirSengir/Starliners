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
using Starliners.Game;
using BLibrary.Gui.Widgets;

namespace Starliners.Gui.Widgets {
    sealed class MenuPlayer : Widget {

        Faction _faction;

        public MenuPlayer (Vect2i position, Vect2i size)
            : base (position, size) {

            Backgrounds = UIProvider.Styles ["hud"].ButtonStyle.CreateBackgrounds ();
            BackgroundStates = BG_STATES_SENSITIVE;

            _faction = GameAccess.Interface.ThePlayer.MainFaction;
            Grouping grouped = new Grouping (Vect2i.ZERO, size) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);
            grouped.AddWidget (new IconBlazon (Vect2i.ZERO, new Vect2i (32, 32), _faction));
            grouped.AddWidget (new Label (new Vect2i (32 + 8, 0), new Vect2i (size.X - 32 - 3 * 8, 24), _faction.FullName) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            });
        }

    }
}

