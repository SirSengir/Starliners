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
using System.Linq;
using BLibrary.Util;
using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using Starliners.Game;
using Starliners.Graphics;
using BLibrary.Gui;


namespace Starliners.Map {

    /// <summary>
    /// Represents a map which is only for rendering and has no way to react to user input.
    /// </summary>
    class MapRenderable : IDisposable {
        /// <summary>
        /// The interface side mock world simulator.
        /// </summary>
        public WorldInterface Simulator {
            get;
            private set;
        }

        public MapControl Control {
            get;
            set;
        }

        /// <summary>
        /// Set to indicate that everything visible needs to be re-rendered.
        /// </summary>
        /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
        internal bool IsDirty {
            get;
            private set;
        }

        internal string DirtynessReason {
            get;
            private set;
        }

        #region Fields

        MapLayer[] _layers;
        List<Entity> _visibleEntities = new List<Entity> ();
        int _zoomLevel = 1;

        #endregion

        #region Constructor

        public MapRenderable (WorldInterface simulator) {
            Simulator = simulator;
        }

        #endregion

        #region Rendering

        public List<Entity> VisibleEntities {
            get { return _visibleEntities; }
        }

        /// <summary>
        /// Returns the world coordinates at the center of the currently visible map.
        /// </summary>
        public Vect2f CenterWorld {
            get {
                Vect2f coordsPlain = Window.MapPixelToCoords (new Vect2i ((int)View.Center.X, (int)View.Center.Y), View);
                return coordsPlain / SpriteManager.TILE_DIMENSION;
            }
        }

        public DrawnArea DrawnArea { get; private set; }

        public View View { get; private set; }

        protected RenderScene Window { get; private set; }

        public virtual void InitMap (RenderScene window, View view) {
            Window = window;
            View = view;
   
            _layers = new MapLayer[(Enum.GetValues (typeof(UILayer)).Length)];

            DrawnArea = new DrawnArea (Simulator);
            ZoomReset ();
        }

        public void AssembleLayers () {
            _layers [(byte)UILayer.Starfield] = new LayerStarfield (this, UILayer.Starfield);
            _layers [(byte)UILayer.Planets] = new LayerPlanets (this, UILayer.Planets);
            _layers [(byte)UILayer.Fleets] = new LayerFleets (this, UILayer.Fleets);
        }

        public virtual void Draw (RenderTarget target, RenderStates states) {

            // Validate map control
            if (Control != null && Control.IsUpdated) {
                CenterView (Control.Center);
                Control.IsUpdated = false;
                OnMapMoved ();
            }

            View previous = target.View;
            target.View = View;

            DrawnArea.CheckChunkChange (target, View);
            if (!IsDirty && DrawnArea.ChunksChanged) {
                DirtynessReason = "ChunksChanged";
            }
            IsDirty = IsDirty || DrawnArea.ChunksChanged;

            // Cache this, otherwise HandleMouseMove will slow the game to a crawl.
            Vect2f coordsMin = CalculateMinCoords (target, View) - new Vect2f (2, 2);
            Vect2f coordsMax = CalculateMaxCoords (target, View) + new Vect2f (2, 2);
            _visibleEntities = Simulator.Access.Entities.Values.Where (p => Utils.IsWithin (p.Location, coordsMin.X - 2, coordsMax.X + 2, coordsMin.Y - 2, coordsMax.Y + 2) && p.IsVisible).ToList ();

            // Draw the map layers.
            foreach (MapLayer layer in _layers) {
                if (layer != null) {
                    layer.Draw (target, states, View);

                    target.View = previous;
                    GuiManager.Instance.DrawMapLegend (target, states, layer.UILayer);
                    target.View = View;
                }
            }

            target.View = previous;

            // Mark the map as rendered.
            IsDirty = false;
            DirtynessReason = "NotDirty";
        }

        /// <summary>
        /// Marks the map as dirty.
        /// </summary>
        public void MarkDirty (Vect2d location, string reason) {
            if (DrawnArea.IsWithinRenderedChunks (location)) {
                IsDirty = true;
                DirtynessReason = reason;
            }
        }

        #endregion

        #region Zooming & Moving

        protected virtual void OnMapMoved () {
        }

        /// <summary>
        /// The current entity hovered over, null if none.
        /// </summary>
        /// <value>The hovered.</value>
        public Entity HoveredEntity {
            get;
            protected set;
        }

        protected void ZoomReset () {
            _zoomLevel = GameAccess.Game.DefaultZoom;
            SetZoom ();
        }

        protected void ZoomOut () {
            if (_zoomLevel >= GameAccess.Game.MaxZoom)
                return;

            _zoomLevel++;
            SetZoom ();
        }

        protected void ZoomIn () {
            if (_zoomLevel <= GameAccess.Game.MinZoom)
                return;

            _zoomLevel--;
            SetZoom ();
        }

        protected void SetZoom () {
            float yScale = View.Size.Y / View.Size.X;
            int xSize = (16 * 32) + _zoomLevel * (16 * 32);
            View.Size = new Vect2i (xSize, (int)(yScale * xSize));
            if (Control != null) {
                CenterView (Control.Center);
            }
            ValidateBounds ();
        }

        protected void MoveView (int deltaX, int deltaY) {
            View.Center = new Vect2i (View.Center.X + deltaX, View.Center.Y + deltaY);
            ValidateBounds ();
        }

        /// <summary>
        /// Centers the view on the given world coordinates.
        /// </summary>
        /// <param name="xCoord"></param>
        /// <param name="yCoord"></param>
        public void CenterView (Vect2f worldCoords) {
            Vect2i coords = Window.MapCoordsToPixel (new Vect2f (worldCoords.X, worldCoords.Y) * SpriteManager.TILE_DIMENSION, Window.View);
            View.Center = new Vect2f (coords.X, coords.Y);
            ValidateBounds ();
        }

        protected void ValidateBounds () {
            //int maxX = Simulator.Access.MaxX * SpriteManager.TILE_DIMENSION;
            //int maxY = Simulator.Access.MaxY * SpriteManager.TILE_DIMENSION;
            /*
            View.Center = new Vect2i (
                View.Center.X < View.Size.X / 2 ? View.Size.X / 2 : View.Center.X > maxX - View.Size.X / 2 ? maxX - View.Size.X / 2 : View.Center.X,
                View.Center.Y < View.Size.Y / 2 ? View.Size.Y / 2 : View.Center.Y > maxY - View.Size.Y / 2 ? maxY - View.Size.Y / 2 : View.Center.Y
            );
            */

            // Marks the map as dirty.
            //IsDirty = true;
        }

        public Vect2i MapCoordsToPixel (Vect2d coords) {
            return Window.MapCoordsToPixel (coords, View);
        }

        #endregion

        #region IDisposable

        public void Dispose () {
            foreach (MapLayer layer in _layers)
                if (layer != null)
                    layer.Dispose ();
        }

        #endregion

        #region Helper functions

        public static Vect2f CalculateMinCoords (RenderTarget target, View view) {
            return target.MapPixelToCoords (
                new Vect2i ((int)(target.Size.X * view.Port.Left), (int)(target.Size.Y * view.Port.Top)),
                view) / SpriteManager.TILE_DIMENSION;
        }

        public static Vect2f CalculateMaxCoords (RenderTarget target, View view) {
            return target.MapPixelToCoords (
                new Vect2i ((int)(target.Size.X * view.Port.Width), (int)(target.Size.Y * view.Port.Height)),
                view) / SpriteManager.TILE_DIMENSION;
        }

        #endregion
    }
}
