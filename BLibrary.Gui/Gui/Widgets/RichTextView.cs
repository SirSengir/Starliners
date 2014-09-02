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
using BLibrary.Graphics;
using BLibrary.Util;

namespace BLibrary.Gui.Widgets {

    public sealed class RichTextView : Scrollable {
        public Alignment AlignmentH { get; set; }

        public Alignment AlignmentV { get; set; }

        /// <summary>
        /// Paragraphs inside the view.
        /// </summary>
        public RichParagraph[] Paragraphs { get; set; }

        Vect2i[] _elementDimensions;

        #region Constructor

        public RichTextView (Vect2i position, Vect2i size, string key, ResourceFile resource)
            : this (position, size, key) {

            string text;
            using (StreamReader reader = new StreamReader (resource.OpenRead (), Encoding.Default)) {
                text = reader.ReadToEnd ().Replace ("\r\n", "\n");
            }

            Paragraphs = new RichParagraph[] { RichParagraph.MakeRenderable (text) };
            RefreshDimensions ();
        }

        public RichTextView (Vect2i position, Vect2i size, string key, params RichParagraph[] parafs)
            : this (position, size, key) {

            Paragraphs = parafs;
            RefreshDimensions ();
        }

        RichTextView (Vect2i position, Vect2i size, string key)
            : base (position, size, key) {
            AlignmentH = Alignment.Left;
            AlignmentV = Alignment.Left;
        }

        #endregion

        protected override Vect2f DetermineDimensions (int fixedWidth) {
            Vect2f dim = new Vect2f ();
            if (_elementDimensions == null || _elementDimensions.Length != Paragraphs.Length)
                _elementDimensions = new Vect2i[Paragraphs.Length];

            for (int i = 0; i < Paragraphs.Length; i++) {
                _elementDimensions [i] = (Vect2i)Paragraphs [i].GetDimensions (fixedWidth);
                dim += _elementDimensions [i];
            }

            return dim;
        }

        protected override void DrawPort (RenderTarget target, RenderStates states) {
            int lastElementHeight = 0;
            int drawnHeight = 0;

            for (int i = 0; i < Paragraphs.Length; i++) {
                states.Transform.Translate (0, lastElementHeight);
                lastElementHeight = _elementDimensions [i].Y;
                drawnHeight += lastElementHeight;

                // Do not draw yet, if we are still drawing into an area which isn't visible yet.
                if (drawnHeight < Scroll.Y)
                    continue;

                Paragraphs [i].Provider.AlignmentH = AlignmentH;
                Paragraphs [i].Provider.AlignmentV = AlignmentV;
                Paragraphs [i].Draw (target, states, (int)EffectiveSize.X, (int)EffectiveSize.Y);

                // Stop if we are already exceeding the visible area of this view.
                if (drawnHeight > Scroll.Y + Size.Y)
                    break;
            }
        }
    }
}
