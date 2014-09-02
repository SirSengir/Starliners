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

namespace Starliners.Graphics {

    /// <summary>
    /// Indicates how the renderable should be treated when
    /// it is hidden behind another on the map.
    /// </summary>
    [Flags]
    public enum UnhidingBehaviour {
        /// <summary>
        /// Never take any action to unhide.
        /// </summary>
        None = 0,
        /// <summary>
        /// Unhide the entity whenever its visibility is impaired.
        /// </summary>
        Always = 1 << 0,
        /// <summary>
        /// Unhide the entity only if visibility of all entities was requested.
        /// </summary>
        Request = 1 << 1
    }
}

