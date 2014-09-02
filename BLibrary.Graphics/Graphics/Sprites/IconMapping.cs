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
using BLibrary.Graphics;
using BLibrary.Util;


namespace BLibrary.Graphics.Sprites {

    /// <summary>
    /// Contains information about a sprite on a texture sheet.
    /// </summary>
    internal abstract class IconMapping {
        #region Properties

        public abstract Sprite this [AnimationClock clock] {
            get;
        }

        public abstract Sprite this [Colour colour] {
            get;
        }

        public abstract Sprite this [int index] {
            get;
        }

        /// <summary>
        /// Gets or sets the amount of different frames contained in the mapping.
        /// </summary>
        /// <value>The frame count.</value>
        public int FrameCount {
            get;
            private set;
        }

        public Texture Texture {
            get;
            private set;
        }

        public abstract Rect2i[] Parts { get; }

        #endregion

        #region Constructor

        public IconMapping (Texture texture, int frameCount) {
            Texture = texture;
            FrameCount = frameCount;
        }

        #endregion

        public virtual void Adjust (Texture sheet, Rect2i[] parts) {
            Texture = sheet;
        }
    }

    internal sealed class SimpleMapping : IconMapping {
        #region Properties

        public override Sprite this [AnimationClock clock] {
            get {
                return this [Colour.White];
            }
        }

        public override Sprite this [Colour colour] {
            get {
                if (!_coloured.ContainsKey (colour)) {
                    _coloured [colour] = new Sprite (_sprite);
                    _coloured [colour].Colour = colour;
                }
                return _coloured [colour];
            }
        }

        public override Sprite this [int index] {
            get {
                return this [Colour.White];
            }
        }

        public override Rect2i[] Parts {
            get {
                return new Rect2i[] { _sprite.SourceRect };
            }
        }

        #endregion

        #region Fields

        Sprite _sprite;
        Dictionary<Colour, Sprite> _coloured = new Dictionary<Colour, Sprite> ();

        #endregion

        public SimpleMapping (Texture texture, Vect2f position, Rect2i rect)
            : base (texture, 1) {
            _sprite = new Sprite (texture);
            _sprite.Position = position;
            _sprite.SourceRect = rect;
        }

        public override void Adjust (Texture sheet, Rect2i[] parts) {
            base.Adjust (sheet, parts);
            _coloured.Clear ();

            _sprite = new Sprite (sheet, _sprite);
            _sprite.SourceRect = parts [0];
        }
    }

}
