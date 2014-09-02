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

﻿using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL;
using System;
using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using Starliners.Game;
using BLibrary.Util;

namespace Starliners.Graphics {

    public sealed class RendererBlazon : IDisposable {
        #region Constants

        public const string PREFIX_KEY_PATTERN = "coa.pattern.";
        public const string PREFIX_KEY_HERALDIC = "coa.heraldic.";

        const string SHADER_SHIELD = @"uniform sampler2D texture0;
uniform sampler2D texture1;

void main() {
    vec4 matchColor = texture2D(texture1, gl_TexCoord[0].st);

    if(matchColor.a > 0.0) {
        gl_FragColor = texture2D(texture0, gl_TexCoord[0].st) * texture2D(texture1, gl_TexCoord[0].st);
    } else {
        gl_FragColor = vec4(0.0, 0.0, 0.0, 0.0);
    }
}";
        const string SHADER_HERALDIC = @"#version 120

float getOutlineAlpha(sampler2D texture, vec2 texPosition, vec2 stepSize) {
    float alpha = 4.0 * texture2D (texture, texPosition).a;
    alpha -= texture2D (texture, texPosition + vec2(stepSize.x, 0.0)).a;
    alpha -= texture2D (texture, texPosition + vec2(-stepSize.x, 0.0)).a;
    alpha -= texture2D (texture, texPosition + vec2( 0.0, stepSize.y)).a;
    alpha -= texture2D (texture, texPosition + vec2( 0.0, -stepSize.y)).a;

    return clamp(alpha, 0.0, 1.0);
}

uniform sampler2D texture0;
uniform vec4 outColour;
uniform float outIntensity;
uniform vec2 texStep;

void main(void) {
    vec4 texColour = texture2D(texture0, gl_TexCoord[0].st);
    gl_FragColor = vec4(outColour.rgb, getOutlineAlpha(texture0, gl_TexCoord[0].st, texStep) * outIntensity);
}";

        #endregion

        #region Instance

        static RendererBlazon _instance;

        public static RendererBlazon Instance {
            get {
                if (_instance == null) {
                    _instance = new RendererBlazon ();
                }
                return _instance;
            }
        }

        #endregion

        readonly Dictionary<int, Sprite> _cache = new Dictionary<int, Sprite> ();
        Dictionary<string, uint> _patterns = new Dictionary<string, uint> ();
        Dictionary<string, uint> _heraldics = new Dictionary<string, uint> ();

        public void OnAtlasRegeneration () {
            foreach (var entry in _cache) {
                entry.Value.Dispose ();
            }
            _cache.Clear ();

            foreach (string pattern in Blazon.VALID_PATTERNS) {
                _patterns [pattern] = SpriteManager.Instance.RegisterSingle (PREFIX_KEY_PATTERN + pattern);
            }
            foreach (string heraldic in Blazon.VALID_HERALDICS) {
                _heraldics [heraldic] = SpriteManager.Instance.RegisterSingle (PREFIX_KEY_HERALDIC + heraldic);
            }
        }

        #region Cached rendering

        public void DrawRenderable (RenderTarget target, RenderStates states, IHeralded heralded, BlazonShape type) {
            // Draw standard CoA for null.
            if (heralded == null) {
                Blazon empty = Blazon.GetEmpty (GameAccess.Interface.Local, type);
                if (!_cache.ContainsKey (empty.CacheCode)) {
                    CacheBlazon (Blazon.GetEmpty (GameAccess.Interface.Local, type));
                }
                target.Draw (_cache [empty.CacheCode], states);
                return;
            }

            // Create CoA if it does not exist yet.
            if (!_cache.ContainsKey (heralded.Blazon.CacheCode)) {
                CacheBlazon (heralded.Blazon);
            }

            target.Draw (_cache [heralded.Blazon.CacheCode], states);
        }

        public void DrawRenderable (RenderTarget target, RenderStates states, Blazon blazon, BlazonShape type) {
            // Draw standard CoA for null.
            if (blazon == null) {
                Blazon empty = Blazon.GetEmpty (GameAccess.Interface.Local, type);
                if (!_cache.ContainsKey (empty.CacheCode)) {
                    CacheBlazon (Blazon.GetEmpty (GameAccess.Interface.Local, type));
                }
                target.Draw (_cache [empty.CacheCode], states);
                return;
            }

            // Create CoA if it does not exist yet.
            if (!_cache.ContainsKey (blazon.CacheCode)) {
                CacheBlazon (blazon);
            }

            target.Draw (_cache [blazon.CacheCode], states);
        }

        public void EnsureBlazonCache (IHeralded heralded) {
            if (heralded == null) {
                Blazon empty = Blazon.GetEmpty (GameAccess.Interface.Local, BlazonShape.Shield);
                if (!_cache.ContainsKey (empty.CacheCode)) {
                    CacheBlazon (Blazon.GetEmpty (GameAccess.Interface.Local, BlazonShape.Shield));
                }
                return;
            }

            if (!_cache.ContainsKey (heralded.Blazon.CacheCode)) {
                CacheBlazon (heralded.Blazon);
            }

        }

        void CacheBlazon (Blazon blazon) {
            _cache [blazon.CacheCode] = CreateBlazon (blazon);
        }

        #endregion

        #region Dressup

        Blazon _cycledressup;
        Sprite _dressup;

        public void SetDressup (Blazon blazon) {
            if (_dressup != null) {
                _dressup.Texture.Dispose ();
            }

            // To avoid messing up GL states,
            // we only note the new blazon here
            // and destroy the old render.
            _cycledressup = blazon;
            _dressup = null;
        }

