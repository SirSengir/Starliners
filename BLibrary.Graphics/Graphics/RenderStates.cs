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

﻿namespace BLibrary.Graphics {

    public struct RenderStates {
        public static readonly RenderStates DEFAULT = new RenderStates (BlendMode.Alpha, Transform.Identity, null);
        /// <summary>Blending mode</summary>
        public BlendMode BlendMode;
        /// <summary>Transform</summary>
        public Transform Transform;
        public Shader Shader;

        #region Constructor

        public RenderStates (BlendMode blendMode) :
            this (blendMode, Transform.Identity, null) {

        }

        public RenderStates (Transform transform) :
            this (BlendMode.Alpha, transform, null) {

        }

        public RenderStates (BlendMode blendMode, Transform transform, Shader shader) {
            BlendMode = blendMode;
            Transform = transform;
            Shader = shader;
        }

        public RenderStates (RenderStates copy) {
            BlendMode = copy.BlendMode;
            Transform = copy.Transform;
            Shader = copy.Shader;
        }

        #endregion
    }
}
