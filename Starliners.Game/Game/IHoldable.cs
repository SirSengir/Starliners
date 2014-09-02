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

﻿using BLibrary.Util;


namespace Starliners.Game {

    public interface IHoldable {

        /// <summary>
        /// Gets a value indicating whether this <see cref="Starliners.Game.IHoldable"/> suppresses entity clicks or not.
        /// </summary>
        /// <value><c>true</c> if suppress entity click; otherwise, <c>false</c>.</value>
        bool SuppressEntityClick {
            get;
        }

        /// <summary>
        /// Called, when the IHoldable is clicked onto the map.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="xCoord"></param>
        /// <param name="yCoord"></param>
        /// <returns>The (possibly modified) IHoldable or null. Replaces it on the player.</returns>
        IHoldable OnMapClick (Player player, Vect2f coordinates, ControlState control);

    }
}
