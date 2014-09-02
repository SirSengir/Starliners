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
using Starliners.Graphics;
using BLibrary.Graphics;
using Starliners.Game;
using BLibrary.Graphics.Sprites;
using System.Linq;
using Starliners.Game.Forces;

namespace Starliners.Map {
    sealed class LayerFleets : MapLayer {
        public LayerFleets (MapRenderable map, UILayer layer)
            : base (map, layer) {

        }

        public override void Draw (RenderTarget target, RenderStates states, View view) {

            foreach (EntityFleet entity in Map.VisibleEntities.Where(p => p.UILayer == UILayer).OfType<EntityFleet>().OrderBy(p => p.Location.Y)) {

                RenderStates tilestate = states;
                tilestate.Transform.Translate ((int)(entity.Location.X * SpriteManager.TILE_DIMENSION), (int)(entity.Location.Y * SpriteManager.TILE_DIMENSION));

                // Render the entity with it's render type.
                IObjectRenderer renderer = MapRendering.Instance.ObjectRenderers [entity.RenderType];
                RenderStates entitystate = tilestate;
                renderer.DrawRenderable (target, entitystate, entity.Contained.Projector);

                entity.RenderFlags = RenderFlags.None;

            }
        }
    }
}

