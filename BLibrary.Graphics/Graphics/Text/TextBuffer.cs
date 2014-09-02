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

﻿using OpenTK;
using QuickFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BLibrary.Util;
using Starliners;


namespace BLibrary.Graphics.Text {

    public sealed class TextBuffer : Transformable, IDrawable, IDisposable {
        #region Properties

        Rect2f _bounds;

        /// <summary>
        /// Size of the actual rendered text.
        /// </summary>
        /// <value>The local bounds.</value>
        public override Rect2f LocalBounds {
            get {
                VerifyClean ();
                return _bounds;
            }
        }

        string _text;

        /// <summary>
        /// Text this buffer contains and renders.
        /// </summary>
        public string Text {
            get {
                return _text;
            }
            set {
                if (value != _text) {
                    _text = value;
                    _dirtyBuffers = true;
                }
            }
        }

        Alignment _hAlign;

        public Alignment HAlign {
            get { return _hAlign; }
            set {
                if (_hAlign != value) {
                    _hAlign = value;
                    _dirtyBuffers = true;
                }
            }
        }

        Alignment _vAlign;

        public Alignment VAlign {
            get { return _vAlign; }
            set {
                if (_vAlign != value) {
                    _vAlign = value;
                    _dirtyBuffers = true;
                }
            }
        }

        Vect2i _area;

        /// <summary>
        /// The maximum area available to the font for rendering.
        /// </summary>
        /// <value>The box.</value>
        public Vect2i Box {
            get { return _area; }
            set {
                if (_area != value) {
                    _area = value;
                    _dirtyBuffers = true;
                }
            }
        }

        FontCollection _fonts;

        /// <summary>
        /// The fonts to use for rendering.
        /// </summary>
        /// <value>The fonts.</value>
        public FontCollection Fonts {
            get { return _fonts; }
            private set {
                _fonts = value;
                CreateBuffer ();
            }
        }

        TextFormat _format;

        /// <summary>
        /// Contains the formatting to use for text without formatting information.
        /// </summary>
        /// <value>The formatting.</value>
        public TextFormat Formatting {
            get { return _format; }
            set {
                if (_format != value) {
                    _format = value;
                    _dirtyBuffers = true;
                }
            }
        }

        #endregion

        bool _dirtyBuffers = true;
        VertexBuffer[] _vertexBuffers;
        Dictionary<int, VertexBuffer> _sortedBuffers = new Dictionary<int, VertexBuffer> ();

        #region Constructors

        public TextBuffer (FontDefinition style, params string[] text)
            : this (style.Fonts, style.Formatting) {
            SetLines (text);
        }

        public TextBuffer (FontDefinition style, string text)
            : this (style.Fonts, style.Formatting) {
            Text = text;
        }

        public TextBuffer (params IList<string>[] text)
            : this (FontManager.Instance.Regular, new TextFormat (Colour.White)) {
            IList<string> combined = text [0];
            for (int i = 1; i < text.Length; i++) {
                for (int j = 0; j < text [i].Count; j++) {
                    combined.Add (text [i] [j]);
                }
            }
            SetLines (combined.ToArray ());
        }

        public TextBuffer (params string[] text)
            : this (FontManager.Instance.Regular, new TextFormat (Colour.White)) {
            SetLines (text);
        }

        public TextBuffer (string text)
            : this (FontManager.Instance.Regular, new TextFormat (Colour.White)) {
            Text = text;
        }

        TextBuffer (FontCollection fonts, TextFormat formatting) {
            Fonts = fonts;
            Formatting = formatting;
        }

        #endregion

        #region IDisposable

        bool _disposed = false;

