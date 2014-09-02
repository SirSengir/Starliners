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
using BLibrary.Util;
using BLibrary.Graphics;
using Starliners;
using BLibrary.Gui.Backgrounds;

namespace BLibrary.Gui {

    public delegate void GuiElementEventHandler (GuiElement sender, EventArgs args);
    /// <summary>
    /// The basic abstraction for all gui elements, whether they are windows, tooltips or widgets.
    /// </summary>
    public abstract class GuiElement : ITooltipController {
        #region Constants

        protected static readonly ElementState[] BG_STATES_SENSITIVE = new ElementState[] {
            ElementState.Disabled,
            ElementState.Pressed,
            ElementState.Hovered
        };

        #endregion

        #region Properties

        public bool Debug {
            get;
            set;
        }

        /// <summary>
        /// Gets the current interface definition.
        /// </summary>
        /// <value>The user interface provider.</value>
        protected static IInterfaceDefinition UIProvider {
            get {
                return GuiManager.Instance.UIProvider;
            }
        }

        /// <summary>
        /// Gets or sets the element's parent element if any.
        /// </summary>
        /// <value>The parent.</value>
        public GuiElement Parent {
            get { return _parent; }
            set {
                _parent = value;
            }
        }

        public abstract WidgetContainer Window { get; }

        protected List<Widget> Children {
            get {
                return _children;
            }
        }

        /// <summary>
        /// Absolute element position.
        /// </summary>
        public abstract Vect2i PositionAbsolute { get; }

        /// <summary>
        /// Position relative to the next surrounding element.
        /// </summary>
        public Vect2i PositionRelative { get; set; }

        /// <summary>
        /// Gets the inner position shift in the element relative to actual screen coordinates.
        /// </summary>
        /// <value>The position shift.</value>
        public virtual Vect2i PositionShift {
            get { return new Vect2i (); }
        }

        public Vect2i PositionRelativeFinal {
            get {
                return Relocation == null ? PositionRelative : Relocation.Destination;
            }
        }

