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

﻿using BLibrary.Util;
using Starliners.Game;
using Starliners.Graphics;

namespace BLibrary.Gui.Widgets {

    public sealed class BlazonEditor : Widget {
        Blazon _original;
        InputIcon _pattern;
        InputColour _colour0;
        InputColour _colour1;
        InputIcon _heraldic;
        InputColour _colour2;

        public BlazonEditor (Vect2i position, Vect2i size, string key, Blazon blazon)
            : base (position, size, key) {

            _original = blazon;

            int padding = 4;
            int column = Size.X / 4;

            int paddedColumn = (Size.X - 8 * padding) / 4;
            Vect2i sizeInputIcon = Size.Y - 2 * padding > column ? new Vect2i (column - 2 * padding, column - 2 * padding) : new Vect2i (Size.Y, Size.Y) - new Vect2i (2 * padding, 2 * padding);

            int paddedRow = (sizeInputIcon.Y - 2 * padding) / 2;
            Vect2i sizeInputColour = new Vect2i (paddedColumn, paddedRow);

            string[] patternkeys = new string[Blazon.VALID_PATTERNS.Length];
            for (int i = 0; i < patternkeys.Length; i++)
                patternkeys [i] = RendererBlazon.PREFIX_KEY_PATTERN + Blazon.VALID_PATTERNS [i];
            AddWidget (_pattern = new InputIcon (new Vect2i (padding, padding), sizeInputIcon, key + ".pattern", patternkeys, Blazon.VALID_PATTERNS) {
                Backgrounds = UIProvider.Style.ButtonStyle.CreateBackgrounds (),
                Value = blazon.Pattern
            });

            AddWidget (_colour0 = new InputColour (new Vect2i (column + padding, padding), sizeInputColour, key + ".colour0", Blazon.VALID_COLOURS) {
                Backgrounds = UIProvider.Style.ButtonStyle.CreateBackgrounds (),
                Value = blazon.Colour0
            });
            AddWidget (_colour1 = new InputColour (new Vect2i (column + padding, paddedRow + 3 * padding), sizeInputColour, key + ".colour1", Blazon.VALID_COLOURS) {
                Backgrounds = UIProvider.Style.ButtonStyle.CreateBackgrounds (),
                Value = blazon.Colour1
            });

            string[] heraldickeys = new string[Blazon.VALID_HERALDICS.Length];
            for (int i = 0; i < heraldickeys.Length; i++)
                heraldickeys [i] = RendererBlazon.PREFIX_KEY_HERALDIC + Blazon.VALID_HERALDICS [i];
            AddWidget (_heraldic = new InputIcon (new Vect2i (2 * column + padding, padding), sizeInputIcon, key + ".heraldic", heraldickeys, Blazon.VALID_HERALDICS) {
                Backgrounds = UIProvider.Style.ButtonStyle.CreateBackgrounds (),
                Value = blazon.Heraldic
            });

            AddWidget (_colour2 = new InputColour (new Vect2i (3 * column + padding, padding), sizeInputColour, key + ".colour2", Blazon.VALID_COLOURS) {
                Backgrounds = UIProvider.Style.ButtonStyle.CreateBackgrounds (),
                Value = blazon.Colour2
            });
        }

        public Blazon CreateBlazon () {
            return new Blazon (_original.Shape) {
                Pattern = _pattern.Value,
                Colour0 = _colour0.Value,
                Colour1 = _colour1.Value,
                Heraldic = _heraldic.Value,
                Colour2 = _colour2.Value
            };
        }

        public override bool HandleMouseClick (Vect2i coordinates, OpenTK.Input.MouseButton button) {
            if (!base.HandleMouseClick (coordinates, button))
                return false;

            RendererBlazon.Instance.SetDressup (CreateBlazon ());
            return true;
        }
    }
}