        public void Dispose () {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        void Dispose (bool manual) {
            if (_disposed) {
                return;
            }

            if (manual) {
                for (int i = 0; i < _vertexBuffers.Length; i++) {
                    _vertexBuffers [i].Dispose ();
                }
            }
            _disposed = true;
        }

        #endregion

        public void Draw (RenderTarget target, RenderStates states) {

            // Update the buffer if it is dirty.
            VerifyClean ();

            // Align vertically.
            if (VAlign == Alignment.Bottom) {
                states.Transform.Translate (0, (int)(Box.Y - LocalBounds.Size.Y));
            } else if (VAlign == Alignment.Center) {
                states.Transform.Translate (0, (int)(Box.Y - LocalBounds.Size.Y) / 2);
            }

            // Combine transform and states
            states.Transform *= Transform;

            //GLWrangler.BindVertexArray (0);
            target.Draw (_vertexBuffers, states);
        }

        void VerifyClean () {
            if (_dirtyBuffers) {

                RenderTarget.VBVertUpdates++;
                ResetVBOs ();
                _bounds = new Rect2f (Position, PrintOrMeasure (_text, Box.X, HAlign, false));
                LoadVBOs ();

                _dirtyBuffers = false;
            }
        }

        /// <summary>
        /// Sets the given strings as lines of a text, concating them into a single continuous string.
        /// </summary>
        /// <param name="lines">Lines.</param>
        public void SetLines (params string[] text) {
            Text = ConcatString (text);
        }

        /// <summary>
        /// Sets the maximum width available for this buffer to render to.
        /// </summary>
        /// <remarks>Equal to TextBuffer.Box = new Vect2i(width, Box.Y). Causes a VBO Update.</remarks>
        /// <param name="width">Width.</param>
        public void SetMaxWidth (int width) {
            if (Box.X == width) {
                return;
            }

            Box = new Vect2i (width, Box.Y);
        }

        /// <summary>
        /// Sets the maximum height available for this buffer to render to.
        /// </summary>
        /// <remarks>Equal to TextBuffer.Box = new Vect2i(Box.X, height). Causes a VBO Update.</remarks>
        /// <param name="width">Width.</param>
        public void SetMaxHeight (int height) {
            if (Box.Y == height) {
                return;
            }

            Box = new Vect2i (Box.X, height);
        }

        #region Buffer Updates

        void CreateBuffer () {

            // Grab all texture pages existing in the used fontcollection.
            Texture[] pages = Fonts.TexturePages;
            // Prepare the sorted collection and the flat array.
            _sortedBuffers.Clear ();

            for (int i = 0; i < pages.Length; i++) {
                _sortedBuffers [pages [i].GlTexId] = new VertexBuffer (pages [i]);
            }

            _vertexBuffers = _sortedBuffers.Values.ToArray ();
        }

        void ResetVBOs () {
            for (int i = 0; i < _vertexBuffers.Length; i++)
                _vertexBuffers [i].Reset ();
        }

        void LoadVBOs () {
            for (int i = 0; i < _vertexBuffers.Length; i++)
                _vertexBuffers [i].Load ();
        }

        void DrawVBOs () {
            for (int i = 0; i < _vertexBuffers.Length; i++)
                _vertexBuffers [i].Draw ();
        }

        #endregion

        #region Text Rendering

        /// <summary>
        /// Creates node list object associated with the text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        TextTokenized ProcessText (string text, float maxWidth) {

            maxWidth = maxWidth > 0 ? maxWidth : float.MaxValue;

            TextNodeList nodeList = new TextNodeList (text, Formatting);
            nodeList.MeasureNodes (Fonts);

            // We "crumble" words that are too long so that that can be split up
            List<TextNode> nodesToCrumble = new List<TextNode> ();
            foreach (TextNode node in nodeList) {
                if (node.Length >= maxWidth && node.Type == TextNodeType.Word)
                    nodesToCrumble.Add (node);
            }

            foreach (TextNode node in nodesToCrumble) {
                nodeList.Crumble (node, 1);
            }

            // Need to re-measure crumbled words
            nodeList.MeasureNodes (Fonts);

            return new TextTokenized (nodeList, maxWidth);
        }

