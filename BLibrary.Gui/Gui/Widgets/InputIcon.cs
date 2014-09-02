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

﻿using OpenTK.Input;
using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using BLibrary.Util;

namespace BLibrary.Gui.Widgets {

    public sealed class InputIcon : Widget {

        static readonly Vect2i PADDING = new Vect2i (4, 4);

        int _selected;

        int Selected {
            get {
                return _selected;
            }
            set {
                _selected = value;
            }
        }

        public string Value {
            get {
                return _values [_selected];
            }
            set {
                for (int i = 0; i < _values.Length; i++)
                    if (_values [i] == value) {
                        Selected = i;
                        return;
                    }

                Selected = 0;
            }
        }

        string[] _values;
        string[] _icons;

        uint[] _indices;

        public InputIcon (Vect2i position, Vect2i size, string key, string[] icons)
            : this (position, size, key, icons, icons) {
        }

        public InputIcon (Vect2i position, Vect2i size, string key, string[] icons, string[] values)
            : base (position, size, key) {
            _icons = icons;
            _values = values;

            _indices = new uint[_icons.Length];
            for (int i = 0; i < _icons.Length; i++)
                _indices [i] = SpriteManager.Instance.RegisterSingle (_icons [i]);
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            Sprite icon = SpriteManager.Instance [_indices [_selected]];
            states.Transform.Translate (PositionRelative + PADDING);
            states.Transform.Scale ((Size - PADDING * 2) / icon.SourceRect.Size);
            target.Draw (icon, states);
        }

        public override bool HandleMouseClick (Vect2i coordinates, MouseButton button) {
            base.HandleMouseClick (coordinates, button);

            if (!IntersectsWith (coordinates))
                return false;
            if (button == MouseButton.Left)
                AdvanceSelection ();
            else if (button == MouseButton.Right)
                RegressSelection ();

            return true;
        }

        void AdvanceSelection () {
            Selected = Selected < _icons.Length - 1 ? Selected + 1 : Selected = 0;
        }

        void RegressSelection () {
            Selected = Selected > 0 ? Selected - 1 : Selected = _icons.Length - 1;
        }
    }
}

