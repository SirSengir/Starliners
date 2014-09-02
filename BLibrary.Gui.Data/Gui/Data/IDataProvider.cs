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

namespace BLibrary.Gui.Data {

    /// <summary>
    /// I am a data provider.
    /// </summary>
    public interface IDataProvider : IUpdateIndicator {

        /// <summary>
        /// Determines whether this instance has a data fragment for the specified key.
        /// </summary>
        /// <returns><c>true</c> if this instance has a fragment for the specified key; otherwise, <c>false</c>.</returns>
        /// <param name="key">Key.</param>
        bool HasFragment (string key);

        /// <summary>
        /// Gets the value contained in the data fragment of the given key.
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="key">Key.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        T GetValue<T> (string key);
    }
}