        Vect2f PrintOrMeasure (string text, float maxWidth, Alignment alignment, bool measureOnly) {

            if (text == null) {
                return new Vect2f ();
            }

            TextTokenized tokenizedText = ProcessText (text, maxWidth);

            float maxMeasuredWidth = 0f;

            maxWidth = tokenizedText.MaxWidth;

            TextNodeList nodeList = tokenizedText.TextNodeList;
            for (TextNode node = nodeList.Head; node != null; node = node.Next) {
                node.LengthTweak = 0f;  //reset tweaks
            }

            float xOffset = GetXOffset (nodeList.Head, maxWidth, alignment);
            float yOffset = 0f;

            bool atLeastOneNodeCosumedOnLine = false;
            float length = 0f;
            for (TextNode node = nodeList.Head; node != null; node = node.Next) {
                bool newLine = false;

                if (node.Type == TextNodeType.LineBreak) {
                    newLine = true;
                } else {

                    if (SkipTrailingSpace (node, length, maxWidth) && atLeastOneNodeCosumedOnLine) {
                        newLine = true;
                    } else if (length + node.ModifiedLength <= maxWidth || !atLeastOneNodeCosumedOnLine) {

                        atLeastOneNodeCosumedOnLine = true;
                        if (!measureOnly) {
                            RenderWord (Fonts [node.Style], node.Colour, xOffset + length, yOffset, node);
                        }
                        length += node.ModifiedLength;
                        maxMeasuredWidth = Math.Max (length, maxMeasuredWidth);

                    } else {
                        newLine = true;
                        if (node.Previous != null)
                            node = node.Previous;
                    }

                }

                if (newLine) {

                    yOffset += Fonts.LineSpacing;
                    length = 0f;
                    atLeastOneNodeCosumedOnLine = false;

                    if (node.Next != null) {
                        xOffset = GetXOffset (node.Next, maxWidth, alignment);
                    }
                }

            }

            return new Vect2f (maxMeasuredWidth, yOffset + Fonts.MaxGlyphSpace);
        }

        float GetXOffset (TextNode node, float maxWidth, Alignment alignment) {
            float xOffset = 0;

            if (alignment == Alignment.Right) {
                xOffset = Box.X;
                xOffset -= (float)Math.Ceiling (TextNodeLineLength (node, maxWidth));
            } else if (alignment == Alignment.Center) {
                xOffset = (float)Math.Ceiling (0.5f * Box.X);
                xOffset -= (float)Math.Ceiling (0.5f * TextNodeLineLength (node, maxWidth));
            } else if (alignment == Alignment.Justify) {
                JustifyLine (Fonts [node.Style], node, maxWidth);
            }

            return xOffset;
        }

        /// <summary>
        /// Renders a single glyph for the given char at the given screen coordinates.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="c">C.</param>
        /// <param name="isDropShadow">Whether or not the glyph is a shadow. Will readjust position and colour.</param>
        public void RenderGlyph (QFont font, Colour color, float x, float y, char c, bool isDropShadow) {

            QFontGlyph glyph = font.FontData.CharSetMapping [c];

            //note: it's not immediately obvious, but this combined with the parameters to 
            //RenderGlyph for the shadow mean that we render the shadow centrally (despite it being a different size)
            //under the glyph
            if (isDropShadow) {
                x -= (glyph.Rect.Width * 0.5f);
                y -= (glyph.Rect.Height * 0.5f + glyph.YOffset);
            } else {
                RenderDropShadow (font, x, y, c, glyph);
            }

            TexturePage sheet = font.FontData.Pages [glyph.Page];

            float tx1 = (float)(glyph.Rect.X);// / sheet.Width;
            float ty1 = (float)(glyph.Rect.Y);// / sheet.Height;
            float tx2 = (float)(glyph.Rect.X + glyph.Rect.Width);// / sheet.Width;
            float ty2 = (float)(glyph.Rect.Y + glyph.Rect.Height);// / sheet.Height;

            Vector2 tv1 = new Vector2 (tx1, ty1);
            Vector2 tv2 = new Vector2 (tx1, ty2);
            Vector2 tv3 = new Vector2 (tx2, ty2);
            Vector2 tv4 = new Vector2 (tx2, ty1);

            Vector3 v1 = new Vector3 (x, y + glyph.YOffset, 0);
            Vector3 v2 = new Vector3 (x, y + glyph.YOffset + glyph.Rect.Height, 0);
            Vector3 v3 = new Vector3 (x + glyph.Rect.Width, y + glyph.YOffset + glyph.Rect.Height, 0);
            Vector3 v4 = new Vector3 (x + glyph.Rect.Width, y + glyph.YOffset, 0);

            Vector3 normal = new Vector3 (0, 0, -1);
            int argb = glyph.SuppressColouring ? Colour.White.ToBgra32 () : color.ToBgra32 ();

            VertexBuffer vbo = _sortedBuffers [sheet.GLTexID];

            vbo.AddVertex (v1, normal, tv1, argb);
            vbo.AddVertex (v2, normal, tv2, argb);
            vbo.AddVertex (v3, normal, tv3, argb);

            vbo.AddVertex (v1, normal, tv1, argb);
            vbo.AddVertex (v3, normal, tv3, argb);
            vbo.AddVertex (v4, normal, tv4, argb);
        }

