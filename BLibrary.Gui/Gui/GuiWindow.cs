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
using OpenTK.Input;
using BLibrary.Util;
using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using BLibrary.Gui.Widgets;
using BLibrary.Gui.Tooltips;
using Starliners;
using System.Collections.Generic;

namespace BLibrary.Gui {

    [Flags]
    public enum WindowButton {
        None = 0,
        Close = 1 << 1,
        Position = 1 << 2,
        Compact = 1 << 3
    }

    public abstract class GuiWindow : WidgetContainer, IDraggable {
        #region Constants

        protected const int FORCE_PADD = 16;
        protected const int HEADER_HEIGHT = 40;
        protected const int HEADER_BORDER_ADJUST = 5;
        protected const WindowButton DEFAULT_BUTTONS = WindowButton.Close | WindowButton.Position | WindowButton.Compact;
        protected const WindowButton DEFAULT_BUTTONS_NOTAG = WindowButton.Close | WindowButton.Position;

        #endregion

        #region Classes

        protected abstract class ControlOption {
            public abstract Widget GetOptionWidget (Vect2i offset, Vect2i size);
        }

        protected sealed class PlainOption : ControlOption {
            public Tooltip Tooltip {
                get;
                set;
            }

            public object[] ActionArgs {
                get;
                private set;
            }

            string _action;
            string _symbol;

            public PlainOption (string action, string symbol, params object[] actionargs) {
                _action = action;
                _symbol = symbol;
                ActionArgs = actionargs;
            }

            public override Widget GetOptionWidget (Vect2i offset, Vect2i size) {
                return new Button (offset, size, _action, new IconSymbol (new Vect2i (16, 8), size - new Vect2i (32, 16), _symbol), ActionArgs) {
                    Backgrounds = UIProvider.Style.TabStyle.CreateBackgrounds (),
                    FixedTooltip = Tooltip
                };
            }
        }

        protected sealed class TabOption : ControlOption {
            public Tooltip Tooltip {
                get;
                set;
            }

            string _tab;
            string _symbol;

            public TabOption (string tab, string symbol) {
                _tab = tab;
                _symbol = symbol;
            }

            public override Widget GetOptionWidget (Vect2i offset, Vect2i size) {
                return new Button (offset, size, Constants.CONTAINER_KEY_TAB_SWITCH, new IconSymbol (new Vect2i (16, 8), size - new Vect2i (32, 16), _symbol), _tab) {
                    Backgrounds = UIProvider.Style.TabStyle.CreateBackgrounds (),
                    FixedTooltip = Tooltip
                };
            }
        }

        #endregion

        #region Properties

        public WindowPresets Presets {
            get {
                return _presets;
            }
            protected set {
                _presets = value;
                PositionRelative = GuiManager.Instance.GetGuiPosition (_presets);
                Size = _presets.GetOuterSize (UIProvider);
            }
        }

        public override sealed Vect2i PositionAbsolute {
            get {
                if (Parent == null) {
                    return PositionRelative;
                } else {
                    return Parent.PositionAbsolute + Parent.PositionShift + PositionRelative;
                }
            }
        }

        /// <summary>
        /// Used to indicate gui ordering.
        /// </summary>
        /// <value>The ordering.</value>
        public int Ordering {
            get;
            set;
        }

        /// <summary>
        /// Windows can fade out if this is set.
        /// </summary>
        public int FadeOut {
            get;
            protected set;
        }

        /// <summary>
        /// Indicates whether clicks/input on the window itself - without activating a widget - count as handled or not.
        /// </summary>
        public bool IsDisembodied {
            get;
            protected set;
        }

        /// <summary>
        /// Indicates whether this window can be dragged with the mouse or not.
        /// </summary>
        /// <value><c>true</c> if this instance is draggable; otherwise, <c>false</c>.</value>
        public bool IsDraggable {
            get;
            protected set;
        }

        /// <summary>
        /// Indicates whether the window can be closed by the user or not.
        /// </summary>
        /// <value><c>true</c> if this instance is closeable; otherwise, <c>false</c>.</value>
        public bool IsCloseable {
            get;
            set;
        }