        public void DrawDressup (RenderTarget target, RenderStates states, BlazonShape type) {
            if (_cycledressup != null) {
                _dressup = CreateBlazon (_cycledressup);
                _cycledressup = null;
            }
            if (_dressup == null) {
                DrawRenderable (target, states, (IHeralded)null, type);
            }

            target.Draw (_dressup, states);
        }

        #endregion

        #region Blazon sprite creation

        const int SIZE_COA_BUFFER = 128;
        Dictionary<BlazonShape, RenderTexture> _coashapes = new Dictionary<BlazonShape, RenderTexture> ();
        Dictionary<BlazonShape, Sprite> _coaframes = new Dictionary<BlazonShape, Sprite> ();
        Shader _cutout;
        Shader _blendin;

        Sprite CreateBlazon (Blazon blazon) {

            Sprite sprited = null;

            GlWrangler.SafeGLDisable (EnableCap.ScissorTest, () => {

                // Set scale from the default tile size to the desired coa size.
                Vect2f scale = new Vect2f (1, 1);// new Vect2i(SIZE_COA_BUFFER, SIZE_COA_BUFFER) / new Vect2i(64, 64);
                RenderStates mstates = RenderStates.DEFAULT;
                mstates.Transform.Scale (scale);

                if (!_coashapes.ContainsKey (blazon.Shape)) {

                    // Create the mask texture
                    Sprite shape = SpriteManager.Instance [SpriteManager.Instance.RegisterSingle ("coa.shape." + blazon.Shape.ToString ().ToLowerInvariant ())];
   
                    _coashapes [blazon.Shape] = new RenderTexture (SIZE_COA_BUFFER);
                    _coashapes [blazon.Shape].Clear (Colour.Transparent);
                    _coashapes [blazon.Shape].Draw (shape, mstates);
                    _coashapes [blazon.Shape].Display ();
                    //_coashape.Texture.ToBitmap().Save("debug_coashape.bmp");

                    // Create the frame sprite
                    _coaframes [blazon.Shape] = SpriteManager.Instance [SpriteManager.Instance.RegisterSingle ("coa.frame." + blazon.Shape.ToString ().ToLowerInvariant ())];
                    //_coaframe.Texture.ToBitmap().Save("debug_coaframe.bmp");
                }

                // First call, setup the shaders.
                if (_cutout == null) {
                    // Create the masking shader.
                    _cutout = new Shader (ShaderType.FragmentShader, new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_SHIELD)));
                    // Create the blendin shader
                    _blendin = new Shader (ShaderType.FragmentShader, new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_HERALDIC)));
                    _blendin.SetUniform ("outColour", Colour.Black);
                    _blendin.SetUniform ("outIntensity", 0.9f);

                }

                // Set the correct shape texture
                _cutout.SetUniform ("texture1", _coashapes [blazon.Shape].Texture);

                RenderTexture buffer = new RenderTexture (SIZE_COA_BUFFER);
                buffer.Clear (blazon.Colour0);

                if (!string.IsNullOrEmpty (blazon.Pattern)) {
                    Sprite pattern = SpriteManager.Instance [_patterns [blazon.Pattern]];
                    pattern.Colour = blazon.Colour1;
                    buffer.Draw (pattern, mstates);
                    pattern.Colour = Colour.White;
                }
                if (!string.IsNullOrEmpty (blazon.Heraldic)) {
                    Sprite heraldic = SpriteManager.Instance [_heraldics [blazon.Heraldic]];

                    heraldic.Colour = blazon.Colour2;
                    RenderStates hstates = mstates;

                    heraldic.Origin = (heraldic.SourceRect.Size / 2);

                    hstates.Transform.Translate ((new Vect2f (SIZE_COA_BUFFER, SIZE_COA_BUFFER) / 4 + heraldic.SourceRect.Size / 2));
                    hstates.Transform.Scale (1.25f, 1.25f);

                    float rotation = 0;
                    switch (blazon.Style) {
                        case HeraldicStyle.Rotated90:
                            rotation = 90;
                            break;
                        case HeraldicStyle.Rotated180:
                            rotation = 180;
                            break;
                        case HeraldicStyle.Rotated270:
                            rotation = 270;
                            break;
                    }
                    hstates.Transform.Rotate (rotation);

                    hstates.Shader = null;
                    buffer.Draw (heraldic, hstates);

                    _blendin.SetUniform ("texStep", new Vect2f (1f / heraldic.Texture.ActualSize.X, 1f / heraldic.Texture.ActualSize.Y));
                    hstates.Shader = _blendin;
                    buffer.Draw (heraldic, hstates);

                    heraldic.Origin = new Vect2f ();
                    heraldic.Colour = Colour.White;
                }

                buffer.Display ();
                //buffer.Texture.ToBitmap().Save(string.Format("debug_buffer_{0}.bmp", uid));

                // Now transform it into coa shape.
                RenderStates states = RenderStates.DEFAULT;
                states.Shader = _cutout;
                RenderTexture created = new RenderTexture (SIZE_COA_BUFFER);
                created.Clear (Colour.Transparent);
                //created.View = created.View;
                created.Draw (new Sprite (buffer.Texture), states);
                created.Draw (_coaframes [blazon.Shape], mstates);
                created.Display ();
                //created.Texture.ToBitmap().Save(string.Format("debug_created_{0}.bmp", uid));

                created.Texture.IsSmooth = true;
                sprited = new Sprite (created.Texture);
                sprited.SourceRect = new Rect2i (0, 0, Blazon.BASE_SIZE);

                // Cleanup
                buffer.Texture.Dispose ();
            });

            return sprited;
        }

        #endregion

        #region IDisposable

        public void Dispose () {
            foreach (Sprite sprite in _cache.Values) {
                sprite.Texture.Dispose ();
            }
        }

        #endregion
    }
}
