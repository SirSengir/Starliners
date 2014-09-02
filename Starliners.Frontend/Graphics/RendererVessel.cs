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
using BLibrary.Util;
using BLibrary.Graphics.Sprites;
using BLibrary.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace Starliners.Graphics {
    sealed class RendererVessel : IObjectRenderer {

        #region Constants

        const string SHADER_ALPHA = @"uniform float intensity;
uniform sampler2D texture0;

void main() {
    vec4 textureColor = texture2D(texture0, gl_TexCoord[0].st);

    gl_FragColor = vec4(textureColor.rgb*gl_Color.rgb, textureColor.a*intensity);
}";

        #endregion

        #region Instance

        static RendererVessel _instance;

        public static RendererVessel Instance {
            get {
                if (_instance == null) {
                    _instance = new RendererVessel ();
                }
                return _instance;
            }
        }

        #endregion

        #region Properties

        public SpriteModel this [IRenderable renderable, ModelPart part] {
            get {
                IRenderableEntity rendered = (IRenderableEntity)renderable;
                VerifyModelCache (rendered, part);

                SpriteModel model = _cachedModels [renderable.RenderHash] [(int)part];
                model.ModelReel = rendered.GetCurrentReel (part);
                return model;
            }
        }

        #endregion

        #region Fields

        Shader _lights0;
        Shader _lights1;

        DisposablesCache<DisposableArray<SpriteModel>> _cachedModels = new DisposablesCache<DisposableArray<SpriteModel>> ("RendererVessel");
        EffectRenderer _effects = new EffectRenderer ();

        #endregion

        public RendererVessel () {
            _lights0 = new Shader (ShaderType.FragmentShader, new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_ALPHA)));
            _lights1 = new Shader (ShaderType.FragmentShader, new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_ALPHA)));
        }

        public void OnAtlasRegeneration () {
            _cachedModels.Cleanup ();
        }

        public void OnFrameStart () {
            _effects.OnFrameStart ();
            _cachedModels.Maintain ();

            _lights0.SetUniform ("intensity", MathUtils.AsAmplitude (SpriteManager.Instance.Metronom1));
            _lights1.SetUniform ("intensity", (float)Math.Sin (MathUtils.AsAmplitude (SpriteManager.Instance.Metronom0)));
        }

        public void OnRenderableRemoved (IRenderable renderable) {
            _cachedModels.Remove (renderable.RenderHash);
        }

        public void DrawRenderable (RenderTarget target, RenderStates states, IRenderable renderable) {
            DrawRenderable (target, states, renderable, RenderFlags.None);
        }

        public void DrawRenderable (RenderTarget target, RenderStates states, IRenderable renderable, RenderFlags flags) {
            IRenderableEntity rendered = (IRenderableEntity)renderable;

            SpriteModel model = this [rendered, ModelPart.Sprite];
            model.Draw (target, states);

            if (rendered.HasPart (ModelPart.Flavour)) {
                states.Shader = rendered.RenderHash % 2 == 0 ? _lights0 : _lights1;
                this [rendered, ModelPart.Flavour].Draw (target, states);
                states.Shader = null;
            }

            if (rendered.HasPart (ModelPart.Damage)) {
                this [rendered, ModelPart.Damage].Draw (target, states);
            }
            _effects.DrawOutline (target, states, renderable, model, flags);
        }

        void VerifyModelCache (IRenderableEntity renderable, ModelPart part) {
            if (!_cachedModels.HasCached (renderable.RenderHash)) {
                _cachedModels [renderable.RenderHash] = new DisposableArray<SpriteModel> (ModelParts.VALUES.Length);
            } else if (renderable.RenderChanged) {
                _cachedModels [renderable.RenderHash].Clear ();
            }

            if (_cachedModels [renderable.RenderHash] [(int)part] == null) {
                _cachedModels [renderable.RenderHash] [(int)part] = new SpriteModel (renderable.GetReels (part), part == ModelPart.Sprite);
            }

            renderable.RenderChanged = false;
        }

    }
}