        /// <summary>
        /// Indicates or sets whether this gui window is currently set to compacted (minimized) state.
        /// </summary>
        /// <value><c>true</c> if this instance is compacted; otherwise, <c>false</c>.</value>
        public bool IsCompacted {
            get { return _compact; }
            set {
                _compact = value;
                OnCompactChange ();
            }
        }

        protected abstract bool CanDraw {
            get;
        }

        #region Appearance

        /// <summary>
        /// Gets or sets the icon used in compact mode.
        /// </summary>
        /// <value>The icon.</value>
        protected IconLayer Icon {
            get;
            set;
        }

        #endregion

        protected Vect2i CornerTopLeft {
            get {
                return new Vect2i (UIProvider.Margin.X, _hasHeader ? UIProvider.Margin.Y + HEADER_HEIGHT - HEADER_BORDER_ADJUST : UIProvider.Margin.Y);
            }
        }

        #endregion

        #region Fields

        bool _hasHeader = false;
        bool _compact;
        int _age;
        WindowPresets _presets;
        int _compactCell;
        List<ControlOption> _controlOptions = new List<ControlOption> ();
        Dictionary<string, Widget> _tabbedWidgets = new Dictionary<string, Widget> ();

        #endregion

        #region Constructor

        public GuiWindow (WindowPresets presets) {
            Presets = presets;
            Tinting = UIProvider.Style.Tinting;
            IsCloseable = true;
            IsDraggable = true;
            IsSensitive = true;

            if (!string.IsNullOrWhiteSpace (presets.Style)) {
                Backgrounds = UIProvider.Styles [presets.Style].CreateBackgrounds ();
            }
            Icon = new IconLayer (SpriteManager.Instance.RegisterSingle ("magnifier"));
        }

        #endregion

        #region Updating

        /// <summary>
        /// Updates the gui window.
        /// </summary>
        public override void Update () {
            base.Update ();

            foreach (Widget widget in Children) {
                widget.Update ();
            }

            // Handle windows which fade out.
            if (FadeOut > 0) {
                if (!State.HasFlag (ElementState.Hovered)) {
                    _age++;
                    if (_age > FadeOut) {
                        GuiManager.Instance.CloseGui (Presets.Key);
                    }
                } else {
                    _age = 0;
                }
            }
        }

        protected override void Regenerate () {
            base.Regenerate ();
            if (_controlOptions.Count > 0) {
                AddControlPanel (_controlOptions);
            }
            _tabbedWidgets.Clear ();
        }

        #endregion

        #region Widget Managment

        /// <summary>
        /// Adds a standard header with buttons if requested.
        /// </summary>
        /// <param name="cornerTopLeft"></param>
        /// <param name="buttons"></param>
        /// <param name="template"></param>
        protected void AddHeader (WindowButton buttons, object template) {
            AddWidget (new Header (new Vect2i (Size.X, HEADER_HEIGHT), buttons, template));
            _hasHeader = true;
        }

        /// <summary>
        /// Adds a control option to this window.
        /// </summary>
        /// <param name="option">Option.</param>
        protected void AddControlOption (ControlOption option) {
            _controlOptions.Add (option);
        }

        void AddControlPanel (IList<ControlOption> options) {

            Vect2i iconsize = new Vect2i (48 + 32, 48 + 16);
            Grouping control = new Grouping (new Vect2i (Size.X - 14, UIProvider.Margin.Y + HEADER_HEIGHT - HEADER_BORDER_ADJUST), new Vect2i (iconsize.X + 16, Size.Y - 2 * UIProvider.Margin.Y)) {
                AlignmentV = Alignment.Top
            };
            AddWidget (control);

            for (int i = 0; i < options.Count; i++) {
                control.AddWidget (options [i].GetOptionWidget (new Vect2i (0, iconsize.Y) * i, iconsize));
            }

        }

        /// <summary>
        /// Adds the given widget to the given tab.
        /// </summary>
        /// <param name="tab">Tab.</param>
        /// <param name="widget">Widget.</param>
        protected void AddWidget (string tab, Widget widget) {
            _tabbedWidgets [tab].AddWidget (widget);
        }

