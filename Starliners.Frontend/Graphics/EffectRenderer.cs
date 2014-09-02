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
using OpenTK.Graphics.OpenGL;
using System.IO;
using Starliners.Map;
using BLibrary.Util;
using BLibrary.Graphics.Sprites;

namespace Starliners.Graphics {
    sealed class EffectRenderer {
        #region Constants

        const string SHADER_OUTLINE = @"#version 120

float getOutlineAlpha(sampler2D texture, vec2 texPosition, vec2 stepSize) {
    float alpha = -4.0 * texture2D (texture, texPosition).a;
    alpha += texture2D (texture, texPosition + vec2(stepSize.x, 0.0)).a;
    alpha += texture2D (texture, texPosition + vec2(-stepSize.x, 0.0)).a;
    alpha += texture2D (texture, texPosition + vec2( 0.0, stepSize.y)).a;
    alpha += texture2D (texture, texPosition + vec2( 0.0, -stepSize.y)).a;

    return clamp(alpha, 0.0, 1.0);
}

uniform sampler2D texture0;
uniform vec4 colour;
uniform float intensity;
uniform vec2 texStep;

void main(void) {
    vec4 texColour = texture2D(texture0, gl_TexCoord[0].st);
    gl_FragColor = vec4(colour.rgb, getOutlineAlpha(texture0, gl_TexCoord[0].st, texStep) * intensity);
}";

        #endregion

        Shader _outlineStatic;
        Shader _outlinePulse;
        Shader _outlineFlexible;

        public EffectRenderer () {
            // Create shaders
            using (Stream stream = new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_OUTLINE))) {
                _outlinePulse = new Shader (ShaderType.FragmentShader, stream);
            }

            using (Stream stream = new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_OUTLINE))) {
                _outlineStatic = new Shader (ShaderType.FragmentShader, stream);
                _outlineStatic.SetUniform ("intensity", 0.9f * 2);
            }

            using (Stream stream = new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_OUTLINE))) {
                _outlineFlexible = new Shader (ShaderType.FragmentShader, stream);
            }

        }

        public void OnFrameStart () {
            _outlineFlexible.SetUniform ("intensity", MathUtils.AsAmplitude (SpriteManager.Instance.Metronom1));
            _outlinePulse.SetUniform ("intensity", MathUtils.AsAmplitude (SpriteManager.Instance.Metronom1));
        }

        public void DrawOutline (RenderTarget target, RenderStates states, IRenderable renderable, SpriteModel sprite, RenderFlags flags) {

            if ((flags & RenderFlags.Outline) != RenderFlags.Outline) {
                return;
            }

            if (GameAccess.Interface.ThePlayer.Target != null && GameAccess.Interface.ThePlayer.Target.IsTargeted (renderable)) {
                _outlineFlexible.SetUniform ("colour", Colour.Fuchsia);
                _outlineFlexible.SetUniform ("texStep", new Vect2f (4f / sprite.LocalBounds.Width, 4f / sprite.LocalBounds.Height));
                _outlineFlexible.SetUniform ("intensity", 2 * GameAccess.Interface.ThePlayer.ActivationProgress);

                states.Shader = _outlineFlexible;

            } else if (renderable == GameAccess.Interface.HoveredEntity) {
                _outlinePulse.SetUniform ("colour", Colour.Yellow);
                _outlinePulse.SetUniform ("texStep", new Vect2f (4f / sprite.LocalBounds.Width, 4f / sprite.LocalBounds.Height));

                states.Shader = _outlinePulse;
            } else {
                _outlineStatic.SetUniform ("colour", Colour.Turquoise);
                _outlineStatic.SetUniform ("texStep", new Vect2f (4f / sprite.LocalBounds.Width, 4f / sprite.LocalBounds.Height));

                states.Shader = _outlineStatic;
                return;
            }

            sprite.DrawMask (target, states);

        }
    }
}

