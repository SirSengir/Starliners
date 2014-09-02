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

﻿using BLibrary.Util;
using BLibrary.Graphics;
using BLibrary.Gui.Backgrounds;

namespace BLibrary.Gui.Widgets {

    public abstract class Scrollable : Widget {
        #region Constants

        const int SCROLLBAR_WIDTH = 12;
        static readonly Vect2i INDICATOR_SIZE = new Vect2i (14, 32);

        #endregion

        #region Classes

        sealed class Scrollbar : Widget {
            Scrollable _scrollable;

            public Scrollbar (Vect2i position, float height, Scrollable scrollable)
                : base (position, new Vect2i (SCROLLBAR_WIDTH, (int)height)) {
                IsSensitive = true;
                _scrollable = scrollable;
                Backgrounds = new BackgroundCollection (new BackgroundDynamic ("guiScrollbar"));
            }

            public override bool HandleMouseClick (Vect2i coordinates, OpenTK.Input.MouseButton button) {

                base.HandleMouseClick (coordinates, button);

                if (IntersectsWith (coordinates)) {
                    SetScroll (coordinates.Y);
                    return true;
                }

                return false;
            }

            public override bool HandleMouseMove (Vect2i coordinates) {
                if (!State.HasFlag (ElementState.Pressed)) {
                    return false;
                }

                if (IntersectsWith (coordinates)) {
                    SetScroll (coordinates.Y);
                } else {
                    int delta = coordinates.Y - GuiManager.Instance.LastMousePos.Y;
                    _scrollable.Scroll += new Vect2i (0, delta * ((float)_scrollable.MaxScroll.Y / _scrollable.Size.Y));
                }
                return true;
            }

            void SetScroll (int intersect) {
                int inner = intersect - PositionAbsolute.Y;
                _scrollable.Scroll = new Vect2i (0, ((float)inner / Size.Y) * _scrollable.MaxScroll.Y);
            }
        }

        sealed class ScrollIndicator : Widget {
            public ScrollIndicator (Vect2i position)
                : base (position, INDICATOR_SIZE) {
                Backgrounds = new BackgroundCollection (new BackgroundDynamic ("guiScrollbarSlider"));
            }
        }

        #endregion

        #region Properties

        public override Vect2i PositionShift {
            get {
                return -Scroll;
            }
        }

        bool ShowScroll {
            get {
                return _showbar > 0;
            }
        }

        protected Vect2f EffectiveSize {
            get {
                if (ShowScroll) {
                    return Size - new Vect2f (INDICATOR_SIZE.X, 0);
                } else {
                    return Size;
                }
            }
        }

        public Vect2i Scroll {
            get {
                return _scroll;
            }
            set {
                _scroll = new Vect2i (
                    value.X > MaxScroll.X ? MaxScroll.X : value.X,
                    value.Y > MaxScroll.Y ? MaxScroll.Y : value.Y
                );
                if (_scroll.X < 0 || _scroll.Y < 0) {
                    _scroll = new Vect2i (
                        value.X < 0 ? 0 : value.X,
                        value.Y < 0 ? 0 : value.Y
                    );
                }
            }
        }

        Vect2f MaxScroll {
            get;
            set;
        }

        protected virtual Rect2i ViewportArea {
            get {
                return new Rect2i (PositionAbsolute, Size);
            }
        }

        #endregion

        #region Fields

        byte _showbar = 0;
        float _stackedscroll = 0;
        ScrollIndicator _scrollindicator;
        Scrollbar _scrollbar;

        Vect2i _scroll;

        #endregion

        #region Constructors

        public Scrollable (Vect2i size, string key)
            : this (new Vect2i (0, 0), size, key) {
        }

        public Scrollable (Vect2i position, Vect2i size, string key)
            : base (position, size, key) {
            IsSensitive = true;
        }

        #endregion

        protected void RefreshDimensions () {
            Vect2f dimensions = DetermineDimensions (Size.X);
            if (dimensions.Y > Size.Y) {
                _showbar = 250;
                MaxScroll = DetermineDimensions ((int)EffectiveSize.X) - Size * 0.5f;
            } else {
                _showbar = 0;
                MaxScroll = Vect2i.ZERO;
            }
        }

        public override void Update () {
            base.Update ();
            if (_stackedscroll > 0) {
                _stackedscroll--;
            }
        }

        public override sealed void Draw (RenderTarget target, RenderStates states) {
            states.Transform.Translate (PositionRelative);
            DrawBackground (target, states);

            RenderStates scrolled = states;
            scrolled.Transform.Translate (-Scroll);
            target.EnableScissor (ViewportArea);
            DrawPort (target, scrolled);
            target.DisableScissor ();

            // Set scrollbar
            if (ShowScroll) {

                CreateScrollbarIfNeeded ();
                _scrollbar.Size = new Vect2i (SCROLLBAR_WIDTH, EffectiveSize.Y);
                _scrollbar.PositionRelative = new Vect2i (EffectiveSize.X + (INDICATOR_SIZE.X - SCROLLBAR_WIDTH) / 2, 0);
                _scrollbar.Draw (target, states);

                float indY = 0;
                if (MaxScroll.Y > 0) {
                    indY = Scroll.Y / MaxScroll.Y;
                }
                _scrollindicator.PositionRelative = new Vect2i (EffectiveSize.X, 1 + (EffectiveSize.Y - INDICATOR_SIZE.Y - 2) * indY);
                _scrollindicator.Draw (target, states);
            }
        }

        protected abstract Vect2f DetermineDimensions (int fixedWidth);

        protected abstract void DrawPort (RenderTarget target, RenderStates states);

        void CreateScrollbarIfNeeded () {
            if (_scrollbar != null && _scrollindicator != null) {
                return;
            }

            _scrollbar = new Scrollbar (Vect2i.ZERO, EffectiveSize.Y, this);
            AddWidget (_scrollbar);

            _scrollindicator = new ScrollIndicator (Vect2i.ZERO);
            AddWidget (_scrollindicator);
        }

        protected override void OnResized () {
            base.OnResized ();
            RefreshDimensions ();
        }

        public override bool IntersectsWith (Vect2i coordinates) {
            return Utils.IntersectsWith (coordinates.X, coordinates.Y, PositionAbsolute.X, PositionAbsolute.Y, Size.X, Size.Y);
        }

        public override bool HandleMouseWheel (Vect2i coordinates, int delta) {

            if (!IntersectsWith (coordinates)) {
                _stackedscroll = 0;
                return false;
            }

            if (_stackedscroll < 20)
                _stackedscroll += 10;

            float speed = 10;
            Scroll += new Vect2i (0, delta * speed * (1 + _stackedscroll / 10));

            return true;
        }
    }
}
