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
using BLibrary.Graphics;

namespace BLibrary.Gui.Backgrounds {

    public sealed class BackgroundCollection {

        public Background Active {
            get { return _active; }
        }

        #region Fields

        Background _active;
        Dictionary<ElementState, Background> _backgrounds;

        bool _hardened = false;

        #endregion

        #region Constructors

        public BackgroundCollection (Background background) {
            _backgrounds = new Dictionary<ElementState, Background> () { { ElementState.None, background } };
            SetActive (ElementState.None);
            Harden ();
        }

        public BackgroundCollection () {
            _backgrounds = new Dictionary<ElementState, Background> ();
        }

        #endregion

        public void AddBackground (ElementState state, Background background) {
            if (_hardened) {
                throw new InvalidOperationException ("Attempted to add a new background to an already hardened background collection.");
            }
            _backgrounds [state] = background;
            if (_backgrounds.Count == 1) {
                SetActive (state);
            }
        }

        public void Harden () {
            _hardened = true;
        }

        public void SetActive (ElementState state) {
            _active = _backgrounds [state];
        }

        public void Render (GuiElement element, RenderTarget target, RenderStates states) {
            _active.Render (element.Size, target, states, element);
        }

        public void Render (GuiElement element, RenderTarget target, RenderStates states, params ElementState[] todraw) {
            for (int i = 0; i < todraw.Length; i++) {
                if (element.State.HasFlag (todraw [i])) {
                    _backgrounds [todraw [i]].Render (element.Size, target, states, element);
                    return;
                }
            }
        }

    }
}

