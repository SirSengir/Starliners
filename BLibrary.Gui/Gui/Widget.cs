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

﻿using OpenTK.Input;
using System;
using BLibrary.Graphics;
using BLibrary.Util;
using BLibrary.Audio;

namespace BLibrary.Gui {

    public abstract class Widget : GuiElement, IDisposable {
        #region Classes

        public abstract class ClickAction {
            public abstract void DoAction (WidgetContainer window, ControlState control);
        }

        public sealed class ClickPlain : ClickAction {

            public string Sound {
                get;
                set;
            }

            string _key;
            object[] _actionargs;

            public ClickPlain (string key, params object[] args) {
                _key = key;
                _actionargs = args;
            }

            public override void DoAction (WidgetContainer window, ControlState control) {
                // Play sound if needed.
                if (!string.IsNullOrEmpty (Sound)) {
                    SoundManager.Instance.Play (Sound);
                }

                // Send with additional args or without
                if (_actionargs.Length > 0) {
                    object[] args = new object[_actionargs.Length + 1];
                    Array.Copy (_actionargs, args, _actionargs.Length);
                    args [args.Length - 1] = control;
                    window.DoAction (_key, args);
                } else {
                    window.DoAction (_key, control);
                }
            }
        }

        public sealed class ClickCustom : ClickAction {
            EventHandler _handler;

            public ClickCustom (EventHandler handler) {
                _handler = handler;
            }

            public override void DoAction (WidgetContainer window, ControlState control) {
                _handler (this, new EventArgs ());
            }
        }

        #endregion

        #region Properties

        public override sealed Vect2i PositionAbsolute {
            get {
                if (Parent == null) {
                    return Window.PositionAbsolute + Window.PositionShift + PositionRelative;
                } else {
                    return Parent.PositionAbsolute + Parent.PositionShift + PositionRelative;
                }
            }
        }

        public virtual bool IsDisplayed {
            get;
            set;
        }

        public string Key { get; private set; }

        public override sealed WidgetContainer Window {
            get {
                return Parent.Window;
            }
        }

        /// <summary>
        /// Indicates whether the widget reacts to mouse movement by default.
        /// </summary>
        protected override bool IsSensitive {
            get {
                return _isSensitive && !State.HasFlag (ElementState.Disabled);
            }
            set {
                _isSensitive = value;
            }
        }

        /// <summary>
        /// Allows to set a fixed tooltip for this widget.
        /// </summary>
        /// <remarks>
        /// Setting this will set IsSensitive to true!
        /// </remarks>
        public Tooltip FixedTooltip {
            get {
                return _tooltip;
            }
            set {
                _tooltip = value;
                if (_tooltip != null) {
                    _tooltip.Controller = this;
                    IsSensitive = true;
                }
            }
        }

        public ClickAction ActionOnClick {
            get;
            set;
        }

        #endregion

        Tooltip _tooltip;
        bool _isSensitive = false;

        #region Constructors

        protected Widget (Vect2i position, Vect2i size)
            : base (position, size) {
            Key = ToString ();
            SetDefaults ();
        }

        protected Widget (Vect2i position, Vect2i size, string key)
            : base (position, size) {
            Key = key;
            SetDefaults ();
        }

        void SetDefaults () {
            IsDisplayed = true;
            Tinting = new ParentTinting ();
        }

        #endregion

        #region Events

        /// <summary>
        /// Called after the widget is added to a parent.
        /// </summary>
        public virtual void OnAddition (GuiElement parent) {
            Parent = parent;
        }

        #endregion

        public override void Update () {
            base.Update ();
            foreach (Widget widget in Children) {
                widget.Update ();
            }
        }

        #region Drawing

        public override void Draw (RenderTarget target, RenderStates states) {
            states.Transform.Translate (PositionRelative);
            DrawBackground (target, states);
            DrawChildren (target, states);
        }

        #endregion

        /// <summary>
        /// Return a tooltip from either this widget or a child widget.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public virtual Tooltip GetTooltip (Vect2i coordinates) {
            // Child tooltips override the main tooltip.
            for (int i = 0; i < Children.Count; i++) {
                if (Children [i].IntersectsWith (coordinates)) {
                    if (!Children [i].IsDisplayed) {
                        continue;
                    }
                    Tooltip tip = Children [i].GetTooltip (coordinates);
                    if (tip != null)
                        return tip;
                }
            }


            return FixedTooltip != null ? FixedTooltip : null;
        }

        #region Interaction

        #region Text events

        public virtual bool HandleTextEntered (char unicode) {
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
            VerifyInteraction (coordinates, false, false);

            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                if (Children [i].HandleMouseMove (coordinates))
                    return true;
            }

            return false;
        }

        public virtual bool HandleMouseRelease (Vect2i coordinates, MouseButton button) {
            VerifyInteraction (coordinates, false, true);

            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                if (Children [i].HandleMouseRelease (coordinates, button))
                    return true;
            }

            return false;
        }

        public virtual bool HandleMouseClick (Vect2i coordinates, MouseButton button) {
            VerifyInteraction (coordinates, true, false);

            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                if (Children [i].HandleMouseClick (coordinates, button)) {
                    return true;
                }
            }

            if (ActionOnClick != null && !State.HasFlag (ElementState.Disabled) && IntersectsWith (coordinates)) {
                ActionOnClick.DoAction (Window, GuiManager.Instance.CombineControlState (button));
                return true;
            }
            return false;
        }

        public virtual bool HandleMouseWheel (Vect2i coordinates, int delta) {
            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                if (Children [i].HandleMouseWheel (coordinates, delta))
                    return true;
            }

            return false;
        }

        #endregion

        #endregion

        public virtual void Dispose () {
            foreach (Widget child in Children)
                child.Dispose ();
            GC.SuppressFinalize (this);
        }
    }
}
