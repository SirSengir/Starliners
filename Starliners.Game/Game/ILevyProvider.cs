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
using BLibrary;
using Starliners.Game.Forces;

namespace Starliners.Game {
    public interface ILevyProvider : IIdIdentifiable {
        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the owner associated with the provider.
        /// </summary>
        /// <value>The owner.</value>
        Faction Owner { get; }

        /// <summary>
        /// Gets the culture associated with the provider.
        /// </summary>
        /// <value>The culture.</value>
        Culture Culture { get; }

        /// <summary>
        /// Gets the time for full reenforcement in reenforcement cycles.
        /// </summary>
        /// <value>The reenforcement.</value>
        uint Reenforcement { get; }

        /// <summary>
        /// Gets the maximum amount of ship maintenance provided.
        /// </summary>
        /// <value>The maintenance.</value>
        int GetMaintenance (ShipSize size);

        /// <summary>
        /// Gets the given attribute.
        /// </summary>
        /// <returns>The attribute.</returns>
        /// <param name="key">Key.</param>
        int GetAttribute (string key);

        /// <summary>
        /// Raises the ship loss event.
        /// </summary>
        /// <param name="sclass">Sclass.</param>
        void OnShipLoss (ShipClass sclass);

        /// <summary>
        /// Creates ship modifiers for ships of the given size.
        /// </summary>
        /// <returns>The ship modifiers.</returns>
        /// <param name="size">Size.</param>
        ShipModifiers CreateShipModifiers (ShipSize size);
    }
}

