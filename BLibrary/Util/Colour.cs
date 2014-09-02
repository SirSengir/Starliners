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
using System.Runtime.Serialization;

namespace BLibrary.Util {

    [Serializable]
    public struct Colour : IEquatable<Colour>, ISerializable {
        public static readonly Colour Transparent = new Colour (166, 166, 166, 0);
        public static readonly Colour White = new Colour (255, 255, 255);
        public static readonly Colour Black = new Colour (0, 0, 0);
        public static readonly Colour Red = new Colour (255, 0, 0);
        public static readonly Colour RedDarker = new Colour (227, 18, 18);
        public static readonly Colour GreenDarker = new Colour (30, 200, 30);
        public static readonly Colour Blue = new Colour (0, 0, 255);
        public static readonly Colour BlueLighter = new Colour (51, 153, 255);
        public static readonly Colour TooltipBackground = new Colour (49, 44, 48);
        public static readonly Colour TooltipHeader = new Colour (14, 14, 14);
        public static readonly Colour DarkGreyBrown = new Colour (49, 44, 48, 249);
        public static readonly Colour Grey = new Colour (160, 160, 160);
        // A
        public static readonly Colour Amber = new Colour (0xffbf00);
        // B
        public static readonly Colour Beige = new Colour (0xF5F5DC);
        public static readonly Colour BlueAngel = new Colour (0xB7CEEC);
        public static readonly Colour BlueGray = new Colour (0x98AFC7);
        // C
        public static readonly Colour Chartreuse = new Colour (0x7fff00);
        public static readonly Colour CornflowerBlue = new Colour (0x6495ed);
        public static readonly Colour Crimson = new Colour (0xdc143c);
        // D
        public static readonly Colour DarkBlue = new Colour (0x00008b);
        public static readonly Colour DarkCyan = new Colour (0x008b8b);
        public static readonly Colour DarkGoldenRod = new Colour (0xB8860B);
        public static readonly Colour DarkGoldenRod1 = new Colour (0xffb90f);
        public static readonly Colour DarkGoldenRod2 = new Colour (0xeead0e);
        public static readonly Colour DarkGoldenRod3 = new Colour (0xcd950c);
        public static readonly Colour DarkGoldenRod4 = new Colour (0x8b6508);
        public static readonly Colour DarkGray = new Colour (0xA9A9A9);
        public static readonly Colour DarkGreen = new Colour (0x006400);
        public static readonly Colour DarkKhaki = new Colour (0xBDB76B);
        public static readonly Colour DarkMagenta = new Colour (0x8B008B);
        public static readonly Colour DarkOliveGreen = new Colour (0x556B2F);
        public static readonly Colour DarkOrange = new Colour (0xFF8C00);
        public static readonly Colour DarkOrchid = new Colour (0x9932CC);
        public static readonly Colour DarkRed = new Colour (0x8B0000);
        public static readonly Colour DarkSalmon = new Colour (0xE9967A);
        public static readonly Colour DarkSeaGreen = new Colour (0x8FBC8F);
        public static readonly Colour DarkSlateBlue = new Colour (0x483D8B);
        public static readonly Colour DarkSlateGray = new Colour (0x2F4F4F);
        public static readonly Colour DarkTurquoise = new Colour (0x00CED1);
        public static readonly Colour DarkViolet = new Colour (0x9400D3);
        public static readonly Colour DeepPink = new Colour (0xFF1493);
        public static readonly Colour DeepSkyBlue = new Colour (0x00BFFF);
        public static readonly Colour DimGray = new Colour (0x696969);
        public static readonly Colour DodgerBlue = new Colour (0x1e90ff);
        public static readonly Colour DodgerBlue1 = new Colour (0x1c86ee);
        public static readonly Colour DodgerBlue2 = new Colour (0x1874cd);
        public static readonly Colour DodgerBlue3 = new Colour (0x104e8b);
        // F
        public static readonly Colour FireBrick = new Colour (0xB22222);
        public static readonly Colour FloralWhite = new Colour (0xFFFAF0);
        public static readonly Colour ForestGreen = new Colour (0x228B22);
        public static readonly Colour Fuchsia = new Colour (0xFF00FF);
        // G
        public static readonly Colour Gainsboro = new Colour (0xDCDCDC);
        public static readonly Colour GhostWhite = new Colour (0xF8F8FF);
        public static readonly Colour Gold = new Colour (0xFFD700);
        public static readonly Colour GoldenRod = new Colour (0xDAA520);
        public static readonly Colour Gray = new Colour (0x808080);
        public static readonly Colour Green = new Colour (0x008000);
        public static readonly Colour GreenYellow = new Colour (0xADFF2F);
        // H, I, J, K
        // L
        public static readonly Colour Lavender = new Colour (0xE6E6FA);
        public static readonly Colour LavenderBlush = new Colour (0xFFF0F5);
        public static readonly Colour LawnGreen = new Colour (0x7CFC00);
        public static readonly Colour LemonChiffon = new Colour (0xFFFACD);
        public static readonly Colour LightBlue = new Colour (0xADD8E6);
        public static readonly Colour LightCoral = new Colour (0xF08080);
        public static readonly Colour LightCyan = new Colour (0xE0FFFF);
        public static readonly Colour LightGoldenRodYellow = new Colour (0xFAFAD2);
        public static readonly Colour LightGray = new Colour (0xD3D3D3);
        public static readonly Colour LightGreen = new Colour (0x90EE90);
        public static readonly Colour LightPink = new Colour (0xffb6c1);
        public static readonly Colour LightSalmon = new Colour (0xFFA07A);
        public static readonly Colour LightSeaGreen = new Colour (0x20B2AA);
        public static readonly Colour LightSkyBlue = new Colour (0x87CEFA);
        public static readonly Colour LightSlateGray = new Colour (0x778899);
        public static readonly Colour LightSteelBlue = new Colour (0xb0c4de);
        public static readonly Colour LightSteelBlue4 = new Colour (0x646d7e);
        public static readonly Colour LightYellow = new Colour (0xFFFFE0);
        public static readonly Colour Lime = new Colour (0x00FF00);
        public static readonly Colour LimeGreen = new Colour (0x32cd32);
        public static readonly Colour Linen = new Colour (0xFAF0E6);
        // M, N
        public static readonly Colour Magenta = new Colour (0xFF00FF);
        public static readonly Colour MediumBlue = new Colour (0x0000CD);
        public static readonly Colour MediumSeaGreen = new Colour (0x3CB371);
        public static readonly Colour MediumVioletRed = new Colour (0xCA226B);
        public static readonly Colour MidnightBlue = new Colour (0x191970);
        public static readonly Colour Moccasin = new Colour (0xFFE4B5);
        // O
        public static readonly Colour OceanBlue = new Colour (0x2b65ec);
        public static readonly Colour OldLace = new Colour (0xFDF5E6);
        public static readonly Colour Olive = new Colour (0x808000);
        public static readonly Colour OliveDrab = new Colour (0x6B8E23);
        public static readonly Colour Orange = new Colour (0xFFA500);
        public static readonly Colour OrangeRed = new Colour (0xFF4500);
        public static readonly Colour Orchid = new Colour (0xDA70D6);
        // P, Q
        // R
        public static readonly Colour Purple = new Colour (0x800080);
        public static readonly Colour RoyalBlue = new Colour (0x4169e1);
        public static readonly Colour RoyalBlue1 = new Colour (0x4876ff);
        public static readonly Colour RoyalBlue2 = new Colour (0x436eee);
        public static readonly Colour RoyalBlue3 = new Colour (0x3a5fcd);
        public static readonly Colour RoyalBlue4 = new Colour (0x27408b);
        public static readonly Colour RoyalBlue5 = new Colour (0x2954cc);
        // S
        public static readonly Colour SeaGreen = new Colour (0x2E8B57);
        public static readonly Colour SlateBlue = new Colour (0x6a5acd);
        public static readonly Colour SlateBlue1 = new Colour (0x836fff);
        public static readonly Colour SlateBlue2 = new Colour (0x7a67ee);
        public static readonly Colour SlateBlue3 = new Colour (0x6959cd);
        public static readonly Colour SlateBlue4 = new Colour (0x473c8b);
        public static readonly Colour SpringGreen = new Colour (0x00ff7f);
        public static readonly Colour SpringGreen3 = new Colour (0x00cd66);
        public static readonly Colour SteelBlue = new Colour (0x4863a0);
        public static readonly Colour SteelBlue1 = new Colour (0x5cb3ff);
        public static readonly Colour SteelBlue2 = new Colour (0x56a5ec);
        public static readonly Colour SteelBlue3 = new Colour (0x488ac7);
        public static readonly Colour SunflowerYellow = new Colour (0xffd700);
        // T
        public static readonly Colour Tan = new Colour (0xd2b48c);
        public static readonly Colour Tan1 = new Colour (0xffa54f);
        public static readonly Colour Tan2 = new Colour (0xee9a49);
        public static readonly Colour Tan3 = new Colour (0xcd853f);
        public static readonly Colour Tan4 = new Colour (0x8b5a2b);
        public static readonly Colour Turquoise = new Colour (0x43c6db);
        // Y
        public static readonly Colour Yellow = new Colour (0xFFFF00);
        public static readonly Colour YellowGreen = new Colour (0x9acd32);
        public static readonly Colour[] DEFINED = new Colour[] {
            new Colour (255, 51, 51), new Colour (255, 153, 51), new Colour (255, 255, 51), new Colour (153, 255, 51), new Colour (51, 255, 51), new Colour (51, 255, 153),
            new Colour (51, 255, 255), new Colour (51, 153, 255), new Colour (51, 51, 255), new Colour (153, 51, 255), new Colour (255, 51, 255), new Colour (255, 51, 153),
            new Colour (160, 160, 160), new Colour (0, 0, 0), new Colour (255, 255, 255)
        };

