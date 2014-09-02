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
using System.Collections.Generic;
using BLibrary.Graphics;
using BLibrary.Util;
using BLibrary.Gui.Tooltips;
using BLibrary.Audio;
using Starliners;

namespace BLibrary.Gui.Widgets {

    public sealed class Switchable : Widget {
        public int Selected {
            get;
            private set;
        }

        object[] _values;
        Widget[] _labels;

        public object[] AdditionalArgs {
            get;
            set;
        }

        public object Value {
            get {
                return _values [Selected];
            }
            set {
                for (int i = 0; i < _values.Length; i++)
                    if (_values [i].Equals (value)) {
                        Selected = i;
                        return;
                    }

                Selected = 0;
            }
        }

        public string[] TooltipHeader {
            get;
            set;
        }

        public string[] TooltipInfo {
            get;
            set;
        }

        public Switchable (Vect2i position, Vect2i size, string key, object[] values, string[] labels)
            : this (position, size, key, values) {

            if (values.Length != labels.Length) {
                throw new ArgumentException ("Values and texts must match up.");
            }

            _labels = new Widget[labels.Length];
            for (int i = 0; i < labels.Length; i++) {
                _labels [i] = new Label (new Vect2i (), size, labels [i]) {
                    AlignmentH = Alignment.Center,
                    AlignmentV = Alignment.Center
                };
            }
        }

        public Switchable (Vect2i position, Vect2i size, string key, object[] values, Widget[] labels)
            : this (position, size, key, values) {

            if (values.Length != labels.Length) {
                throw new ArgumentException ("Values and texts must match up.");
            }

            _labels = labels;
        }

        Switchable (Vect2i position, Vect2i size, string key, object[] values)
            : base (position, size, key) {
            _values = values;

            Backgrounds = UIProvider.Style.SwitchableStyle.CreateBackgrounds ();
            BackgroundStates = BG_STATES_SENSITIVE;

            IsSensitive = true;
        }

        public override void Update () {
            base.Update ();
            for (int i = 0; i < _labels.Length; i++) {
                _labels [i].Update ();
            }
        }

        #region implemented abstract members of Widget

        public override void Draw (RenderTarget target, RenderStates states) {
            states.Transform.Translate (PositionRelative);
            DrawBackground (target, states);
            // Label
            _labels [Selected].Draw (target, states);
        }

        #endregion

        public override Tooltip GetTooltip (Vect2i coordinates) {
            return TooltipInfo != null ? new TooltipSwitchable (this) { Parent = this } : base.GetTooltip (coordinates);
        }

        public override bool HandleMouseClick (Vect2i coordinates, MouseButton button) {
            if (!State.HasFlag (ElementState.Disabled) && IntersectsWith (coordinates)) {
                if (button == MouseButton.Left)
                    Selected = Selected < _values.Length - 1 ? Selected + 1 : Selected = 0;
                else
                    Selected = Selected > 0 ? Selected - 1 : Selected = _values.Length - 1;

                SoundManager.Instance.Play (SoundKeys.CLICK);

                if (AdditionalArgs != null) {
                    List<object> list = new List<object> ();
                    list.Add (_values [Selected]);
                    list.AddRange (AdditionalArgs);
                    Window.DoAction (Key, list.ToArray ());
                } else {
                    Window.DoAction (Key, _values [Selected]);
                }
                return true;
            }

            return false;
        }
    }
}
