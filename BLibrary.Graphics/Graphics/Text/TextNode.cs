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

using System.Drawing;
using BLibrary.Util;

namespace BLibrary.Graphics.Text {

    sealed class TextNode {
        public TextNodeType Type;
        public string Text;
        ///<summary>
        ///pixel length (without tweaks)
        ///</summary>
        public float Length;
        ///<summary>
        ///length tweak for justification
        ///</summary>
        public float LengthTweak;
        //
        public float ModifiedLength {
            get { return Length + LengthTweak; }
        }

        public FontStyle Style {
            get;
            set;
        }

        public Colour Colour {
            get;
            set;
        }

        public TextNode (TextNodeType type, string text, TextFormat format) {
            Type = type;
            Text = text;
            Style = format.Style;
            Colour = format.Colour;
        }

        public TextNode Next;
        public TextNode Previous;

        public override string ToString () {
            return string.Format ("[TextNode: Type={0}, Text={1}, ModifiedLength={2}, Colour={3}]", Type, Text, ModifiedLength, Colour);
        }
    }
}
