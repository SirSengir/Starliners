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
using BLibrary.Util;
using BLibrary.Graphics;
using Starliners.States;
using BLibrary.Graphics.Sprites;
using BLibrary.Gui.Data;
using Starliners.Graphics;

namespace BLibrary.Gui {
    public class LegendElement : WidgetContainer {

        public override Vect2i PositionAbsolute {
            get {
                Vect2i screen = MapState.Instance.Map.MapCoordsToPixel ((_controller.Location + CoordinateOffset) * SpriteManager.TILE_DIMENSION);

                switch (Anchored) {
                    case Anchor.TopCenter:
                        return screen - new Vect2i (Size.X / 2, 0);
                    case Anchor.Center:
                    default:
                        return screen;
                }
            }
        }

        public Anchor Anchored {
            get;
            set;
        }

        public UILayer MapLayer {
            get {
                return _controller.UILayer;
            }
        }

        /// <summary>
        /// Indicates the offset to apply to the controller's location when rendering on the map.
        /// </summary>
        /// <value>The coordinate offset.</value>
        public Vect2d CoordinateOffset {
            get;
            set;
        }

        #region Fields

        ILegendController _controller;

        #endregion

        public LegendElement (ILegendController controller, Vect2i size) {
            Anchored = Anchor.Center;
            _controller = controller;
            Size = size;
            Subscribe (controller);
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            states.Transform.Translate (PositionAbsolute);

            DrawBackground (target, states);
            DrawChildren (target, states);
        }

    }
}

