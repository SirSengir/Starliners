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

﻿using System.Collections.Generic;
using BLibrary.Util;
using BLibrary.Graphics;
using BLibrary.Gui.Widgets;
using BLibrary.Gui.Backgrounds;
using Starliners;
using BLibrary.Gui.Data;

namespace BLibrary.Gui {

    public abstract class Tooltip : WidgetContainer {
        #region Constants

        public const int MAX_TOOLTIP_WIDTH = 480;
        protected static readonly Vect2i TOOLTIP_PADDING = new Vect2i (12, 12);
        static readonly Vect2i TOOLTIP_OFFSET = new Vect2i (18, 18);

        public const string HEADER_FORMAT = "§#ffd700§{0}";
        public const string SUBSCRIPT_FORMAT = "§#00bfff§{0}";
        public const string PRICE_FORMAT0 = "§#00bfff§$ {0:0,0}";
        public const string PRICE_FORMAT1 = "§#ffff00§$ {0}";
        public const string PRICE_FORMAT2 = "§#FF4500§$ {0}";

        #endregion

        #region Properties

        public override Vect2i PositionAbsolute {
            get { return PositionRelative; }
        }

        public ITooltipController Controller {
            get {
                return _controller ?? Parent;
            }
            set {
                _controller = value;
            }
        }

        protected bool HeaderOnly {
            get;
            set;
        }

        protected Vect2i CornerTopLeft {
            get {
                return _header == null ? TOOLTIP_PADDING : TOOLTIP_PADDING + new Vect2i (0, 30);
            }
        }

        #endregion

        #region Fields

        Label _header;
        Background _headerbgmulti;
        Background _headerbgsingle;
        ITooltipController _controller;

        #endregion

        public Tooltip (Vect2i size) {
            Size = size;
            Backgrounds = UIProvider.Styles ["tooltip"].CreateBackgrounds ();
            Tinting = new BackgroundTinting ();
        }

        #region Events

        /// <summary>
        /// Raised whenever the tooltip is shown.
        /// </summary>
        public virtual void OnShown () {
        }

        #endregion

        public override void Update () {
            base.Update ();
            AdjustPosition ();
        }

        protected virtual void AdjustPosition () {
            PositionRelative = GuiManager.Instance.MouseGuiPosition + TOOLTIP_OFFSET;
            PositionRelative = new Vect2i (
                PositionRelative.X < 0 ? 0 : PositionRelative.X > GameAccess.Interface.WindowSize.X - Size.X ? GameAccess.Interface.WindowSize.X - Size.X : PositionRelative.X,
                PositionRelative.Y < 0 ? 0 : PositionRelative.Y > GameAccess.Interface.WindowSize.Y - Size.Y ? GameAccess.Interface.WindowSize.Y - Size.Y : PositionRelative.Y
            );
        }

        protected void AddHeader (string header, int innerOffset) {
            AddHeader (new TextComponent (header) { Template = HEADER_FORMAT }, innerOffset);
        }

        protected void AddHeader (ITextProvider header, int innerOffset) {
            //header.Template = Constants.TOOLTIPS_HEADER_FORMAT;

            _headerbgmulti = new BackgroundDynamic ("tooltipHeader");
            _headerbgsingle = new BackgroundDynamic ("tooltipSingle");
            _header = new Label (new Vect2i (innerOffset, 0), new Vect2i (Size.X - innerOffset, 36), header) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
        }

        protected void SetHeader (string header) {
            _header.Template = string.Format (HEADER_FORMAT, header);
        }

        protected void DrawHeader (Vect2i size, RenderTarget target, RenderStates states) {
            if (_header == null) {
                return;
            }

            if (!HeaderOnly) {
                _headerbgmulti.Render (size, target, states, this);
            } else {
                _headerbgsingle.Render (size, target, states, this);
            }
            _header.Size = size;
            _header.Draw (target, states);
        }

        protected abstract Vect2i GetDimensions ();

        protected Vect2i DetermineDimensions () {
            Vect2i required = GetDimensions ();
            if (required.X < _header.GetDimensions (MAX_TOOLTIP_WIDTH).X) {
                required = new Vect2i (_header.GetDimensions (MAX_TOOLTIP_WIDTH).X, required.Y);
            }

            if (HeaderOnly) {
                return new Vect2i (required.X + TOOLTIP_PADDING.X * 2, 36);
            } else {
                return required + TOOLTIP_PADDING * 2 + new Vect2i (0, _header != null ? 30 : 0);
            }
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            states.Transform.Translate (PositionRelative);
            Size = DetermineDimensions ();

            DrawBackground (target, states);
            DrawHeader (new Vect2i (Size.X, 36), target, states);
            DrawChildren (target, states);
        }

        protected IList<string> CombineWithSeperation (params IList<string>[] text) {
            List<string> combined = new List<string> ();
            for (int i = 0; i < text.Length; i++) {
                if (text [i].Count <= 0) {
                    continue;
                }
                if (combined.Count > 0) {
                    combined.Add (string.Empty);
                }

                combined.AddRange (text [i]);
            }

            return combined;
        }
    }
}