        void RenderDropShadow (QFont font, float x, float y, char c, QFontGlyph nonShadowGlyph) {
            //note can cast drop shadow offset to int, but then you can't move the shadow smoothly...
            if (font.FontData.DropShadow != null && font.Options.DropShadowActive) {
                //make sure fontdata font's options are synced with the actual options
                if (font.FontData.DropShadow.Options != font.Options) {
                    font.FontData.DropShadow.Options = font.Options;
                }

                RenderGlyph (font.FontData.DropShadow, Colour.Black,
                    x + (font.FontData.MeanGlyphWidth * font.Options.DropShadowOffset.X + nonShadowGlyph.Rect.Width * 0.5f),
                    y + (font.FontData.MeanGlyphWidth * font.Options.DropShadowOffset.Y + nonShadowGlyph.Rect.Height * 0.5f + nonShadowGlyph.YOffset), c, true);
            }
        }

        /// <summary>
        /// Computes the length of the next line, and whether the line is valid for
        /// justification.
        /// </summary>
        void JustifyLine (QFont font, TextNode node, float targetLength) {

            bool justifiable = false;

            if (node == null)
                return;

            TextNode headNode = node; //keep track of the head node


            //start by finding the length of the block of text that we know will actually fit:

            int charGaps = 0;
            int spaceGaps = 0;

            bool atLeastOneNodeCosumedOnLine = false;
            float length = 0;
            var expandEndNode = node; //the node at the end of the smaller list (before adding additional word)
            for (; node != null; node = node.Next) {

                if (node.Type == TextNodeType.LineBreak)
                    break;

                if (SkipTrailingSpace (node, length, targetLength) && atLeastOneNodeCosumedOnLine) {
                    justifiable = true;
                    break;
                }

                if (length + node.Length < targetLength || !atLeastOneNodeCosumedOnLine) {

                    expandEndNode = node;

                    if (node.Type == TextNodeType.Space)
                        spaceGaps++;

                    if (node.Type == TextNodeType.Word) {
                        charGaps += (node.Text.Length - 1);

                        //word was part of a crumbled word, so there's an extra char cap between the two words
                        if (CrumbledWord (node))
                            charGaps++;

                    }

                    atLeastOneNodeCosumedOnLine = true;
                    length += node.Length;
                } else {
                    justifiable = true;
                    break;
                }

            }

            //now we check how much additional length is added by adding an additional word to the line
            float extraLength = 0f;
            int extraSpaceGaps = 0;
            int extraCharGaps = 0;
            bool contractPossible = false;
            TextNode contractEndNode = null;
            for (node = expandEndNode.Next; node != null; node = node.Next) {


                if (node.Type == TextNodeType.LineBreak)
                    break;

                if (node.Type == TextNodeType.Space) {
                    extraLength += node.Length;
                    extraSpaceGaps++;
                } else if (node.Type == TextNodeType.Word) {
                    contractEndNode = node;
                    contractPossible = true;
                    extraLength += node.Length;
                    extraCharGaps += (node.Text.Length - 1);
                    break;
                }
            }

            if (justifiable) {

                //last part of this condition is to ensure that the full contraction is possible (it is all or nothing with contractions, since it looks really bad if we don't manage the full)
                bool contract = contractPossible && (extraLength + length - targetLength) * font.Options.JustifyContractionPenalty < (targetLength - length) &&
                                ((targetLength - (length + extraLength + 1)) / targetLength > -font.Options.JustifyCapContract); 

                if ((!contract && length < targetLength) || (contract && length + extraLength > targetLength)) {  //calculate padding pixels per word and char

                    if (contract) {
                        length += extraLength + 1; 
                        charGaps += extraCharGaps;
                        spaceGaps += extraSpaceGaps;
                    }



                    int totalPixels = (int)(targetLength - length); //the total number of pixels that need to be added to line to justify it
                    int spacePixels = 0; //number of pixels to spread out amongst spaces
                    int charPixels = 0; //number of pixels to spread out amongst char gaps


                    if (contract) {

                        if (totalPixels / targetLength < -font.Options.JustifyCapContract)
                            totalPixels = (int)(-font.Options.JustifyCapContract * targetLength);
                    } else {
                        if (totalPixels / targetLength > font.Options.JustifyCapExpand)
                            totalPixels = (int)(font.Options.JustifyCapExpand * targetLength);
                    }


                    //work out how to spread pixles between character gaps and word spaces
                    if (charGaps == 0) {
                        spacePixels = totalPixels;
                    } else if (spaceGaps == 0) {
                        charPixels = totalPixels;
                    } else {

                        if (contract)
                            charPixels = (int)(totalPixels * font.Options.JustifyCharacterWeightForContract * charGaps / spaceGaps);
                        else
                            charPixels = (int)(totalPixels * font.Options.JustifyCharacterWeightForExpand * charGaps / spaceGaps);


                        if ((!contract && charPixels > totalPixels) ||
                            (contract && charPixels < totalPixels))
                            charPixels = totalPixels;

                        spacePixels = totalPixels - charPixels;
                    }


                    int pixelsPerChar = 0;  //minimum number of pixels to add per char
                    int leftOverCharPixels = 0; //number of pixels remaining to only add for some chars

                    if (charGaps != 0) {
                        pixelsPerChar = charPixels / charGaps;
                        leftOverCharPixels = charPixels - pixelsPerChar * charGaps;
                    }


                    int pixelsPerSpace = 0; //minimum number of pixels to add per space
                    int leftOverSpacePixels = 0; //number of pixels remaining to only add for some spaces

                    if (spaceGaps != 0) {
                        pixelsPerSpace = spacePixels / spaceGaps;
                        leftOverSpacePixels = spacePixels - pixelsPerSpace * spaceGaps;
                    }

                    //now actually iterate over all nodes and set tweaked length
                    for (node = headNode; node != null; node = node.Next) {

                        if (node.Type == TextNodeType.Space) {
                            node.LengthTweak = pixelsPerSpace;
                            if (leftOverSpacePixels > 0) {
                                node.LengthTweak += 1;
                                leftOverSpacePixels--;
                            } else if (leftOverSpacePixels < 0) {
                                node.LengthTweak -= 1;
                                leftOverSpacePixels++;
                            }


                        } else if (node.Type == TextNodeType.Word) {
                            int cGaps = (node.Text.Length - 1);
                            if (CrumbledWord (node))
                                cGaps++;

                            node.LengthTweak = cGaps * pixelsPerChar;


                            if (leftOverCharPixels >= cGaps) {
                                node.LengthTweak += cGaps;
                                leftOverCharPixels -= cGaps;
                            } else if (leftOverCharPixels <= -cGaps) {
                                node.LengthTweak -= cGaps;
                                leftOverCharPixels += cGaps;
                            } else {
                                node.LengthTweak += leftOverCharPixels;
                                leftOverCharPixels = 0;
                            }
                        }

                        if ((!contract && node == expandEndNode) || (contract && node == contractEndNode))
                            break;

                    }

                }

            }

        }

