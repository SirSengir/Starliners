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


namespace BLibrary.Network {

    [Flags]
    public enum UpdateMarker {
        None = 0,
        Full = 1 << 0,
        Status = 1 << 1,
        Tag = 1 << 2,
        Progress = 1 << 3,
        Visibility = 1 << 4,
        Update0 = 1 << 5,
        Update1 = 1 << 6,
        Update2 = 1 << 7,
        Update3 = 1 << 8,
        Update4 = 1 << 9,
        Update5 = 1 << 10
    }
}
