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

using OpenTK.Input;
using BLibrary.Resources;
using BLibrary.Util;
using BLibrary.Gui.Tooltips;
using Starliners;

namespace BLibrary.Gui.Widgets {

    public sealed class Header : Widget {
        static readonly Vect2i PADDING = new Vect2i (8, 8);

        #region Fields

        Label _label;
        WindowButton _buttons;
        object _template;

        #endregion

        public Header (Vect2i size, WindowButton buttons, object template)
            : base (new Vect2i (), size, "header") {

            Tinting = UIProvider.Style.HeaderStyle.Tinting;
            Backgrounds = UIProvider.Style.HeaderStyle.CreateBackgrounds ();

            _buttons = buttons;
            _template = template;
        }

        protected override void Regenerate () {
            base.Regenerate ();
            if (_template != null) {
                _label = new Label (
                    new Vect2i (),
                    new Vect2i (Size.X, Size.Y),
                    _template) {
                    Style = FontManager.Instance [FontManager.HEADER],
                    AlignmentH = Alignment.Center,
                    AlignmentV = Alignment.Center
                };
                AddWidget (_label);
            }

            int btns = 0;
            if (_buttons.HasFlag (WindowButton.Close)) {
                btns++;
                AddWidget (new Button (new Vect2i (Size.X - (btns * 24) - PADDING.X, PADDING.Y), new Vect2i (24, 24), Constants.CONTAINER_KEY_BTN_CLOSE_WINDOW, "X") {
                    FixedTooltip = new TooltipSimple (Localization.Instance ["tt_close_help"], new string[0])
                });
            }
            if (_buttons.HasFlag (WindowButton.Compact)) {
                btns++;
                AddWidget (new Button (new Vect2i (Size.X - (btns * 24) - PADDING.X, PADDING.Y), new Vect2i (24, 24), Constants.CONTAINER_KEY_BTN_COMPACT, "_") {
                    FixedTooltip = new TooltipSimple (Localization.Instance ["tt_compact_help"], Localization.Instance ["tt_compact_help_desc"])
                });
            }
            if (_buttons.HasFlag (WindowButton.Position)) {
                btns++;
                AddWidget (new Button (new Vect2i (Size.X - (btns * 24) - PADDING.X, PADDING.Y), new Vect2i (24, 24), Constants.CONTAINER_KEY_BTN_POSITION, "P") {
                    FixedTooltip = new TooltipSimple (Localization.Instance ["tt_position_help"], Localization.Instance ["tt_position_help_desc"])
                });
            }

        }

        public override void Update () {
            base.Update ();
            Backgrounds.SetActive (Window.BeingDragged ? ElementState.Dragged : ElementState.None);
        }

        public override bool HandleMouseClick (Vect2i coordinates, MouseButton button) {
            VerifyInteraction (coordinates, true, false);

            if (!IntersectsWith (coordinates)) {
                return false;
            }

            if (button == MouseButton.Right && Window is GuiWindow) {
                ((GuiWindow)Window).IsCompacted = true;
                return true;
            }

            bool wasChildClick = false;
            foreach (Widget child in Children) {
                if (child != _label && child.IntersectsWith (coordinates))
                    wasChildClick = true;
                if (child.HandleMouseClick (coordinates, button))
                    return true;
            }

            if (!wasChildClick && ((IDraggable)Window).IsDraggable) {
                GuiManager.Instance.SetDraggedElement ((IDraggable)Window);
                return true;
            } else {
                return false;
            }
        }
    }
}

