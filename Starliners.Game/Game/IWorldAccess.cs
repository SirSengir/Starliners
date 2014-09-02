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
using System.Collections.Generic;
using BLibrary.Util;

namespace Starliners.Game {

    public interface IWorldAccess : IIdObjectAccess {

        WorldHolder Controller { get; }

        string Name { get; }

        string File { get; }

        Random Rand { get; }

        Random Seed { get; }

        bool CoinToss { get; }

        GameClock Clock { get; }

        GameConsole GameConsole { get; }

        T GetParameter<T> (string key);

        #region Players

        IReadOnlyDictionary<ulong, Player> Players { get; }

        Player RequirePlayer (ulong serial);

        #endregion

        #region Assets

        IReadOnlyDictionary<ulong, Asset> Assets { get; }

        Asset RequireAsset (ulong serial);

        T RequireAsset<T> (ulong serial) where T : Asset;

        #endregion

        #region Entities

        IReadOnlyDictionary<ulong, Entity> Entities { get; }

        Entity GetEntity (ulong serial);

        Entity RequireEntity (ulong serial);

        T RequireEntity<T> (ulong serial) where T : Entity;

        IList<Entity> GetEntitiesWithin (Vect2f coordinates, int radius);

        #endregion

        #region States

        IReadOnlyDictionary<ulong, StateObject> States { get; }

        StateObject GetState (ulong serial);

        StateObject RequireState (ulong serial);

        T RequireState<T> (ulong serial) where T : StateObject;

        #endregion

        #region Particles

        IReadOnlyList<Particle> Particles { get; }

        #endregion

    }
}

