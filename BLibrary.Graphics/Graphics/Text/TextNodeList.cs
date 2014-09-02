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
using System.Text;
using System.Collections;
using QuickFont;
using System.Drawing;
using BLibrary.Util;

namespace BLibrary.Graphics.Text {

    /// <summary>
    /// A doubly linked list of text nodes
    /// </summary>
    sealed class TextNodeList : IEnumerable<TextNode> {
        #region Constants

        const char TOKEN_RESET = '%';
        const char TOKEN_COLOUR = '#';
        const char TOKEN_STYLE = '$';
        const char TOKEN_SEPERATOR = '?';
        const char TOKEN_DISABLE = '-';
        const char TOKEN_ENABLE = '+';

        #endregion

        #region Properties

        public TextNode Head {
            get;
            set;
        }

        public TextNode Tail {
            get;
            set;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Builds a doubly linked list of text nodes from the given input string
        /// </summary>
        /// <param name="text"></param>
        public TextNodeList (string text, TextFormat format) {
            Parse (text, format);
        }

        #endregion

        void Parse (string text, TextFormat format) {
            text = text.Replace ("\r\n", "\r");
            text = text.Replace ('^', '\n');

            TextFormat currentFormat = format;
            bool wordInProgress = false;
            bool formatInProgress = false;

            char previousChar = ' ';
            StringBuilder currentWord = new StringBuilder ();

            for (int i = 0; i < text.Length; i++) {
                if (text [i] == '\r' || text [i] == '\n' || text [i] == ' ') {
                    if (wordInProgress) {
                        Add (new TextNode (TextNodeType.Word, currentWord.ToString (), currentFormat));
                        wordInProgress = false;
                    }

                    if (text [i] == '\r' || text [i] == '\n') {
                        Add (new TextNode (TextNodeType.LineBreak, null, currentFormat));
                        currentFormat = format;
                    } else if (text [i] == ' ') {
                        Add (new TextNode (TextNodeType.Space, null, currentFormat));
                    }

                } else if (text [i] == '§' && previousChar != '\\') {

                    if (formatInProgress) {
                        formatInProgress = false;
                        currentFormat = ParseFormatting (currentWord.ToString (), currentFormat, format);
                    } else {
                        formatInProgress = true;

                        if (wordInProgress) {
                            Add (new TextNode (TextNodeType.Word, currentWord.ToString (), currentFormat));
                            wordInProgress = false;
                        }
                        currentWord.Clear ();
                    }

                } else {
                    if (!formatInProgress && !wordInProgress) {
                        wordInProgress = true;
                        currentWord.Clear ();
                    }

                    currentWord.Append (text [i]);
                }

                previousChar = text [i];
            }

            if (wordInProgress) {
                Add (new TextNode (TextNodeType.Word, currentWord.ToString (), currentFormat));
            }
        }

        TextFormat ParseFormatting (string format, TextFormat current, TextFormat defaultFormat) {

            string[] tokens = format.Split (TOKEN_SEPERATOR);
            for (int i = 0; i < tokens.Length; i++) {
                // Reset if we have the reset code.
                if (tokens [i] [0] == TOKEN_RESET) {
                    current = defaultFormat;

                } else if (tokens [i] [0] == TOKEN_COLOUR) {
                    current.Colour = new Colour (Int32.Parse (tokens [i].TrimStart (TOKEN_COLOUR), System.Globalization.NumberStyles.HexNumber));

                } else if (tokens [i] [0] == TOKEN_ENABLE || tokens [i] [0] == TOKEN_DISABLE) {
                    for (int j = 0; j < tokens [i].Length - 1; j += 2) {
                        if (tokens [i] [j].Equals (TOKEN_ENABLE)) {
                            current.Style |= ParseStyle (tokens [i] [j + 1]);
                        } else if (tokens [i] [j].Equals (TOKEN_DISABLE)) {
                            current.Style &= ~ParseStyle (tokens [i] [j + 1]);
                        }
                    }
                }
            }

            return current;
        }

        FontStyle ParseStyle (char style) {
            switch (style) {
                case 'b':
                    return FontStyle.Bold;
                case 'i':
                    return FontStyle.Italic;
                case 'u':
                    return FontStyle.Underline;
                case 's':
                    return FontStyle.Strikeout;
                default:
                case 'r':
                    return FontStyle.Regular;
            }
        }

        public void MeasureNodes (FontCollection fonts) {
            
            foreach (TextNode node in this) {
                if (node.Length == 0f)
                    node.Length = MeasureTextNodeLength (node, fonts);
            }
        }

        float MeasureTextNodeLength (TextNode node, FontCollection fonts) {

            QFont font = fonts [node.Style];
            QFontRenderOptions options = font.Options;

            bool monospaced = font.FontData.IsMonospacingActive (options);
            float monospaceWidth = font.FontData.GetMonoSpaceWidth (options);

            if (node.Type == TextNodeType.Space) {
                if (monospaced) {
                    return monospaceWidth;
                }

                return (float)Math.Ceiling (font.FontData.MeanGlyphWidth * options.WordSpacing);
            }


            float length = 0f;
            if (node.Type == TextNodeType.Word) {

                for (int i = 0; i < node.Text.Length; i++) {
                    char c = node.Text [i];
                    if (font.FontData.CharSetMapping.ContainsKey (c)) {
                        if (monospaced)
                            length += monospaceWidth;
                        else
                            length += (float)Math.Ceiling (font.FontData.CharSetMapping [c].Rect.Width + font.FontData.MeanGlyphWidth * options.CharacterSpacing + fonts.GetKerningPairCorrection (node.Style, i, node.Text, node));
                    }
                }
            }
            return length;
        }

        /// <summary>
        /// Splits a word into sub-words of size less than or equal to baseCaseSize 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="baseCaseSize"></param>
        public void Crumble (TextNode node, int baseCaseSize) {

            //base case
            if (node.Text.Length <= baseCaseSize)
                return;
 
            var left = SplitNode (node);
            var right = left.Next;

            Crumble (left, baseCaseSize);
            Crumble (right, baseCaseSize);

        }

        /// <summary>
        /// Splits a word node in two, adding both new nodes to the list in sequence.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>The first new node</returns>
        TextNode SplitNode (TextNode node) {
            if (node.Type != TextNodeType.Word) {
                throw new Exception ("Cannot split text node of type: " + node.Type);
            }

            int midPoint = node.Text.Length / 2;

            string newFirstHalf = node.Text.Substring (0, midPoint);
            string newSecondHalf = node.Text.Substring (midPoint, node.Text.Length - midPoint);


            TextNode newFirst = new TextNode (TextNodeType.Word, newFirstHalf, new TextFormat (node.Colour));
            TextNode newSecond = new TextNode (TextNodeType.Word, newSecondHalf, new TextFormat (node.Colour));
            newFirst.Next = newSecond;
            newSecond.Previous = newFirst;

            //node is head
            if (node.Previous == null)
                Head = newFirst;
            else {
                node.Previous.Next = newFirst;
                newFirst.Previous = node.Previous;
            }

            //node is tail
            if (node.Next == null)
                Tail = newSecond;
            else {
                node.Next.Previous = newSecond;
                newSecond.Next = node.Next;
            }

            return newFirst;
        }

        void Add (TextNode node) {

            //new node is head (and tail)
            if (Head == null) {
                Head = node;
                Tail = node;
            } else {
                Tail.Next = node;
                node.Previous = Tail;
                Tail = node;
            }

        }

        public override string ToString () {
            StringBuilder builder = new StringBuilder ();


            // for (var node = Head; node.Next != null; node = node.Next)

            foreach (TextNode node in this) {
                if (node.Type == TextNodeType.Space)
                    builder.Append (" ");
                if (node.Type == TextNodeType.LineBreak)
                    builder.Append (System.Environment.NewLine);
                if (node.Type == TextNodeType.Word)
                    builder.Append ("" + node.Text + "");

            }

            return builder.ToString ();
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator () {
            return GetEnumerator ();
        }

        public IEnumerator<TextNode> GetEnumerator () {
            return new TextNodeListEnumerator (this);
        }

        sealed class TextNodeListEnumerator : IEnumerator<TextNode> {
            TextNode _current = null;
            TextNodeList _target;

            public TextNodeListEnumerator (TextNodeList targetList) {
                _target = targetList;
            }

            object IEnumerator.Current {
                get { return Current; }
            }

            public TextNode Current {
                get { return _current; }
            }

            public bool MoveNext () {
                if (_current == null)
                    _current = _target.Head;
                else
                    _current = _current.Next;
                return _current != null;
            }

            public void Reset () {
                _current = null;
            }

            public void Dispose () {

            }
        }

        #endregion
    }
}
