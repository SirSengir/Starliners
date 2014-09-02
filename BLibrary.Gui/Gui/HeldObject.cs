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

﻿using BLibrary.Graphics;
using BLibrary.Util;
using Starliners.Game;
using Starliners;
using Starliners.Graphics;
using Starliners.Game.Forces;
using BLibrary.Gui.Data;

namespace BLibrary.Gui {

    public class HeldObject : WidgetContainer, IDrawable {
        #region Properties

        public override Vect2i PositionAbsolute {
            get {
                return PositionRelative;
            }
        }

        public bool CloseOnRightClick {
            get {
                return GameAccess.Interface.ThePlayer != null && GameAccess.Interface.ThePlayer.HeldObject != null;
            }
        }

        public bool HideMouseCursor {
            get {
                return GameAccess.Interface.IsInGame && GameAccess.Interface.ThePlayer.HeldObject != null;
            }
        }

        #endregion

        public HeldObject () {
            Size = new Vect2i (54, 54);
        }

        public override void Update () {
            base.Update ();
            PositionRelative = GuiManager.Instance.MouseGuiPosition;
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            if (!GameAccess.Interface.IsInGame || GameAccess.Interface.ThePlayer == null) {
                return;
            }
            IHoldable holdable = GameAccess.Interface.ThePlayer.HeldObject;
            if (holdable == null) {
                return;
            }

            if (holdable is FleetRelocator) {
                states.Transform.Translate (PositionAbsolute);
                states.Transform.Scale (2, 2);
                RendererVessel.Instance.DrawRenderable (target, states, ((FleetRelocator)holdable).Fleet.Projector);
            }
        }
    }
}
