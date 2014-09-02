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
using BLibrary.Graphics.Sprites;
using BLibrary.Util;
using OpenTK.Graphics.OpenGL;
using System.IO;
using Starliners.States;

namespace Starliners.Map {
    sealed class LayerStarfield : MapLayer {

        const string SHADER_TRANSLUCENCY = @"uniform sampler2D texture0;

void main() {
    vec4 texColor = texture2D(texture0, gl_TexCoord[0].st);
    float alpha = clamp(texColor.r + texColor.g + texColor.b, 0.0, 1.0);
    gl_FragColor = vec4(texColor.rgb, alpha);
}";

        Sprite _background;
        Sprite _nebulae0;
        Sprite _nebulae1;
        Sprite _nebulae2;

        Shader _translucency;

        public LayerStarfield (MapRenderable map, UILayer layer)
            : base (map, layer) {
            _background = new Sprite (SpriteManager.Instance.LoadTexture ("Resources/Textures/Map/Default.png"));
            _nebulae0 = new Sprite (SpriteManager.Instance.LoadTexture ("Resources/Textures/Map/Nebulae0.png"));
            _nebulae1 = new Sprite (SpriteManager.Instance.LoadTexture ("Resources/Textures/Map/Nebulae1.png"));
            _nebulae2 = new Sprite (SpriteManager.Instance.LoadTexture ("Resources/Textures/Map/Nebulae2.png"));

            _translucency = new Shader (ShaderType.FragmentShader, new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_TRANSLUCENCY)));
        }

        public override void Draw (RenderTarget target, RenderStates states, View view) {

            View previous = target.View;

            target.View = target.DefaultView;
            target.Draw (_background, states);
            target.View = previous;

            states.Shader = _translucency;

            states.Transform.Translate (view.Center * 0.9f);
            target.Draw (_nebulae0, states);

            states.Transform.Translate (new Vect2i (-100, 20));
            target.Draw (_nebulae1, states);

            states.Transform.Translate (new Vect2i (-500, -720));
            target.Draw (_nebulae2, states);

        }
    }
}