        /// <summary>
        /// Creates a default tab at the given position with the given size.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="position">Position.</param>
        /// <param name="size">Size.</param>
        protected void CreateTab (string key, Vect2i position, Vect2i size) {
            AddTab (key, new Grouping (position, size));
        }

        /// <summary>
        /// Marks the given widget as a tab in this window.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="widget">Widget.</param>
        protected void AddTab (string key, Widget widget) {
            AddWidget (widget);
            if (_tabbedWidgets.Count > 0) {
                widget.IsDisplayed = false;
            }
            _tabbedWidgets [key] = widget;
            if (string.Equals (Presets.SavedTab.Active, key)) {
                ActivateTab (key);
            }
        }

        /// <summary>
        /// Displays the given tag.
        /// </summary>
        /// <param name="key">Key.</param>
        protected void ActivateTab (string key) {
            HideTabs ();
            _tabbedWidgets [key].IsDisplayed = true;
            Presets.SavedTab.Active = key;
        }

        void HideTabs () {
            foreach (var entry in _tabbedWidgets) {
                entry.Value.IsDisplayed = false;
            }
        }

        #endregion

        #region Rendering

        public override void Draw (RenderTarget target, RenderStates states) {
            // Wait until the container is open before doing anything.
            if (!CanDraw) {
                return;
            }

            // Draw according to state.
            if (IsCompacted) {
                DrawCompact (target, states);
            } else {
                DrawExpanded (target, states);
            }
        }

        #region Compact

        Vect2i GetCompactPosition () {
            int perRow = (GameAccess.Interface.WindowSize.Y - 2 * GuiManager.GRID_EDGE) / GuiManager.GRID_EDGE;
            int rows = _compactCell / perRow;
            int yGrid = _compactCell - (rows * perRow);

            return new Vect2i (GameAccess.Interface.WindowSize.X - (rows * GuiManager.GRID_EDGE) - GuiManager.GRID_EDGE, yGrid * GuiManager.GRID_EDGE + GuiManager.GRID_EDGE);
        }

        void DrawCompact (RenderTarget target, RenderStates states) {
            // Set to current compact cell.
            _compactCell = GuiManager.Instance.GetNextCompactCell ();

            Vect2i pos = GetCompactPosition ();
            states.Transform.Translate (pos.X, pos.Y);

            UIProvider.Backgrounds ["gui.compact"].Render (GuiManager.COMPACT_SIZE, target, states, this);
            states.Transform.Scale (2f, 2f);
            target.Draw (SpriteManager.Instance [Icon], states);
        }

        #endregion

        #region Expanded

        void DrawExpanded (RenderTarget target, RenderStates states) {

            states.Transform.Translate (PositionAbsolute);
            DrawBackground (target, states);
            DrawChildren (target, states);

            /*
            if (FadeOut > 0) {
                float alpha = ((1.25f - ((float)_age / FadeOut)) * 255);
                alpha = alpha > 255 ? 255 : alpha;
                buffer.Colour = new Colour(buffer.Colour.R, buffer.Colour.G, buffer.Colour.B, (byte)alpha);
            } */
            //target.Draw(buffer, states);

        }

        #endregion

        #endregion

        public Tooltip CreateTooltip (Vect2i coordinates) {

            if (IsCompacted && IntersectsWith (coordinates)) {
                return CompactTooltip;
            }

            Tooltip tooltip = null;
            foreach (Widget widget in Children) {
                if (!widget.IsDisplayed) {
                    continue;
                }
                if (widget.IntersectsWith (coordinates)) {
                    tooltip = widget.GetTooltip (coordinates);
                }
                if (tooltip != null) {
                    break;
                }
            }

            return tooltip;
        }

        protected static readonly string[] TT_COMPACT_HELP = new string[] {
            "tt_compact_leftclick",
            "tt_compact_rightclick"
        };

        protected virtual Tooltip CompactTooltip {
            get { return new TooltipSimple ("Minimized Window", TT_COMPACT_HELP) { Parent = this }; }
        }