        /// <summary>
        /// Checks whether to skip trailing space on line because the next word does not
        /// fit.
        /// 
        /// We only check one space - the assumption is that if there is more than one,
        /// it is a deliberate attempt to insert spaces.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="lengthSoFar"></param>
        /// <param name="boundWidth"></param>
        /// <returns></returns>
        bool SkipTrailingSpace (TextNode node, float lengthSoFar, float boundWidth) {

            if (node.Type == TextNodeType.Space && node.Next != null && node.Next.Type == TextNodeType.Word && node.ModifiedLength + node.Next.ModifiedLength + lengthSoFar > boundWidth) {
                return true;
            }

            return false;

        }

        void RenderWord (QFont font, Colour colour, float x, float y, TextNode node) {

            if (node.Type != TextNodeType.Word && node.Type != TextNodeType.Space) {
                Console.Out.WriteLine ("Not rendering: " + node.Type);
                return;
            }

            string text = node.Type == TextNodeType.Word ? node.Text : " ";
            int charGaps = text.Length - 1;
            bool isCrumbleWord = CrumbledWord (node);
            if (isCrumbleWord)
                charGaps++;

            int pixelsPerGap = 0;
            int leftOverPixels = 0;

            if (charGaps != 0) {
                pixelsPerGap = (int)node.LengthTweak / charGaps;
                leftOverPixels = (int)node.LengthTweak - pixelsPerGap * charGaps;
            }

            for (int i = 0; i < text.Length; i++) {
                char c = text [i];
                if (font.FontData.CharSetMapping.ContainsKey (c)) {

                    QFontGlyph glyph = font.FontData.CharSetMapping [c];
                    RenderGlyph (font, colour, x, y, c, false);

                    if (font.IsMonospacingActive) {
                        x += font.MonoSpaceWidth;
                    } else {
                        x += (int)Math.Ceiling (glyph.Rect.Width + font.FontData.MeanGlyphWidth * font.Options.CharacterSpacing + Fonts.GetKerningPairCorrection (node.Style, i, text, node));
                    }

                    x += pixelsPerGap;
                    if (leftOverPixels > 0) {
                        x += 1.0f;
                        leftOverPixels--;
                    } else if (leftOverPixels < 0) {
                        x -= 1.0f;
                        leftOverPixels++;
                    }


                }
            }
        }

