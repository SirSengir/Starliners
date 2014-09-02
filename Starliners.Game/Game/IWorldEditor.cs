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
using System.Collections.Generic;
using BLibrary.Util;

namespace Starliners.Game {

    public interface IWorldEditor : IWorldAccess {

        ulong LastSerial { get; set; }

        void OnStatesLoaded ();

        void OnWorldLoaded ();

        void AddPlayer (Player player);

        void AddAsset (Asset asset);

        #region Entities

        void AddEntity (Entity entity);

        void RemoveEntity (Entity entity);

        #endregion

        #region States

        void AddState (StateObject state);

        void RemoveState (StateObject state);

        #endregion

        void AddParticle (Particle particle);

        void RemoveParticle (Particle particle);
    }
}