        #region Properties

        public byte R { get; private set; }

        public byte G { get; private set; }

        public byte B { get; private set; }

        public byte A { get; private set; }

        #endregion

        #region Constructor

        public Colour (int rgb)
            : this () {
            R = (byte)((rgb >> 16) & 0x0ff);
            G = (byte)((rgb >> 8) & 0x0ff);
            B = (byte)((rgb) & 0x0ff);
            A = 255;
        }

        public Colour (Colour copy, byte a)
            : this () {
            R = copy.R;
            G = copy.G;
            B = copy.B;
            A = a;
        }

        public Colour (byte r, byte g, byte b)
            : this (r, g, b, 255) {
        }

        public Colour (byte r, byte g, byte b, byte a)
            : this () {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        #endregion

        #region Serialization

        public Colour (SerializationInfo info, StreamingContext context)
            : this () {
            FromBgra32 (info.GetInt32 ("BGRA32"));
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("BGRA32", ToBgra32 ());
        }

        #endregion

        void FromBgra32 (int bgra32) {
            A = (byte)((bgra32 >> 24) & 0x0ff);
            B = (byte)((bgra32 >> 16) & 0x0ff);
            G = (byte)((bgra32 >> 8) & 0x0ff);
            R = (byte)((bgra32) & 0x0ff);
        }

        public int ToInteger () {
            return ((R & 0x0ff) << 16) | ((G & 0x0ff) << 8) | (B & 0x0ff);
        }

        public int ToBgra32 () {
            return ((A << 24) | (B << 16) | (G << 8) | R);
        }

        public int ToBgra32WithAlpha (float alpha) {
            return (((byte)(A * alpha) << 24) | (B << 16) | (G << 8) | R);
        }

        public override string ToString () {
            return string.Format ("[Colour: R={0}, G={1}, B={2}, A={3}]", R, G, B, A);
        }

        public string ToString (string format) {
            if ("#§".Equals (format)) {
                return string.Format ("§#{0}§", ToInteger ().ToString ("X"));
            } else if ("#".Equals (format)) {
                return string.Format ("#{0}", ToInteger ().ToString ("X"));
            }
            return ToInteger ().ToString (format);
        }

        public override int GetHashCode () {
            return R.GetHashCode () ^ G.GetHashCode () ^ B.GetHashCode () ^ A.GetHashCode ();
        }

        public override bool Equals (object obj) {
            if (!(obj is Colour))
                return false;

            return this.Equals ((Colour)obj);
        }

        public bool Equals (Colour other) {
            return R == other.R
            && G == other.G
            && B == other.B
            && A == other.A;
        }

        public static Colour GetRandom (Random rand) {
            return new Colour ((byte)rand.Next (256), (byte)rand.Next (256), (byte)rand.Next (256), 255);
        }

        public static bool operator == (Colour lhs, Colour rhs) {
            return lhs.Equals (rhs);
        }

        public static bool operator != (Colour lhs, Colour rhs) {
            return !(lhs == rhs);
        }

        public static Colour operator - (Colour lhs, Colour rhs) {
            //return new Colour(lhs.ToInteger() - rhs.ToInteger());
            int rShift = (255 - lhs.R) * (255 - lhs.R);
            rShift += (255 - rhs.R) * (255 - rhs.R);
            int gShift = (255 - lhs.G) * (255 - lhs.G);
            gShift += (255 - rhs.G) * (255 - rhs.G);
            int bShift = (255 - lhs.B) * (255 - lhs.B);
            bShift += (255 - rhs.B) * (255 - rhs.B);

            return new Colour (
                (byte)(255 - Math.Sqrt (rShift)),
                (byte)(255 - Math.Sqrt (gShift)),
                (byte)(255 - Math.Sqrt (bShift))
            );

        }
    }
}
