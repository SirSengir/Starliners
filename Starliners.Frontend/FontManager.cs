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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using BLibrary;
using QuickFont;
using BLibrary.Graphics.Text;
using BLibrary.Util;
using OpenTK.Graphics;
using Starliners.Util;

namespace Starliners {

    public sealed class FontManager {
        #region Constants

        public const string BASIC = "basic";
        public const string HEADER = "header";
        public const string STACK_NUM = "stacknum";
        public const string PARTICLE = "particle";

        const string FONT_10_5 = "Fonts.Regular.Sansation_10_5.qfont";
        const string FONT_10_5_BOLD = "Fonts.Bold.Sansation_10_5.qfont";

        const string FONT_13_5 = "Fonts.Regular.Sansation_13_5.qfont";
        const string FONT_13_5_UNDERLINE = "Fonts.Underline.Sansation_13_5.qfont";
        const string FONT_13_5_BOLD = "Fonts.Bold.Sansation_13_5.qfont";
        const string FONT_13_5_BOLD_ITALIC = "Fonts.Bold_Italic.Sansation_13_5.qfont";
        const string FONT_13_5_BOLD_UNDERLINE = "Fonts.Bold_Underline.Sansation_13_5.qfont";
        const string FONT_13_5_BOLD_ITALIC_UNDERLINE = "Fonts.Bold_Italic_Underline.Sansation_13_5.qfont";
        const string FONT_13_5_ITALIC = "Fonts.Italic.Sansation_13_5.qfont";
        const string FONT_13_5_ITALIC_UNDERLINE = "Fonts.Italic_Underline.Sansation_13_5.qfont";
        const string FONT_16_5 = "Fonts.Regular.Sansation_16_5.qfont";
        const string FONT_16_5_BOLD = "Fonts.Bold.Sansation_16_5.qfont";

        #endregion

        static FontManager _instance;

