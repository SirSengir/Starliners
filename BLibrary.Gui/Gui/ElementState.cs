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

namespace BLibrary.Gui {

    [Flags]
    public enum ElementState {
        None = 0,
        Disabled = 1 << 0,
        Active = 1 << 1,
        Hovered = 1 << 2,
        Pressed = 1 << 3,
        Dragged = 1 << 4
    }

    public static class ElementStates {
        public static ElementState[] VALUES = (ElementState[])Enum.GetValues (typeof(ElementState));
    }
}

