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
using System.IO;
using OpenTK.Graphics.OpenGL;
using BLibrary.Util;
using Starliners;

namespace BLibrary.Graphics.Sprites {

    /// <summary>
    /// Represents the sprite(s) needed for more complex shapes. Includes animations and masks.
    /// </summary>
    public sealed class SpriteModel : Transformable, IDrawable, IDisposable {
        #region Constants

        const string SHADER_MASK = @"uniform sampler2D texture0;

void main() {
    vec4 matchColor = texture2D(texture0, gl_TexCoord[0].st);

    if(matchColor.a > 0.0) {
        gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    } else {
        gl_FragColor = vec4(0.0, 0.0, 0.0, 0.0);
    }
}";

        #endregion

        #region Properties

        public Rect2f SourceRect0 {
            get { return _batches [0] [0].SourceRect0; }
        }

        public int ModelReel {
            get;
            set;
        }

        public float AnimationSpeed {
            get;
            set;
        }

        public override Rect2f LocalBounds {
            get {
                return _batches [0] [0].LocalBounds;
            }
        }

        Vect2f AnchorShift {
            get {
                if (_anchors [ModelReel] == Anchor.Center) {
                    return (LocalBounds.Size / 2) * -1;
                }

                return Vect2f.ZERO;
            }
        }

        int CurrentCell {
            get {
                return _batches [ModelReel].Length > 1 ? (int)(((SpriteManager.Instance.Metronom0 * AnimationSpeed) % 1) * _batches [ModelReel].Length) : 0;
            }
        }

        #endregion

        static Shader _mask;
        SpriteBatch[][] _batches;
        Anchor[] _anchors;
        Sprite[][] _masks;
        bool _masking;

        #region Constructor

        public SpriteModel (ModelReel[] reels, bool masking)
            : this (masking) {
            Rebuild (reels);
        }

        public SpriteModel (SpriteBatch[] batches, bool masking)
            : this (masking) {
            Rebuild (batches);
        }

        SpriteModel (bool masking) {
            _masking = masking;
            // Create shaders if those haven't been already created.
            if (_mask == null) {
                // Create the masking shader
                _mask = new Shader (ShaderType.FragmentShader, new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_MASK)));
            }
            AnimationSpeed = 1.0f;
        }

        #endregion

        #region IDisposable

        bool _disposed = false;

        public void Dispose () {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        void Dispose (bool manual) {
            if (_disposed) {
                return;
            }

            if (manual) {
                for (int i = 0; i < _batches.Length; i++) {
                    for (int j = 0; j < _batches [i].Length; j++) {
                        _batches [i] [j].Dispose ();
                    }
                }

                if (_masks != null) {
                    for (int i = 0; i < _masks.Length; i++) {
                        for (int j = 0; j < _masks [i].Length; j++) {
                            _masks [i] [j].Dispose ();
                        }
                    }
                }

            }
            _disposed = true;
        }

        #endregion

        public void Draw (RenderTarget target, RenderStates states) {
            states.Transform.Translate (AnchorShift);
            states.Transform *= Transform;
            _batches [ModelReel] [CurrentCell].Draw (target, states);
        }

        public void DrawMask (RenderTarget target, RenderStates states) {
            states.Transform.Translate (AnchorShift);
            states.Transform *= Transform;
            _masks [ModelReel] [CurrentCell].Draw (target, states);
        }

        public void Rebuild (SpriteBatch[] batches) {
            ModelReel = 0;
            _batches = new SpriteBatch[batches.Length][];
            _anchors = new Anchor[batches.Length];

            for (int i = 0; i < _batches.Length; i++) {
                _batches [i] = new SpriteBatch[] { batches [i] };
                _anchors [i] = Anchor.TopLeft;
            }

            if (_masking) {
                _masks = new Sprite[batches.Length][];
                for (int i = 0; i < _masks.Length; i++) {
                    _masks [i] = CreateMask (_batches [i]);
                }
            }
        }

        public void Rebuild (ModelReel[] reels) {
            ModelReel = 0;
            _batches = new SpriteBatch[reels.Length][];
            _anchors = new Anchor[reels.Length];

            for (int i = 0; i < _batches.Length; i++) {
                _batches [i] = BuildReel (reels [i]);
                _anchors [i] = reels [i].Anchor;
            }

            if (_masking) {
                _masks = new Sprite[reels.Length][];
                for (int i = 0; i < _masks.Length; i++) {
                    _masks [i] = CreateMask (_batches [i]);
                }
            }
        }

        SpriteBatch[] BuildReel (ModelReel reel) {
            int cells = 0;
            for (int i = 0; i < reel.Layers.Length; i++) {
                IconMapping mapping = SpriteManager.Instance.GetMapping (reel.Layers [i]);
                if (mapping.FrameCount > cells) {
                    cells = mapping.FrameCount;
                }
            }

            SpriteBatch[] animation = new SpriteBatch[cells];
            for (int i = 0; i < cells; i++) {
                animation [i] = BuildBatch (reel.Layers, i);
                animation [i].Position = reel.Translation;
            }

            return animation;
        }

        SpriteBatch BuildBatch (IconLayer[] layers, int frame) {
            Texture texture = null;
            Rect2i[] sources = new Rect2i[layers.Length];
            Rect2f[] destinations = new Rect2f[layers.Length];
            Colour[] colours = new Colour[layers.Length];

            for (int i = 0; i < layers.Length; i++) {
                IconMapping mapping = SpriteManager.Instance.GetMapping (layers [i]);
                if (texture == null) {
                    texture = mapping.Texture;
                } else if (texture != mapping.Texture) {
                    throw new SystemException ("Cannot combine sprites from different textures into a single SpriteModel!");
                }

                Sprite sprite = mapping [frame < mapping.FrameCount ? frame : 0];
                sources [i] = sprite.SourceRect;
                destinations [i] = new Rect2f (sprite.Position, sprite.SourceRect.Size);
                colours [i] = layers [i].Colour;
            }

            return new SpriteBatch (texture, sources, destinations, colours);
        }

        Sprite[] CreateMask (SpriteBatch[] batch) {
            Sprite[] masks = new Sprite[batch.Length];
            RenderStates states = RenderStates.DEFAULT;
            states.Shader = _mask;

            GlWrangler.SafeGLDisable (EnableCap.ScissorTest, () => {
                for (int i = 0; i < masks.Length; i++) {
                    RenderTexture texture = new RenderTexture ((int)batch [i].LocalBounds.Size.X);
                    texture.Clear (new Colour (0, 0, 0, 0));
                    texture.Draw (batch [i], states);
                    texture.Display ();
                    masks [i] = new Sprite (texture.Texture);
                }
            });

            return masks;
        }
    }
}

