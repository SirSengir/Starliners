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
using BLibrary.Graphics.Sprites;
using BLibrary.Util;

namespace BLibrary.Gui.Widgets {

    public sealed class LoadBar : Widget {
        #region Constants

        const float BASE_SPEED = 6;
        static readonly Vect2i ICON_SIZE = new Vect2i (64, 64);
        static readonly string[] ICONS_TO_USE = new string[] {
            "load-symbol-0",
            "load-symbol-0",
            "load-symbol-1",
            "load-symbol-1",
            "load-symbol-1",
            "load-symbol-1",
            "load-symbol-2",
            "load-symbol-2",
            "load-symbol-2",
            "load-symbol-3",
            "load-symbol-3",
            "load-symbol-4"
        };
        /*
        static readonly Colour[] BODY_COLOURS = new Colour[] {
            new Colour (0xffdc16), new Colour (0xffbe16), new Colour (0xedf56b), new Colour (0xffff96), new Colour (0xffa600)
        };*/

        #endregion

        #region Classes

        sealed class Floater {
            public readonly IconSymbol Widget;
            public int Age;
            public readonly int Delay;
            public readonly int MaxAge;
            public float Way = 0.01f;

            public Floater (IconSymbol widget, int maxAge, int delay) {
                Widget = widget;
                MaxAge = maxAge;
                Delay = delay;
            }
        }

        #endregion

        #region Properties

        public float FillState {
            get;
            set;
        }

        #endregion

        Random _rand;
        Rectangle _fill;
        Floater[] _floating;
        Vect2i _floatstart;
        Vect2i _floatend;
        uint[][] _floatindices;
        object _template;

        public LoadBar (Vect2i position, Vect2i size, object template)
            : base (position, size, "load.bar") {

            Backgrounds = UIProvider.Style.CreateInset ();
            _rand = new Random ();
            _template = template;

            _floatindices = new uint[ICONS_TO_USE.Length][];
            for (int i = 0; i < ICONS_TO_USE.Length; i++) {
                _floatindices [i] = SpriteManager.Instance.RegisterIcon (ICONS_TO_USE [i]);
            }

        }

        protected override void Regenerate () {
            base.Regenerate ();

            int padding = (Size.Y - ICON_SIZE.Y) / 2;
            AddWidget (new IconSymbol (new Vect2i (padding, padding), ICON_SIZE, "load-start"));
            AddWidget (new IconSymbol (new Vect2i (Size.X - ICON_SIZE.X - padding, padding), ICON_SIZE, "load-end"));
            AddWidget (new Label (new Vect2i (), Size, _template) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            });

            _floatstart = new Vect2i (padding + ICON_SIZE.X, padding);
            _floatend = new Vect2i (Size.X - ICON_SIZE.X * 2 - padding, padding);
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            if (_floating == null) {
                RegenerateFloating ();
            }

            bool underway = false;
            // Update floating icons, remove them as necessary.
            for (int i = 0; i < _floating.Length; i++) {
                Floater floater = _floating [i];
                if (floater.Widget.PositionRelative.X >= _floatend.X) {
                    continue;
                }

                underway = true;
                floater.Age++;
                if (floater.Delay > floater.Age) {
                    continue;
                }

                floater.Way += BASE_SPEED * (float)Math.Sqrt ((float)floater.Age / floater.MaxAge);
                floater.Widget.PositionRelative = _floatstart + new Vect2i ((int)floater.Way, 0);
                if (floater.Widget.PositionRelative.X >= _floatend.X) {
                    floater.Widget.PositionRelative = new Vect2i (_floatend.X, floater.Widget.PositionRelative.Y);
                }

            }

            // Remove if all floaters have been processed.
            if (!underway) {
                for (int i = 0; i < _floating.Length; i++) {
                    RemoveWidget (_floating [i].Widget);
                }
                _floating = null;
            }

            states.Transform.Translate (PositionRelative);
            DrawBackground (target, states);

            if (FillState > 0) {
                int padd = 8;
                Vect2i fillsize = new Vect2i ((int)((Size.X - padd) * FillState), Size.Y - padd);
                if (_fill == null) {
                    _fill = new Rectangle (fillsize) {
                        Position = new Vect2i (padd / 2, padd / 2),
                        Colour = new Colour (Colour.Crimson, 160)
                    };
                } else {
                    _fill.Size = fillsize;
                }

                _fill.Draw (target, states);
            }

            DrawChildren (target, states);
        }

        void RegenerateFloating () {
            Floater[] floating = new Floater[4];
            //Colour frontColour = Colour.GetRandom (_rand);
            //Colour backColour = BODY_COLOURS [_rand.Next (BODY_COLOURS.Length)];

            for (int i = 0; i < floating.Length; i++) {
                // Create the SpriteBatch
                uint[] indices = _floatindices [_rand.Next (_floatindices.Length)];
                ModelReel reel = new ModelReel (new IconLayer (indices [0], new Colour (Colour.Turquoise, 128)),
                                     new IconLayer (indices [1], Colour.DarkGray), new IconLayer (indices [2]));

                SpriteBatch batch = SpriteManager.CompressToBatch (new ModelReel[] { reel });
                // Create the IconSymbol
                floating [i] = new Floater (new IconSymbol (_floatstart, ICON_SIZE, batch), 500, (floating.Length - i) * ICON_SIZE.X);
                // Add the IconSymbol as a child widget
                AddWidget (floating [i].Widget);
            }

            _floating = floating;
        }
    }
}

