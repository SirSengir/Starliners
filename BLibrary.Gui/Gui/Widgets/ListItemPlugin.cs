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

﻿using BLibrary.Resources;
using OpenTK.Input;
using BLibrary.Graphics;
using BLibrary.Graphics.Text;
using BLibrary.Util;

namespace BLibrary.Gui.Widgets {

    public sealed class ListItemPlugin : Widget {
        ResourceCollection _collection;
        TextBuffer _bufferName;
        TextBuffer _bufferVersion;
        TextBuffer _bufferState;

        public ListItemPlugin (Vect2i position, Vect2i size, string key, ResourceCollection collection)
            : base (position, size, key) {

            _collection = collection;

            _bufferName = new TextBuffer (_collection.Name);
            _bufferName.Box = new Vect2i ((Size.X - 128 - 96), Size.Y);
            _bufferName.VAlign = Alignment.Center;

            _bufferVersion = new TextBuffer (_collection.Version.ToString ());
            _bufferVersion.Box = new Vect2i (96, Size.Y);
            _bufferVersion.HAlign = Alignment.Center;
            _bufferVersion.VAlign = Alignment.Center;

            _bufferState = new TextBuffer ("Unknown");
            _bufferState.Box = new Vect2i (128, Size.Y);
            _bufferState.HAlign = Alignment.Center;
            _bufferState.VAlign = Alignment.Center;
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            states.Transform.Translate (PositionRelative.X, PositionRelative.Y);
            _bufferName.Draw (target, states);
            states.Transform.Translate (Size.X - 128 - 96, 0);
            _bufferVersion.Draw (target, states);
            states.Transform.Translate (96, 0);
            _bufferState.Text = _collection.IsBuiltin ? string.Format ("§#{0}§Built-in", Colour.Yellow.ToString ("X")) : _collection.IsEnabled ? string.Format ("§#{0}§Enabled", Colour.Chartreuse.ToString ("X")) : string.Format ("§#{0}§Disabled", Colour.Crimson.ToString ("X"));
            _bufferState.Draw (target, states);

        }

        public override bool HandleMouseClick (Vect2i coordinates, MouseButton button) {
            if (!IntersectsWith (coordinates)) {
                return base.HandleMouseClick (coordinates, button);
            }
            Window.DoAction ("plugin.slot.clicked", _collection);
            return true;
        }
    }
}
