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
using System.IO;
using System.Text;
using BLibrary.Util;
using BLibrary.Graphics;
using BLibrary.Graphics.Text;

namespace BLibrary.Gui.Widgets {

    public sealed class TextView : Scrollable {
        #region Properties

        public Alignment AlignmentH { get; set; }

        public Alignment AlignmentV { get; set; }

        #endregion

        TextBuffer _buffer;
        ITextProvider[] _text;
        long _lastUpdated;

        #region Constructor

        public TextView (Vect2i position, Vect2i size, string key, string text)
            : this (position, size, key) {

            if ("LOREM".Equals (text)) {
                text = @"Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.   

Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi. Lorem ipsum dolor sit amet.";
            }

            SetText (TextComponent.ConvertToComponents (text.Replace ("\r\n", "\n").Split ('\n')));
        }

        public TextView (Vect2i position, Vect2i size, string key, ITextProvider text)
            : this (position, size, key) {
            SetText (new ITextProvider[] { text });
        }

        public TextView (Vect2i position, Vect2i size, string key, params string[] lines)
            : this (position, size, key) {

            SetText (TextComponent.ConvertToComponents (lines));
        }

        public TextView (Vect2i position, Vect2i size, string key, ResourceFile resource)
            : this (position, size, key) {

            string[] lines = null;
            using (StreamReader reader = new StreamReader (resource.OpenRead (), Encoding.Default)) {
                lines = reader.ReadToEnd ().Replace ("\r\n", "\n").Split ('\n');
            }
            SetText (TextComponent.ConvertToComponents (lines));
        }

        TextView (Vect2i position, Vect2i size, string key)
            : base (position, size, key) {
            AlignmentH = Alignment.Left;
            AlignmentV = Alignment.Left;

            _buffer = new TextBuffer (key);
        }

        #endregion

        void SetText (ITextProvider[] text) {
            _text = text;
            IsGenerated = false;
        }

        public override void Update () {
            base.Update ();

            // Marks the widget for regeneration if any of its
            // text components have been updated.
            for (int i = 0; i < _text.Length; i++) {
                if (_text [i].LastUpdated > _lastUpdated) {
                    IsGenerated = false;
                    _lastUpdated = _text [i].LastUpdated;
                    break;
                }
            }
        }

        protected override void Regenerate () {
            base.Regenerate ();

            if (_text == null || _text.Length <= 0) {
                _buffer.Text = string.Empty;
            } else {
                StringBuilder builder = new StringBuilder (_text [0].ToString ());
                for (int i = 1; i < _text.Length; i++) {
                    builder.AppendLine ();
                    builder.Append (_text [i].ToString ());
                }
                _buffer.Text = builder.ToString ();
            }
            RefreshDimensions ();
        }

        protected override Vect2f DetermineDimensions (int fixedWidth) {
            _buffer.SetMaxWidth (fixedWidth);
            return _buffer.LocalBounds.Size;
        }

        protected override void DrawPort (RenderTarget target, RenderStates states) {
            _buffer.Box = new Vect2i ((int)EffectiveSize.X, (int)EffectiveSize.Y);
            _buffer.HAlign = AlignmentH;
            _buffer.VAlign = AlignmentV;
            _buffer.Draw (target, states);
        }

        public void ResetText (params string[] lines) {
            SetText (TextComponent.ConvertToComponents (lines));
        }
    }
}