        /// <summary>
        /// Computes the length of the next line, and whether the line is valid for
        /// justification.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="maxLength"></param>
        /// <param name="justifable"></param>
        /// <returns></returns>
        float TextNodeLineLength (TextNode node, float maxLength) {

            if (node == null)
                return 0;

            bool atLeastOneNodeCosumedOnLine = false;
            float length = 0;
            for (; node != null; node = node.Next) {

                if (node.Type == TextNodeType.LineBreak)
                    break;

                if (SkipTrailingSpace (node, length, maxLength) && atLeastOneNodeCosumedOnLine)
                    break;

                if (length + node.Length <= maxLength || !atLeastOneNodeCosumedOnLine) {
                    atLeastOneNodeCosumedOnLine = true;
                    length += node.Length;
                } else {
                    break;
                }


            }
            return length;
        }

        bool CrumbledWord (TextNode node) {
            return (node.Type == TextNodeType.Word && node.Next != null && node.Next.Type == TextNodeType.Word);  
        }

        #endregion

        #region Text formatting

        static StringBuilder _builder = new StringBuilder ();

        static string ConcatString (params string[] text) {
            _builder.Clear ();
            for (int i = 0; i < text.Length; i++) {
                if (i > 0) {
                    _builder.AppendLine ();
                }
                _builder.Append (text [i]);
            }
            return _builder.ToString ();
        }

        static string ConcatString (string text, params string[] lines) {
            _builder.Clear ();
            _builder.Append (text);
            for (int i = 0; i < lines.Length; i++) {
                _builder.AppendLine ();
                _builder.Append (lines [i]);
            }
            return _builder.ToString ();
        }

        #endregion

        #region Pool

        static Dictionary<string, TextBuffer> _pool = new Dictionary<string, TextBuffer> ();

        public static TextBuffer GetBuffer (params string[] text) {
            return GetBuffer (ConcatString (text));
        }

        public static TextBuffer GetBuffer (string text, params string[] texts) {
            return GetBuffer (ConcatString (text, texts));
        }

        public static TextBuffer GetBuffer (string text) {
            // Needs to be cleared out now and then.
            if (_pool.Count > 512) {
                GameAccess.Interface.GameConsole.Rendering ("Clearing text buffer pool.");
                _pool.Clear ();
            }

            if (!_pool.ContainsKey (text)) {
                _pool [text] = new TextBuffer (text);
            }
            return _pool [text];
        }

        #endregion
    }
}
