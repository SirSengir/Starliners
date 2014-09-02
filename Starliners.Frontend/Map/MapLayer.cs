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
using BLibrary.Graphics;
using Starliners.Graphics;
using Starliners.Game;

namespace Starliners.Map {

    abstract class MapLayer : IDisposable {
        protected MapRenderable Map {
            get;
            private set;
        }

        protected IWorldAccess Access {
            get;
            private set;
        }

        public UILayer UILayer {
            get;
            private set;
        }

        public MapLayer (MapRenderable map, UILayer layer) {
            Map = map;
            Access = map.Simulator.Access;
            UILayer = layer;
        }

        public abstract void Draw (RenderTarget window, RenderStates states, View view);

        #region IDisposable

        public virtual void Dispose () {
        }

        #endregion
    }
}
