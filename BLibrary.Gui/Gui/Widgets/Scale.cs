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
using BLibrary.Util;


namespace BLibrary.Gui.Widgets {

    public sealed class Scale : Widget {

        #region Properties

        public Alignment AlignmentH {
            get;
            set;
        }

        public Alignment AlignmentV {
            get;
            set;
        }

        public Colour FillColour {
            get {
                return _fillColour;
            }
            set {
                _fillColour = value;
                _filledSprites = CreateFillSprites (_fillColour);
            }
        }

        public Widget Text {
            get;
            set;
        }

        public float BaseLine {
            get {
                return _baseline;
            }
            set {
                _baseline = value;
            }
        }

        #endregion

        #region Fields

        float _max = 1.0f;
        float _baseline = -1f;

        IDataReference<float> _fill;

        Sprite _texture;

        int _cells;
        Vect2i _cellSize;
        Sprite[] _cellSprites = new Sprite[3];
        Sprite[] _filledSprites;
        Sprite[] _baselineSprites;

        Colour _fillColour;

        #endregion

        public Scale (Vect2i position, Vect2i size, int cells, float max, float scale)
            : this (position, size, cells, max) {
            _fill = new DataPod<float> (scale);
        }

        public Scale (Vect2i position, Vect2i size, int cells, float max, IDataReference<float> scale)
            : this (position, size, cells, max) {
            _fill = scale;
        }

        Scale (Vect2i position, Vect2i size, int cells, float max)
            : base (position, size) {

            _max = max;

            _texture = GuiManager.Instance.GetGuiSprite ("guiScale");

            _cells = cells;
            _cellSize = new Vect2i (_texture.SourceRect.Width / 3, _texture.SourceRect.Height / 2);

            _cellSprites [0] = new Sprite (_texture);
            _cellSprites [0].SourceRect = new Rect2i (_texture.SourceRect.Coordinates, _cellSize.X, _cellSize.Y);
            _cellSprites [1] = new Sprite (_texture);
            _cellSprites [1].SourceRect = new Rect2i (_texture.SourceRect.Coordinates + new Vect2i (_cellSize.X, 0), _cellSize.X, _cellSize.Y);
            _cellSprites [2] = new Sprite (_texture);
            _cellSprites [2].SourceRect = new Rect2i (_texture.SourceRect.Coordinates + new Vect2i (_texture.SourceRect.Width - _cellSize.X, 0), _cellSize.X, _cellSize.Y);

            FillColour = Colour.LightGreen;
        }

        Sprite[] CreateFillSprites (Colour colour) {
            Sprite[] sprites = new Sprite[3];
            sprites [0] = new Sprite (_texture);
            sprites [0].Colour = colour;
            sprites [0].SourceRect = new Rect2i (_texture.SourceRect.Coordinates + new Vect2i (0, _cellSize.Y), _cellSize.X, _cellSize.Y);
            sprites [1] = new Sprite (_texture);
            sprites [1].Colour = colour;
            sprites [1].SourceRect = new Rect2i (_texture.SourceRect.Coordinates + new Vect2i (_cellSize.X, _cellSize.Y), _cellSize.X, _cellSize.Y);
            sprites [2] = new Sprite (_texture);
            sprites [2].Colour = colour;
            sprites [2].SourceRect = new Rect2i (_texture.SourceRect.Coordinates + new Vect2i (_texture.SourceRect.Width - _cellSize.X, _cellSize.Y), _cellSize.X, _cellSize.Y);

            return sprites;

        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            states.Transform.Translate (PositionRelative);
            states.Transform.Translate (
                Alignments.GetAlignH (Size.X, _cellSize.X * _cells, AlignmentH),
                Alignments.GetAlignV (Size.Y, _cellSize.Y, AlignmentV)
            );

            int fill = (int)((_fill.Value / _max) * _cells * _cellSize.X);
            int baseline = (int)((BaseLine / _max) * _cells * _cellSize.X);
            if (BaseLine > 0 && baseline > fill) {
                if (_baselineSprites == null) {
                    _baselineSprites = CreateFillSprites (Colour.DarkOrange);
                }
                DrawScaleFill (target, states, _baselineSprites, baseline);
            }

            if (fill > 0) {
                DrawScaleFill (target, states, _filledSprites, fill);
            }

            if (BaseLine > 0 && baseline < fill) {
                if (_baselineSprites == null) {
                    _baselineSprites = CreateFillSprites (Colour.DarkGreen);
                }
                DrawScaleFill (target, states, _baselineSprites, baseline);
            }

            RenderStates cstates = states;
            target.Draw (_cellSprites [0], cstates);
            for (int i = 1; i < _cells - 1; i++) {
                cstates.Transform.Translate (_cellSize.X, 0);
                target.Draw (_cellSprites [1], cstates);
            }
            cstates.Transform.Translate (_cellSize.X, 0);
            target.Draw (_cellSprites [2], cstates);

            if (Text != null) {
                states.Transform.Translate (
                    Alignments.GetAlignH (_cellSize.X * _cells, Text.Size.X, Alignment.Center),
                    Alignments.GetAlignV (_cellSize.Y, Text.Size.Y, Alignment.Center)
                );
                Text.Draw (target, states);
            }
        }

        void DrawScaleFill (RenderTarget target, RenderStates states, Sprite[] sprites, int fill) {
            DrawFractionedFill (target, states, sprites [0], fill);

            for (int i = 1; i < _cells - 1; i++) {
                if (fill < _cellSize.X * i) {
                    break;
                }
                states.Transform.Translate (_cellSize.X, 0);
                DrawFractionedFill (target, states, sprites [1], fill - i * _cellSize.X);
            }

            if (fill > (_cells - 1) * _cellSize.X) {
                states.Transform.Translate (_cellSize.X, 0);
                DrawFractionedFill (target, states, sprites [2], fill - (_cells - 1) * _cellSize.X);
            }
        }

        void DrawFractionedFill (RenderTarget target, RenderStates states, Sprite cellsprite, int remain) {
            if (remain > _cellSize.X) {
                target.Draw (cellsprite, states);
                return;
            }

            states.Transform.Scale ((float)remain / _cellSize.X, 1f);
            target.Draw (cellsprite, states);
        }
    }
}