        ElementRelocator Relocation {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this element has been moved by a player during its lifetime.
        /// </summary>
        public bool WasMoved { get; protected set; }

        public BackgroundCollection Backgrounds {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the tinting which is to be applied to this element.
        /// </summary>
        /// <value>The child tinting.</value>
        public Tinting Tinting {
            get;
            set;
        }

        /// <summary>
        /// Size of the widget.
        /// </summary>
        public virtual Vect2i Size {
            get {
                return _size;
            }
            set {
                if (_size != value) {
                    _size = value;
                    OnResized ();
                }
            }
        }

        /// <summary>
        /// Indicates whether this element is dead / was removed.
        /// </summary>
        /// <value><c>true</c> if this instance is dead; otherwise, <c>false</c>.</value>
        public bool IsDead {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this element has been initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        bool IsInitialized {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the element has been generated. If false or set to false, Regenerate() will be called.
        /// </summary>
        /// <value><c>true</c> if this instance is generated; otherwise, <c>false</c>.</value>
        protected bool IsGenerated {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the element reacts to mouse movement by default.
        /// </summary>
        protected virtual bool IsSensitive {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the current state of this element.
        /// </summary>
        /// <value>The state.</value>
        public ElementState State {
            get;
            protected set;
        }

        /// <summary>
        /// Indicates whether elements were added or removed from the child list.
        /// </summary>
        /// <value><c>true</c> if child list changed; otherwise, <c>false</c>.</value>
        protected bool ChildListChanged {
            get;
            private set;
        }

        public ElementState[] BackgroundStates {
            get;
            set;
        }

        #endregion

        #region Events

        /// <summary>
        /// Fired when the gui element is (newly) hovered over.
        /// </summary>
        public event GuiElementEventHandler Hovered;

        protected void OnHovered () {
            if (Hovered != null) {
                Hovered (this, new EventArgs ());
            }
        }

        protected void OnUnhovered () {
        }

        /// <summary>
        /// Raised when the element is resized.
        /// </summary>
        protected virtual void OnResized () {
            IsGenerated = false;
            foreach (Widget widget in Children) {
                widget.OnResized ();
            }
        }

        /// <summary>
        /// Raised if the child list changed.
        /// </summary>
        public virtual void OnChildListChanged () {
        }

        #endregion

        #region Fields

        GuiElement _parent;
        Vect2i _size;
        List<Widget> _children = new List<Widget> ();

        #endregion

        #region Constructor

        protected GuiElement (Vect2i position, Vect2i size)
            : this () {
            PositionRelative = position;
            _size = size;
        }

        protected GuiElement () {
            Tinting = NoTinting.INSTANCE;
        }

        #endregion

        #region Updating

        /// <summary>
        /// Regenerate this gui element from the ground up. Discards all child widgets and marks them as dead.
        /// </summary>
        protected virtual void Regenerate () {
            if (IsInitialized) {
                ClearWidgets ();
            }
        }

        /// <summary>
        /// Called every frame the gui is rendered.
        /// </summary>
        public virtual void Update () {
            ValidateElement ();

            if (ChildListChanged) {
                OnChildListChanged ();
                ChildListChanged = false;
            }

            // Move if needed.
            if (Relocation != null) {
                if (Relocation.Move (this)) {
                    Relocation = null;
                }
            }
        }

        public void ValidateElement () {
            if (!IsGenerated || !IsInitialized) {
                Regenerate ();
                IsGenerated = true;
                IsInitialized = true;
            }
        }

        public void Move (Vect2i delta) {
            WasMoved = true;
            PositionRelative = PositionRelative + delta;
        }

        public void Relocate (Vect2i destination, Easing easing) {
            Relocation = new ElementRelocator (this, destination, easing);
        }

        public void RelocateY (int yCoord, Easing easing) {
            if (Relocation == null) {
                Relocation = new ElementRelocator (this, new Vect2i (0, yCoord), easing);
            } else {
                Relocation = new ElementRelocator (this, new Vect2i (Relocation.Destination.X, yCoord), easing);
            }
        }

        /// <summary>
        /// Verifies that this element is hovered over and also calls this function on all child widgets.
        /// </summary>
        /// <param name="coordinates"></param>
        public virtual void VerifyInteraction (Vect2i coordinates, bool buttonDown, bool buttonUp) {

            if (IsSensitive && IntersectsWith (coordinates)) {

                if (!State.HasFlag (ElementState.Hovered)) {
                    OnHovered ();
                }

                FlagState (ElementState.Hovered);

                if (buttonDown) {
                    FlagState (ElementState.Pressed);
                } else if (buttonUp) {
                    UnflagState (ElementState.Pressed);
                }

            } else {

                if (State.HasFlag (ElementState.Hovered)) {
                    OnUnhovered ();
                }

                UnflagState (ElementState.Hovered);
                if (buttonDown || buttonUp) {
                    UnflagState (ElementState.Pressed);
                }

            }

            for (int i = 0; i < Children.Count; i++) {
                Children [i].VerifyInteraction (coordinates, buttonDown, buttonUp);
            }

        }

        #endregion

        #region Drawing

        public abstract void Draw (RenderTarget target, RenderStates states);

        protected void DrawBackground (RenderTarget target, RenderStates states) {
            if (Backgrounds != null) {
                Backgrounds.Render (this, target, states);
                if (BackgroundStates != null) {
                    Backgrounds.Render (this, target, states, BackgroundStates);
                }
            }
        }

        protected void DrawShadow (RenderTarget target, RenderStates states) {
            UIProvider.Backgrounds ["guishadow"].Render (new Vect2i (8, 8), Size, target, states);
        }

        /// <summary>
        /// Draws child widgets of this widget.
        /// </summary>
        /// <param name="renderTarget"></param>
        /// <param name="states"></param>
        protected virtual void DrawChildren (RenderTarget target, RenderStates states) {

            if (Children.Count <= 0) {
                return;
            }

            states.Transform.Translate (PositionShift);
            for (int i = 0; i < Children.Count; i++) {
                if (!Children [i].IsDisplayed) {
                    continue;
                }
                Children [i].ValidateElement ();
                Children [i].Draw (target, states);
            }
        }

        #endregion

        #region Child managment

        /// <summary>
        /// Adds a child widget.
        /// </summary>
        /// <param name="widget"></param>
        public virtual void AddWidget (Widget widget) {
            Children.Add (widget);
            widget.OnAddition (this);
            ChildListChanged = true;
        }

        /// <summary>
        /// Removes a widget from the list of widgets contained in this gui.
        /// </summary>
        /// <param name="widget"></param>
        protected void RemoveWidget (Widget widget) {
            if (widget == null) {
                return;
            }
            widget.IsDead = true;
            Children.Remove (widget);
            ChildListChanged = true;
        }

        /// <summary>
        /// Clears the list of child widgets.
        /// </summary>
        public void ClearWidgets () {
            List<Widget> deceased = Children;
            for (int i = 0; i < deceased.Count; i++) {
                RemoveWidget (deceased [i]);
            }
            Children.Clear (); // Just because.
        }

        public T GetChild<T> (string key) where T : Widget {
            foreach (Widget widget in Children) {
                if (widget.Key.Equals (key))
                    return (T)widget;
            }

            return default(T);
        }

        #endregion

        public void SetState (ElementState state, bool flag) {
            if (flag) {
                FlagState (state);
            } else {
                UnflagState (state);
            }
        }

        protected void FlagState (ElementState state) {
            State |= state;
        }

        protected void UnflagState (ElementState state) {
            State &= ~state;
        }

        public bool SustainsTooltip (Vect2i coordinates) {
            return !IsDead && IntersectsWith (coordinates);
        }

        public virtual bool IntersectsWith (Vect2i coordinates) {
            if (Utils.IntersectsWith (coordinates.X, coordinates.Y, PositionAbsolute.X, PositionAbsolute.Y, Size.X, Size.Y)) {
                return true;
            }

            for (int i = 0; i < Children.Count; i++) {
                if (Children [i].IntersectsWith (coordinates)) {
                    return true;
                }
            }

            return false;
        }
    }
}
