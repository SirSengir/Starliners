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

﻿using System;
using System.Collections.Generic;
using QuickFont;
using BLibrary.Graphics;
using BLibrary.Graphics.Text;
using System.Drawing;

namespace BLibrary {

    /// <summary>
    /// Collects multiple related fonts for different font styles to make handling easier.
    /// </summary>
    public sealed class FontCollection {
        #region Properties

        /// <summary>
        /// Gets the font associated with the specified style in this collection.
        /// </summary>
        /// <param name="style">Style.</param>
        public QFont this [FontStyle style] {
            get { return _fonts.ContainsKey (style) ? _fonts [style] : _fonts [FontStyle.Regular]; }
        }

        QFont _basic;
        Dictionary<FontStyle, QFont> _fonts = new Dictionary<FontStyle, QFont> ();

        public float LineSpacing {
            get { return _basic.LineSpacing; }
        }

        public float MaxGlyphSpace {
            get;
            private set;
        }

        Texture[] _textures;

        /// <summary>
        /// Gets the list of texture pages contained within the fonts of this collection.
        /// </summary>
        /// <value>The texture pages.</value>
        public Texture[] TexturePages {
            get {

                if (_textures == null) {
                    List<Texture> pages = new List<Texture> ();
                    foreach (KeyValuePair<FontStyle, QFont> entry in _fonts) {
                        if (entry.Value.Options.DropShadowActive) {
                            foreach (TexturePage page in entry.Value.FontData.DropShadow.FontData.Pages) {
                                pages.Add (new Texture (page.GLTexID, page.Width, page.Height));
                            }
                        }
                        foreach (TexturePage page in entry.Value.FontData.Pages) {
                            pages.Add (new Texture (page.GLTexID, page.Width, page.Height));
                        }
                    }
                    _textures = pages.ToArray ();
                }

                return _textures;
            }
        }

        #endregion

        #region Constructor

        public FontCollection (QFont basic) {
            _basic = basic;
            AddFont (FontStyle.Regular, _basic);
            CalculateMaxGlyphSpace ();
        }

        #endregion

        void CalculateMaxGlyphSpace () {
            MaxGlyphSpace = 0;
            foreach (var glyph in _basic.FontData.CharSetMapping)
                MaxGlyphSpace = Math.Max (glyph.Value.Rect.Height + glyph.Value.YOffset, MaxGlyphSpace);

        }

        /// <summary>
        /// Adds the given font to be used for the given style to the FontCollection.
        /// </summary>
        /// <param name="style">Style.</param>
        /// <param name="font">Font.</param>
        public void AddFont (FontStyle style, QFont font) {
            _fonts [style] = font;
            _textures = null;
        }

        /// <summary>
        /// Returns the kerning length correction for the character at the given index in the given string.
        /// Also, if the text is part of a textNode list, the nextNode is given so that the following 
        /// node can be checked incase of two adjacent word nodes.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="text"></param>
        /// <param name="textNode"></param>
        /// <returns></returns>
        internal int GetKerningPairCorrection (FontStyle style, int index, string text, TextNode textNode) {
            QFontData fontData = this [style].FontData;

            if (fontData.KerningPairs == null) {
                return 0;
            }

            char[] chars = new char[2];

            if (index + 1 == text.Length) {
                if (textNode != null && textNode.Next != null && textNode.Next.Type == TextNodeType.Word)
                    chars [1] = textNode.Next.Text [0];
                else
                    return 0;
            } else {
                chars [1] = text [index + 1];
            }

            chars [0] = text [index];

            String str = new String (chars);
            if (fontData.KerningPairs.ContainsKey (str)) {
                return fontData.KerningPairs [str];
            }

            return 0;

        }

        /// <summary>
        /// Determines whether this instance can render the specified char.
        /// </summary>
        /// <returns><c>true</c> if this instance can render the specified char; otherwise, <c>false</c>.</returns>
        /// <param name="c">C.</param>
        public bool CanRenderChar (char c) {
            return _basic.FontData.CharSetMapping.ContainsKey (c);
        }
    }
}

