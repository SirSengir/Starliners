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
using Starliners.Game;
using BLibrary.Graphics.Sprites;
using BLibrary.Util;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Starliners.Graphics {
    public class RendererParticles : IParticleRenderer {

        #region Constants

        const string SHADER_ALPHA = @"uniform float intensity;
uniform sampler2D texture0;

void main() {
    vec4 textureColor = texture2D(texture0, gl_TexCoord[0].st);

    gl_FragColor = vec4(textureColor.rgb*gl_Color.rgb, textureColor.a*intensity);
}";

        #endregion

        Sprite[] _explosions;
        Shader _alpha;

        public RendererParticles () {
            _alpha = new Shader (ShaderType.FragmentShader, new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_ALPHA)));
        }

        public void OnAtlasRegeneration () {
            _explosions = new Sprite[] {
                SpriteManager.Instance [SpriteManager.Instance.RegisterSingle ("impactHull0")],
                SpriteManager.Instance [SpriteManager.Instance.RegisterSingle ("impactHull1")],
                SpriteManager.Instance [SpriteManager.Instance.RegisterSingle ("impactHull2")],
                SpriteManager.Instance [SpriteManager.Instance.RegisterSingle ("impactHull3")],
                SpriteManager.Instance [SpriteManager.Instance.RegisterSingle ("impactHull4")]
            };
        }

        public void DrawParticle (RenderTarget target, RenderStates states, Particle particle) {
            states.Transform.Translate (particle.Location * SpriteManager.TILE_DIMENSION);

            double scale = (0.2 + 0.8 * particle.Age / particle.MaxAge);
            _alpha.SetUniform ("intensity", (float)(1 - 0.5 * particle.Age / particle.MaxAge));
            Drawable drawable = _explosions [particle.Seed];

            states.Shader = _alpha;
            states.Transform.Scale (new Vect2d (scale, scale), drawable.LocalBounds.Center);
            target.Draw (drawable, states);

        }
    }
}

