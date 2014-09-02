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
using BLibrary.Graphics;
using BLibrary.Graphics.Text;
using BLibrary.Graphics.Sprites;
using BLibrary.Gui.Data;
using BLibrary.Util;
using Starliners;


namespace BLibrary.Gui.Widgets {

    #region RichParagraph Implementations
    /// <summary>
    /// Wraps simple text in a RichParagraph.
    /// </summary>
    sealed class ParagraphText : RichParagraph {
        TextBuffer _header;
        TextBuffer _text;

        public ParagraphText (IParagraphProvider provider)
            : base (provider) {

            // Set the header if needed.
            if (!string.IsNullOrWhiteSpace (provider.Header)) {
                _header = new TextBuffer (string.Format (Tooltip.HEADER_FORMAT, provider.Header));
            }

            string text = provider.Content;
            if ("LOREM".Equals (text)) {
                text = @"Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.   

Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi. Lorem ipsum dolor sit amet.";
            }
            //_text = text.Replace("\r\n", "\n").Split('\n');
            _text = new TextBuffer (text);
        }

        public override Vect2f GetDimensions (int fixedWith) {
            _text.SetMaxWidth (fixedWith);

            Vect2f dim = _text.LocalBounds.Size;
            if (_header != null) {
                _header.SetMaxWidth (fixedWith);
                dim += new Vect2f (0, _header.LocalBounds.Size.Y) + new Vect2f (0, FontManager.Instance.Regular.LineSpacing);
            }
            return dim + new Vect2f (0, FontManager.Instance.Regular.LineSpacing);
        }

        public override void Draw (RenderTarget target, RenderStates states, int width, int height) {
            if (_header != null) {
                _header.Box = new Vect2i (width, height);
                _header.Draw (target, states);
                states.Transform.Translate (0, _header.LocalBounds.Height + _header.Fonts.LineSpacing);
            }

            _text.HAlign = Provider.AlignmentH;
            _text.VAlign = Provider.AlignmentV;
            _text.Box = new Vect2i (width, height);
            _text.Draw (target, states);
        }
    }

    /// <summary>
    /// Wraps a single image in a RichParagraph
    /// </summary>
    sealed class ParagraphImage : RichParagraph {
        Sprite _image;
        int _lastScale;

        public ParagraphImage (IParagraphProvider provider)
            : base (provider) {

            _image = new Sprite (SpriteManager.Instance.LoadTexture (provider.Content));
        }

        public override Vect2f GetDimensions (int fixedWith) {
            return _image.LocalBounds.Size + new Vect2f (0, FontManager.Instance.Regular.LineSpacing);
        }

        public override void Draw (RenderTarget target, RenderStates states, int width, int height) {
            if (_lastScale != width && width < _image.LocalBounds.Size.X) {
                _image.Scale = new Vect2f (1.0f * ((float)width / _image.LocalBounds.Size.X), 1.0f);
                _lastScale = width;
            }
            target.Draw (_image, states);
        }
    }
    #endregion
    #region Default ParagraphProvider Implementations
    /// <summary>
    /// Very basic implementation of IParagraphProvider to wrap simple text.
    /// </summary>
    sealed class ParagraphProvider : IParagraphProvider {
        public ParagraphType Type { get; set; }

        public string Header { get; set; }

        public string Content { get; set; }

        public Alignment AlignmentH { get; set; }

        public Alignment AlignmentV { get; set; }
    }
    #endregion
    /// <summary>
    /// Wraps a paragraph provider for rendering.
    /// </summary>
    public abstract class RichParagraph {
        public IParagraphProvider Provider {
            get;
            private set;
        }

        protected RichParagraph (IParagraphProvider provider) {
            Provider = provider;
        }

        public abstract Vect2f GetDimensions (int fixedWith);

        public abstract void Draw (RenderTarget target, RenderStates states, int width, int height);

        /// <summary>
        /// Create a RichParagraph from the given IParagraphProvider.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static RichParagraph MakeRenderable (IParagraphProvider provider) {
            switch (provider.Type) {
                case ParagraphType.Text:
                    return new ParagraphText (provider);
                case ParagraphType.Image:
                    return new ParagraphImage (provider);
                default:
                    throw new SystemException ("Cannot handle a paragraph of type: " + provider.Type);
            }
        }

        /// <summary>
        /// Create a RichParagraph from the given text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static RichParagraph MakeRenderable (string text) {
            return new ParagraphText (new ParagraphProvider () {
                Type = ParagraphType.Text,
                Content = text
            });
        }
    }
}
