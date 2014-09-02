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

﻿using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using Starliners.Graphics;
using BLibrary.Util;

namespace Starliners {

    public interface IObjectRenderer {
        SpriteModel this [IRenderable renderable, ModelPart part] { get; }

        /// <summary>
        /// Called when the SpriteAtlas regenerates.
        /// </summary>
        void OnAtlasRegeneration ();

        void OnFrameStart ();

        void OnRenderableRemoved (IRenderable renderable);

        void DrawRenderable (RenderTarget target, RenderStates states, IRenderable renderable);

        void DrawRenderable (RenderTarget target, RenderStates states, IRenderable renderable, RenderFlags flags);
    }
}
