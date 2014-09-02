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
using BLibrary.Gui;
using OpenTK.Input;
using BLibrary.Util;
using BLibrary.Graphics.Sprites;
using Starliners.States;
using Starliners.Game;
using System.Linq;

namespace Starliners.Map {
    sealed class MapInteractive : MapRenderable, ITooltipController {
        #region Constructor

        public MapInteractive (WorldInterface simulator)
            : base (simulator) {
        }

        #endregion

        protected override void OnMapMoved () {
            base.OnMapMoved ();
            GuiManager.Instance.HandleMapMoved ();
            VerifyHovering (GuiManager.Instance.MouseScreenPosition);
        }

        public void HandleKeyPress (Key key) {
            if (key == Key.PageDown) {
                ZoomOut ();
            } else if (key == Key.PageUp) {
                ZoomIn ();
            } else if (key == Key.Home) {
                ZoomReset ();
            }
        }

        /// <summary>
        /// Handles a mouse click on the map.
        /// </summary>
        /// <param name="screenX"></param>
        /// <param name="screenY"></param>
        /// <param name="button"></param>
        public void OnMouseClick (int screenX, int screenY, MouseButton button) {

            Vect2f coordsPlain = Window.MapPixelToCoords (new Vect2i (screenX, screenY), View);
            Vect2f coords = coordsPlain / SpriteManager.TILE_DIMENSION;

            if (GameAccess.Interface.ThePlayer.HeldObject != null && GameAccess.Interface.ThePlayer.HeldObject.SuppressEntityClick) {
                MapState.Instance.Controller.ActivatedMap (coords, GuiManager.Instance.CombineControlState (button));
            } else if (!GameAccess.Interface.KeyboardState.HasFlag (ControlState.Shift)) {
                Entity clicked = GetClickedEntity (coords);
                if (clicked != null) {
                    MapState.Instance.Controller.ActivatedEntity (clicked, GuiManager.Instance.CombineControlState (button));
                    return;
                }
            }

        }

        /// <summary>
        /// Handles release of a mouse button.
        /// </summary>
        /// <param name="screenX"></param>
        /// <param name="screenY"></param>
        /// <param name="button"></param>
        public void HandleMouseRelease (int screenX, int screenY, MouseButton button) {

            Vect2f coordsPlain = Window.MapPixelToCoords (new Vect2i (screenX, screenY), View);
            Vect2f coords = coordsPlain / SpriteManager.TILE_DIMENSION;

            if (GameAccess.Interface.ThePlayer.HeldObject != null && GameAccess.Interface.ThePlayer.HeldObject.SuppressEntityClick) {
                MapState.Instance.Controller.ClickedMap (coords, GuiManager.Instance.CombineControlState (button));
            } else if (!GameAccess.Interface.KeyboardState.HasFlag (ControlState.Shift)) {
                Entity clicked = GetClickedEntity (coords);
                if (clicked != null) {
                    MapState.Instance.Controller.ClickedEntity (clicked, GuiManager.Instance.CombineControlState (button));
                    return;
                }
            }

        }

        Entity GetClickedEntity (Vect2f coords) {
            Entity clicked = null;
            foreach (Entity entity in VisibleEntities.Where(p => p.Bounding.IntersectsWith(coords)).OrderByDescending(p => p.UILayer).ThenBy(p => MathUtils.GetDistanceBetween(p.Center, coords))) {
                if (clicked == null) {
                    clicked = entity;
                    break;
                } else if (GameAccess.Interface.ThePlayer.SelectedEntity != null &&
                           GameAccess.Interface.ThePlayer.SelectedEntity.Serial == clicked.Serial) {
                    clicked = entity;
                    break;
                }
            }

            return clicked;
        }

        /// <summary>
        /// Handles the mouse leaving the screen.
        /// </summary>
        public void HandleMouseLeft () {
            HoveredEntity = null;
        }

        /// <summary>
        /// Handles mouse movement.
        /// </summary>
        /// <param name="screenX"></param>
        /// <param name="screenY"></param>
        public void HandleMouseMove (int screenX, int screenY) {
            VerifyHovering (new Vect2i (screenX, screenY));
        }

        Rect2f _tooltipArea;

        public bool SustainsTooltip (Vect2i coordinates) {
            return _tooltipArea.IntersectsWith (Window.MapPixelToCoords (coordinates, View) / SpriteManager.TILE_DIMENSION);
        }

        public void HandleMouseWheel (int screenX, int screenY, int delta) {
            if (delta > 1) {
                ZoomIn ();
            } else if (delta < -1) {
                ZoomOut ();
            }
        }

        void VerifyHovering (Vect2i screen) {
            Vect2f coords = Window.MapPixelToCoords (screen, View) / SpriteManager.TILE_DIMENSION;

            // Handle entity hovers.
            Entity previouslyHovered = HoveredEntity;
            HoveredEntity = null;
            foreach (Entity entity in VisibleEntities.Where(p => p.Bounding.IntersectsWith(coords)).OrderByDescending(p => p.UILayer).ThenBy(p => MathUtils.GetDistanceBetween(p.Center, coords))) {
                HoveredEntity = entity;
                break;
            }
            if (HoveredEntity != null && HoveredEntity != previouslyHovered) {
                MapState.Instance.Controller.PulseEntity (HoveredEntity);
            }

            // Create tooltip from map if there isn't any yet.
            if (GuiManager.Instance.Tooltip == null) {
                if (HoveredEntity != null) {
                    _tooltipArea = HoveredEntity.Bounding;
                    Tooltip tooltip = (Tooltip)GameAccess.Interface.GetTooltip (HoveredEntity.TooltipId, HoveredEntity);
                    if (tooltip != null) {
                        tooltip.Controller = this;
                        GuiManager.Instance.Tooltip = tooltip;
                    }
                }
            }
        }
    }
}

