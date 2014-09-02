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

namespace Starliners.States {

    sealed class FlatCanvas : ScreenCanvas {
        public override float Alpha {
            get {
                return _background.Alpha;
            }
            set {
                _background.Alpha = value;
            }
        }

        Sprite _background;

        public FlatCanvas (string ident) {

            Texture mapTexture = SpriteManager.Instance.LoadRandomTexture (ident);
            mapTexture.IsSmooth = true;
            _background = new Sprite (mapTexture);

        }

        public override void Draw (RenderTarget target, RenderStates states) {
            float resize = target.Size.X / _background.LocalBounds.Size.X;
            states.Transform.Translate (0, (target.Size.Y - (resize * _background.LocalBounds.Size.Y)) / 2);
            states.Transform.Scale (resize, resize);
            target.Draw (_background, states);
        }
    }
}

