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
using BLibrary.Util;

namespace BLibrary.Gui.Widgets {

    public sealed class InputColour : Widget {

        static readonly Vect2i PADDING = new Vect2i (4, 4);

        int _selected;

        int Selected {
            get {
                return _selected;
            }
            set {
                _selected = value;
                _fillable.Colour = _colours [_selected];
            }
        }

        public Colour Value {
            get {
                return _colours [_selected];
            }
            set {
                for (int i = 0; i < _colours.Length; i++)
                    if (_colours [i] == value) {
                        Selected = i;
                        return;
                    }

                Selected = 0;
            }
        }

        Colour[] _colours;

        Rectangle _fillable;

        public InputColour (Vect2i position, Vect2i size, string key, Colour[] colours)
            : base (position, size, key) {
            _colours = colours;
            _fillable = new Rectangle (Size - PADDING * 2);

            Selected = 0;
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            states.Transform.Translate (PositionRelative + PADDING);
            target.Draw (_fillable, states);
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
            Selected = Selected < _colours.Length - 1 ? Selected + 1 : Selected = 0;
        }

        void RegressSelection () {
            Selected = Selected > 0 ? Selected - 1 : Selected = _colours.Length - 1;
        }
    }
}

