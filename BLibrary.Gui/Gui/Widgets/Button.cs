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
using BLibrary.Graphics;
using BLibrary.Util;
using BLibrary.Audio;
using Starliners;

namespace BLibrary.Gui.Widgets {

    public sealed class Button : Widget {
        #region Fields

        Widget _label;

        #endregion

        #region Constructor

        public Button (Vect2i position, Vect2i size, string key, object label, params object[] actionargs)
            : this (position, size, label) {
            ActionOnClick = new ClickPlain (key, actionargs) { Sound = SoundKeys.CLICK };
        }

        public Button (Vect2i position, Vect2i size, object label)
            : this (position, size, new Label (Vect2i.ZERO, size, label) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            }) {
        }

        public Button (Vect2i position, Vect2i size, string key, Widget label, params object[] actionargs)
            : this (position, size, label) {
            ActionOnClick = new ClickPlain (key, actionargs) { Sound = SoundKeys.CLICK };
        }

        public Button (Vect2i position, Vect2i size, Widget label)
            : base (position, size) {

            IsSensitive = true;
            Backgrounds = UIProvider.Style.ButtonStyle.CreateBackgrounds ();
            BackgroundStates = BG_STATES_SENSITIVE;

            _label = label;
        }

        #endregion

        public override void Draw (RenderTarget target, RenderStates states) {
            states.Transform.Translate (PositionRelative);
            DrawBackground (target, states);
            DrawChildren (target, states);
        }

        public void ReplaceLabel (string label) {
            ReplaceLabel (new Label (new Vect2i (), Size, label) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            });
        }

        public void ReplaceLabel (Widget label) {
            _label = label;
            IsGenerated = false;
        }

        protected override void Regenerate () {
            base.Regenerate ();

            _label.PositionRelative = (Vect2i)((Size - _label.Size) / 2);
            AddWidget (_label);
        }

        /*
        public override bool HandleMouseRelease (Vect2i coordinates, MouseButton button) {
            if ( && IntersectsWith (coordinates)) {
                SoundManager.Instance.Play (SoundKeys.CLICK);
                Window.DoAction (Key, _actionargs);
                return true;
            }

            return false;
        }
        */
    }
}
