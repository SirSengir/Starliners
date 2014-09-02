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

﻿using System.Collections.Generic;
using BLibrary.Graphics;
using BLibrary.Graphics.Text;
using BLibrary.Graphics.Sprites;
using BLibrary.Util;
using BLibrary.Gui.Widgets;
using Starliners.Game;
using Starliners;
using Starliners.States;

namespace BLibrary.Gui.Tooltips {

    public class TooltipEntity : Tooltip {

        #region Constants

        static readonly Vect2i ICON_SIZE = new Vect2i (16, 16);
        static readonly Vect2i BAR_SIZE = new Vect2i (8, 72);

        #endregion

        protected virtual Vect2i InnerCorner {
            get { return CornerTopLeft; }
        }

        protected virtual Vect2i InnerSize {
            get {
                int width = (_buffer != null ? (int)_buffer.LocalBounds.Width : 0) + _entity.ProgressInfos.Count * 2 * BAR_SIZE.X + UIProvider.MarginSmall.X;
                int minheight = _entity.ProgressInfos.Count > 0 ? BAR_SIZE.Y + ICON_SIZE.Y + ICON_SIZE.Y / 2 : _buffer != null ? (int)_buffer.LocalBounds.Height : 0;
                int height = _buffer != null && _buffer.LocalBounds.Height > minheight ? (int)_buffer.LocalBounds.Height : minheight;
                return new Vect2i (width, height);
            }
        }

        #region Fields

        Entity _entity;
        TextBuffer _buffer;
        long _lastUpdate;

        #endregion

        public TooltipEntity (Entity entity)
            : base (new Vect2i (256, 64)) {

            _entity = entity;
            AddHeader (_entity.Description, 0);

            IList<string> info = CombineWithSeperation (_entity.GetInformation (GameAccess.Interface.ThePlayer), _entity.GetUsage (GameAccess.Interface.ThePlayer));
            if (info.Count > 0) {
                _buffer = new TextBuffer (info);
                _buffer.SetMaxWidth (MAX_TOOLTIP_WIDTH);
            } else {
                HeaderOnly = true;
            }

        }

        protected override void Regenerate () {
            base.Regenerate ();
            for (int i = 0; i < _entity.ProgressInfos.Count; i++) {
                Vect2i start = CornerTopLeft + new Vect2i (InnerSize.X, 0) - new Vect2i ((i + 1) * BAR_SIZE.X * 2, 0);
                AddWidget (new Progress (start + new Vect2i ((ICON_SIZE.X - BAR_SIZE.X) / 2, 0), BAR_SIZE, _entity.ProgressInfos [i], "toolbarProgress"));
                AddWidget (new IconSymbol (start + new Vect2i (0, BAR_SIZE.Y + ICON_SIZE.Y / 2), ICON_SIZE, _entity.ProgressInfos [i].Icon));
            }
        }

        public override void Update () {
            base.Update ();
            if (GameAccess.Interface.Local.Clock.Ticks > _lastUpdate + 5) {
                MapState.Instance.Controller.PulseEntity (_entity);
                _lastUpdate = GameAccess.Interface.Local.Clock.Ticks;
            }
        }

        protected override void AdjustPosition () {
            Vect2i location = MapState.Instance.Map.MapCoordsToPixel ((_entity.Bounding.Coordinates + new Vect2f (_entity.BoundingSize.X, 0)) * SpriteManager.TILE_DIMENSION);
            Vect2i end = MapState.Instance.Map.MapCoordsToPixel ((_entity.Bounding.Coordinates + new Vect2f (0, _entity.BoundingSize.Y)) * SpriteManager.TILE_DIMENSION);
            int height = end.Y - location.Y;
            Vect2i size = DetermineDimensions ();

            int offsetY = ((height - size.Y) / 2);
            PositionRelative = new Vect2i (location.X, location.Y + offsetY);
        }

        protected override sealed Vect2i GetDimensions () {
            return InnerSize;
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);
            if (_buffer == null) {
                return;
            }
            states.Transform.Translate (PositionRelative + InnerCorner);
            _buffer.Draw (target, states);

        }

    }
}