        public static FontManager Instance {
            get {
                if (_instance == null) {
                    _instance = new FontManager ();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Returns a font for the text or the shadow for the given key.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="shadow"></param>
        /// <returns></returns>
        public FontDefinition this [string index] {
            get {
                if (!_styles.ContainsKey (index)) {
                    return null;
                }
                return _styles [index];
            }
        }

        public FontCollection Regular {
            get;
            private set;
        }

        Dictionary<string, FontDefinition> _styles = new Dictionary<string, FontDefinition> ();
        Dictionary<string, FontCollection> _collections = new Dictionary<string, FontCollection> ();

        public FontManager () {

            _collections [BASIC] = new FontCollection (CreateFont (FONT_13_5, Colour.White, false));
            _collections [BASIC].AddFont (FontStyle.Underline, CreateFont (FONT_13_5_UNDERLINE, Colour.White, true));
            _collections [BASIC].AddFont (FontStyle.Bold, CreateFont (FONT_13_5_BOLD, Colour.White, false));
            _collections [BASIC].AddFont (FontStyle.Bold | FontStyle.Italic, CreateFont (FONT_13_5_BOLD_ITALIC, Colour.White, false));
            _collections [BASIC].AddFont (FontStyle.Bold | FontStyle.Underline, CreateFont (FONT_13_5_BOLD_UNDERLINE, Colour.White, true));
            _collections [BASIC].AddFont (FontStyle.Bold | FontStyle.Italic | FontStyle.Underline, CreateFont (FONT_13_5_BOLD_ITALIC_UNDERLINE, Colour.White, true));
            _collections [BASIC].AddFont (FontStyle.Italic, CreateFont (FONT_13_5_ITALIC, Colour.White, false));
            _collections [BASIC].AddFont (FontStyle.Italic | FontStyle.Underline, CreateFont (FONT_13_5_ITALIC_UNDERLINE, Colour.White, true));

            Regular = _collections [BASIC];
            _styles [BASIC] = new FontDefinition (_collections [BASIC], new TextFormat (Colour.White));

            _collections [HEADER] = new FontCollection (CreateFont (FONT_13_5_BOLD, Colour.White, false));
            _styles [HEADER] = new FontDefinition (_collections [HEADER], new TextFormat (Colour.White));

            _collections [STACK_NUM] = new FontCollection (CreateFont (FONT_10_5_BOLD, Colour.White, new QFontShadowConfiguration () { blurPasses = 0 }, false));
            _styles [STACK_NUM] = new FontDefinition (_collections [STACK_NUM], new TextFormat (Colour.White));

            _collections [PARTICLE] = new FontCollection (CreateFont (FONT_10_5_BOLD, Colour.White, false));
            _collections [PARTICLE].AddFont (FontStyle.Bold, CreateFont (FONT_13_5_BOLD, Colour.White, false));
            _styles [PARTICLE] = new FontDefinition (_collections [PARTICLE], new TextFormat (Colour.White));

            //CreateBitmapFontsFromTTF();
        }

        QFont CreateFont (string fontface, Colour front, bool nospacing) {
            QFontShadowConfiguration shadow = new QFontShadowConfiguration ();
            shadow.blurRadius = 1;
            shadow.blurPasses = 1;

            return CreateFont (fontface, front, shadow, nospacing);
        }

        QFont CreateFont (string fontface, Colour front, QFontShadowConfiguration shadowconfig, bool nospacing) {
            QFontLoaderConfiguration config = new QFontLoaderConfiguration (true);
            //config.ShadowConfig.blurRadius = 1;
            //config.ShadowConfig.blurPasses = 1;
            config.ShadowConfig = shadowconfig;

            QFont font = new QFont (new FontLoadDescription (new FontResourcesRepo (fontface), 1.0f, config));
            font.Options.DropShadowActive = true;
            font.Options.DropShadowOffset = new OpenTK.Vector2 (0.125f, 0.125f);
            font.Options.DropShadowOpacity = 1f;

            font.Options.Colour = Conversion.ColourToColor4 (front);
            if (nospacing) {
                font.Options.CharacterSpacing = 0f;
            }

            return font;
        }

        /*
        static readonly float[] SIZES_TO_CREATE = new float[] {
            10.5f,
            13.5f,
            16.5f
        };

        struct FontDef {
            public string Output;
            public string Face;
            public FontStyle Style;

            public FontDef (string output, string face, FontStyle style) {
                Output = output;
                Face = face;
                Style = style;
            }
        }

        List<FontDef> FONTS_TO_CREATE = new List<FontDef> () {
            new FontDef ("Sansation_Regular", "Sansation_Regular", FontStyle.Regular),
            new FontDef ("Sansation_Underline", "Sansation_Regular", FontStyle.Underline),
            new FontDef ("Sansation_Bold", "Sansation_Bold", FontStyle.Bold),
            new FontDef ("Sansation_Bold_Underline", "Sansation_Bold", FontStyle.Bold | FontStyle.Underline),
            new FontDef ("Sansation_Italic", "Sansation_Italic", FontStyle.Italic),
            new FontDef ("Sansation_Italic_Underline", "Sansation_Italic", FontStyle.Italic | FontStyle.Underline),
            new FontDef ("Sansation_Bold_Italic", "Sansation_Bold_Italic", FontStyle.Bold | FontStyle.Italic),
            new FontDef ("Sansation_Bold_Italic_Underline", "Sansation_Bold_Italic", FontStyle.Bold | FontStyle.Italic | FontStyle.Underline)
        };
        const string OUTPUT_STRING = "Sansation";
        const string SUPPORTED_CHARSET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,;'\"(!?)+-/*÷=_{}[]@~#\\<>|^%$¢€£¥¤¿¡¶§µ©®™&∞≤ƒ¹²³¼½¾•ÞÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØŠÙÚÛÜŸÝŽßŒàáâãäåæçèéêëìíîïñòóôõöøšùúûüýÿž≠†‡¬…¸‰·º";

        void CreateBitmapFontsFromTTF () {
            System.Drawing.Text.PrivateFontCollection fontCollection = new System.Drawing.Text.PrivateFontCollection ();
            FixedGlyph moneyGlyph = new FixedGlyph (new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Coin.png"))));
            moneyGlyph.AddBitmap (11, new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Coin16x16.png"))));

            FixedGlyph stockGlyph = new FixedGlyph (new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Stock12x12.png"))));
            stockGlyph.AddBitmap (11, new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Stock16x16.png"))));

            FixedGlyph mbRGlyph = new FixedGlyph (new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/MouseRMB12x12.png"))));
            mbRGlyph.AddBitmap (11, new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/MouseRMB16x16.png"))));

            FixedGlyph mbLGlyph = new FixedGlyph (new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/MouseLMB12x12.png"))));
            mbLGlyph.AddBitmap (11, new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/MouseLMB16x16.png"))));

            FixedGlyph mbMGlyph = new FixedGlyph (new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/MouseMMB12x12.png"))));
            mbMGlyph.AddBitmap (11, new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/MouseMMB16x16.png"))));

            FixedGlyph ctrlGlyph = new FixedGlyph (new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Ctrl12x12.png"))));
            ctrlGlyph.AddBitmap (11, new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Ctrl16x16.png"))));
            FixedGlyph altGlyph = new FixedGlyph (new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Alt12x12.png"))));
            altGlyph.AddBitmap (11, new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Alt16x16.png"))));
            FixedGlyph cmdGlyph = new FixedGlyph (new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Cmd12x12.png"))));
            cmdGlyph.AddBitmap (11, new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Cmd16x16.png"))));
            FixedGlyph shiftGlyph = new FixedGlyph (new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Shift12x12.png"))));
            shiftGlyph.AddBitmap (11, new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Shift16x16.png"))));
            FixedGlyph tabGlyph = new FixedGlyph (new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Tab12x12.png"))));
            tabGlyph.AddBitmap (11, new Bitmap (File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, "ErsatzChars/Tab16x16.png"))));

            foreach (FontDef entry in FONTS_TO_CREATE) {

                using (Stream stream = File.OpenRead (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, entry.Face + ".ttf"))) {
                    int streamlength = (int)stream.Length;
                    byte[] fontdata = new byte[streamlength];
                    stream.Read (fontdata, 0, streamlength);
                    stream.Close ();

                    IntPtr marshalled = Marshal.AllocCoTaskMem (streamlength);
                    Marshal.Copy (fontdata, 0, marshalled, streamlength);

                    fontCollection.AddMemoryFont (marshalled, streamlength);

                    Marshal.FreeCoTaskMem (marshalled);
                }

                QFontBuilderConfiguration config = new QFontBuilderConfiguration (true);
                config.CharSet = SUPPORTED_CHARSET;
                config.FixedGlyphs ['$'] = moneyGlyph;
                config.FixedGlyphs ['≠'] = stockGlyph;
                config.FixedGlyphs ['†'] = mbRGlyph;
                config.FixedGlyphs ['‡'] = mbLGlyph;
                config.FixedGlyphs ['¬'] = mbMGlyph;
                config.FixedGlyphs ['…'] = ctrlGlyph;
                config.FixedGlyphs ['¸'] = altGlyph;
                config.FixedGlyphs ['‰'] = cmdGlyph;
                config.FixedGlyphs ['·'] = shiftGlyph;
                config.FixedGlyphs ['º'] = tabGlyph;
                config.TextGenerationRenderHint = TextGenerationRenderHint.AntiAliasGridFit;

                DirectoryInfo dir = new DirectoryInfo (GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, entry.Style.ToString ().Replace (", ", "_")));
                if (!dir.Exists) {
                    dir.Create ();
                }

                foreach (float size in SIZES_TO_CREATE) {
                    Console.Out.WriteLine ("Generating size: {0:0.0}", size);
                    QFont.CreateTextureFontFiles (new Font (fontCollection.Families [0], size, entry.Style), GameAccess.Folders.GetFilePath (Constants.PATH_FONTS, string.Format ("{0}/{1}_{2:0.0}", entry.Style.ToString ().Replace (", ", "_"), OUTPUT_STRING, size).Replace (",", "_")), config);
                }
            }
        }
        */
    }
}
