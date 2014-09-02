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
using BLibrary.Graphics.Sprites;
using BLibrary.Util;


namespace BLibrary.Gui.Widgets {

    public sealed class Checkbox : Widget {
        #region Classes

        abstract class CheckState {
            public abstract bool State { get; }
        }

        sealed class FixedState : CheckState {
            public override bool State {
                get {
                    return _state;
                }
            }

            bool _state;

            public FixedState (bool state) {
                _state = state;
            }
        }

        sealed class ContainsUIDState : CheckState {
            public override bool State {
                get {
                    return _set.Contains (_contained);
                }
            }

            HashSet<uint> _set;
            uint _contained;

            public ContainsUIDState (HashSet<uint> hashset, uint contained) {
                _set = hashset;
                _contained = contained;
            }
        }

        sealed class ContainsSerialState : CheckState {
            public override bool State {
                get {
                    return _set.Contains (_contained);
                }
            }

            HashSet<ulong> _set;
            ulong _contained;

            public ContainsSerialState (HashSet<ulong> hashset, ulong contained) {
                _set = hashset;
                _contained = contained;
            }
        }

        #endregion

        CheckState CheckedState { get; set; }

        Sprite _cell;
        Sprite _fill;

        public Checkbox (Vect2i position, Vect2i size, string key, bool state)
            : this (position, size, key, new FixedState (state)) {
        }

        public Checkbox (Vect2i position, Vect2i size, string key, HashSet<uint> hashset, uint contained)
            : this (position, size, key, new ContainsUIDState (hashset, contained)) {
        }

        public Checkbox (Vect2i position, Vect2i size, string key, HashSet<ulong> hashset, ulong contained)
            : this (position, size, key, new ContainsSerialState (hashset, contained)) {
        }

        Checkbox (Vect2i position, Vect2i size, string key, CheckState state)
            : base (position, size, key) {

            Sprite template = GuiManager.Instance.GetGuiSprite ("guiCheckbox");

            _cell = new Sprite (template);
            int cellWidth = _cell.SourceRect.Width / 2;
            Vect2i texCoords = _cell.SourceRect.Coordinates;
            _cell.SourceRect = new Rect2i (texCoords + new Vect2i (cellWidth, 0), cellWidth, _cell.SourceRect.Height);

            _fill = new Sprite (template);
            _fill.Colour = Colour.LimeGreen;
            _fill.SourceRect = new Rect2i (texCoords, cellWidth, _cell.SourceRect.Height);

            CheckedState = state;
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            states.Transform.Translate (PositionRelative.X, PositionRelative.Y);
            states.Transform.Translate ((Size - _cell.SourceRect.Size) / 2);
            if (CheckedState.State) {
                target.Draw (_fill, states);
            }
            target.Draw (_cell, states);
        }
    }
}