        #region Interaction

        #region Text Events

        public virtual bool HandleTextEntered (char unicode) {

            if (IsCompacted || !CanDraw)
                return false;

            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                if (Children [i].HandleTextEntered (unicode))
                    return true;
            }

            return false;
        }

        public virtual bool HandleKeyPress (Key key) {

            if (IsCompacted || !CanDraw)
                return false;

            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                if (Children [i].HandleKeyPress (key))
                    return true;
            }

            return false;
        }

        #endregion

        #region Mouse events

        public virtual bool HandleMouseMove (Vect2i coordinates) {

            if (!CanDraw)
                return false;

            VerifyInteraction (coordinates, false, false);

            if (IsCompacted/* || !IntersectsWith(coordinates)*/)
                return false;

            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                if (Children [i].HandleMouseMove (coordinates))
                    return true;
            }

            return !IsDisembodied && IntersectsWith (coordinates);
        }

        public virtual bool HandleMouseClick (GuiManager manager, Vect2i coordinates, MouseButton button) {

            if (!CanDraw) {
                return false;
            }

            // Compacted windows need special handling here.
            if (IsCompacted) {
                if (!IntersectsWith (coordinates)) {
                    return false;
                }

                if (button == MouseButton.Right) {
                    Close ();
                } else {
                    IsCompacted = false;
                }
                return true;
            }

            VerifyInteraction (coordinates, true, false);

            // Inner elements take precedence
            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                //Console.Out.WriteLine ("Checking child {0} for {1} ", this.GetType(), Children[i].GetType());
                if (Children [i].HandleMouseClick (coordinates, button)) {
                    return true;
                }
            }

            return !IsDisembodied && IntersectsWith (coordinates);
        }

        public virtual bool HandleMouseRelease (GuiManager manager, Vect2i coordinates, MouseButton button) {

            if (IsCompacted || !CanDraw)
                return false;

            VerifyInteraction (coordinates, false, true);

            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                if (Children [i].HandleMouseRelease (coordinates, button))
                    return true;
            }

            return !IsDisembodied && IntersectsWith (coordinates);

        }

        public virtual bool HandleMouseWheel (GuiManager manager, Vect2i coordinates, int delta) {

            if (IsCompacted || !CanDraw)
                return false;

            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                if (Children [i].HandleMouseWheel (coordinates, delta))
                    return true;
            }

            return !IsDisembodied && IntersectsWith (coordinates);
        }

        #endregion

        #endregion

        #region Actions

        /// <summary>
        /// Raised when the window changes from compact to full or back.
        /// </summary>
        protected virtual void OnCompactChange () {
        }

        public override bool DoAction (string key, params object[] args) {
            switch (key) {
                case Constants.CONTAINER_KEY_BTN_CLOSE_WINDOW:
                    Close ();
                    return true;
                case Constants.CONTAINER_KEY_BTN_POSITION:
                    WasMoved = true;
                    PositionRelative = GuiManager.Instance.GetGuiPositionDefault (Presets);
                    return true;
                case Constants.CONTAINER_KEY_BTN_COMPACT:
                    IsCompacted = true;
                    return true;
                case Constants.CONTAINER_KEY_TAB_SWITCH:
                    ActivateTab ((string)args [0]);
                    return true;
                default:
                    return base.DoAction (key, args);
            }
        }

        /// <summary>
        /// Closes this gui window.
        /// </summary>
        protected virtual void Close () {
            GuiManager.Instance.CloseGui (Presets.Key);
            IsDead = true;
            OnClosed ();
        }

        protected virtual void OnClosed () {
        }

        #endregion

        public override sealed bool IntersectsWith (Vect2i coordinates) {
            if (!IsCompacted) {
                return base.IntersectsWith (coordinates);
            }

            Vect2i pos = GetCompactPosition ();
            return Utils.IntersectsWith (coordinates.X, coordinates.Y, pos.X, pos.Y, GuiManager.COMPACT_SIZE.X, GuiManager.COMPACT_SIZE.Y);
        }
    }
}
