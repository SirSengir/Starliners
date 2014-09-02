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
using BLibrary.Graphics;
using BLibrary;
using BLibrary.Graphics.Text;
using Starliners;


namespace BLibrary.Gui.Widgets {

    public class Label : Widget {
        #region Properties

        public FontDefinition Style {
            get { return _style; }
            set {
                _style = value;
                RebuildBuffer ();
            }
        }

        public TextFormat Formatting {
            get { return _format; }
            set {
                _format = value;
                _setformat = true;
                RebuildBuffer ();
            }
        }

        public Alignment AlignmentH {
            get;
            set;
        }

        public Alignment AlignmentV {
            get;
            set;
        }

        public object Template {
            get;
            set;
        }

        #endregion

        FontDefinition _style;
        TextFormat _format;
        bool _setformat;

        object[] _keys;
        bool _fixed;
        TextBuffer _buffer;

        #region Constructor

        public Label (Vect2i position, object label)
            : this (position, label, null) {
        }

        public Label (Vect2i position, object template, params object[] keys)
            : this (position, new Vect2i (0, 0), template, keys) {
            Size = (Vect2i)GetDimensions (int.MaxValue);
        }

        public Label (Vect2i position, TextBuffer buffer)
            : base (position, (Vect2i)buffer.LocalBounds.Size) {
            _fixed = true;
            Style = FontManager.Instance [FontManager.BASIC];
            AlignmentH = Alignment.Left;
            AlignmentV = Alignment.Left;
            _buffer = buffer;
        }

        public Label (Vect2i position, Vect2i size, object label)
            : this (position, size, label, null) {
        }

        public Label (Vect2i position, Vect2i size, object template, params object[] keys)
            : base (position, size) {
            Style = FontManager.Instance [FontManager.BASIC];
            AlignmentH = Alignment.Left;
            AlignmentV = Alignment.Left;
            Template = template;
            _keys = keys;

        }

        #endregion

        void RebuildBuffer () {
            if (!_fixed) {
                _buffer = _setformat ? new TextBuffer (Style, GetString ()) { Formatting = Formatting } : new TextBuffer (Style, GetString ());
            }
        }

        string GetString () {

            string str;
            if (_keys == null) {
                str = Template != null ? Template.ToString () : string.Empty;
            } else {
                str = string.Format (Template.ToString (), _keys);
            }

            return str;
        }

        public Vect2f GetDimensions (int maxWidth) {
            _buffer.Text = GetString ();
            return _buffer.LocalBounds.Size;
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            states.Transform.Translate (PositionRelative);
            DrawBackground (target, states);

            if (!_fixed) {
                _buffer.Text = GetString ();
                _buffer.HAlign = AlignmentH;
                _buffer.VAlign = AlignmentV;
                _buffer.Box = Size;
            }
            _buffer.Draw (target, states);
        }
    }
}
