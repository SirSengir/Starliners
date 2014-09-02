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

﻿using BLibrary.Util;
using BLibrary.Graphics;
using Starliners.Game;

namespace Starliners.Map {

    /// <summary>
    /// Represents the area of the map which is currently drawn to the screen.
    /// </summary>
    sealed class DrawnArea {
        #region Properties

        public bool ChunksChanged { get; private set; }

        public Vect2i TileMin { get; private set; }

        public Vect2i TileMax { get; private set; }

        public Vect2f CoordsMin { get; private set; }

        public Vect2f CoordsMax { get; private set; }

        //public Vect2i[] Chunks { get; private set; }

        /// <summary>
        /// Gets the center of the currently drawn area.
        /// </summary>
        /// <value>The center.</value>
        public Vect2f Center {
            get {
                return (CoordsMax - CoordsMin) / 2;
            }
        }

        #endregion

        //WorldInterface _simulator;

        public DrawnArea (WorldInterface simulator) {
            ChunksChanged = true;
            //_simulator = simulator;
            //Chunks = new Vect2i[0];
        }

        /// <summary>
        /// Updates the viewed area and checks whether the viewed chunks have changed.
        /// </summary>
        /// <returns><c>true</c>, if chunk change was checked, <c>false</c> otherwise.</returns>
        /// <param name="target">Target.</param>
        /// <param name="view">View.</param>
        public void CheckChunkChange (RenderTarget target, View view) {
            /*
            ChunksChanged = false;

            CoordsMin = MapRenderable.CalculateMinCoords (target, view);
            CoordsMax = MapRenderable.CalculateMaxCoords (target, target.View);

            float minTileX = CoordsMin.X > 0 ? CoordsMin.X : 0;
            float minTileY = CoordsMin.Y > 0 ? CoordsMin.Y : 0;
            float maxTileX = CoordsMax.X + 1;
            float maxTileY = CoordsMax.Y + 1;
            TileMin = new Vect2i (minTileX, minTileY);
            TileMax = new Vect2i (maxTileX, maxTileY);

            Vect2i[] chunks = _simulator.Access.Map.GetChunksWithin (new Rect2i (TileMin, TileMax - TileMin));
            if (Chunks == null || Chunks.Length != chunks.Length) {
                ChunksChanged = true;
            } else {
                for (int i = 0; i < Chunks.Length; i++) {

                    bool matched = false;
                    for (int j = 0; j < chunks.Length; j++) {
                        if (Chunks [i] == chunks [j]) {
                            matched = true;
                            break;
                        }
                    }

                    if (!matched) {
                        ChunksChanged = true;
                        break;
                    }
                }
            }

            Chunks = chunks;
            */
        }

        /// <summary>
        /// Determines whether the given location is within the area to be rendered.
        /// </summary>
        /// <returns><c>true</c> if this instance is within rendered area the specified location; otherwise, <c>false</c>.</returns>
        /// <param name="location">Location.</param>
        public bool IsWithinRenderedArea (Vect2d location) {
            return true;//Utils.IsWithin (location, CoordsMin.X, CoordsMax.X, CoordsMin.Y, CoordsMax.Y);
        }

        /// <summary>
        /// Determines whether the given location is within the currently rendered chunks.
        /// </summary>
        /// <returns><c>true</c> if this instance is within rendered chunks the specified location; otherwise, <c>false</c>.</returns>
        /// <param name="location">Location.</param>
        public bool IsWithinRenderedChunks (Vect2d location) {
            /*
            for (int i = 0; i < Chunks.Length; i++) {
                if (Utils.IsWithin (location, new Rect2f (Chunks [i] * WorldData.CHUNK_SIZE, WorldData.CHUNK_SIZE))) {
                    return true;
                }
            }
            return false;
            */
            return true;
        }
    }
}

