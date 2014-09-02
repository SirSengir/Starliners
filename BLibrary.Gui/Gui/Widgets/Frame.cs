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


namespace BLibrary.Gui.Widgets {

    public abstract class Frame : Widget {
        public Alignment AlignmentH {
            get;
            set;
        }

        public Alignment AlignmentV {
            get;
            set;
        }

        public override Vect2i PositionShift {
            get {
                return _adjust;
            }
        }

        #region Fields

        Vect2i _adjust;

        #endregion

        #region Constructor

        public Frame (Vect2i position, Vect2i size, string key)
            : base (position, size, key) {
        }

        #endregion

        public override void OnChildListChanged () {
            base.OnChildListChanged ();
            ResetChildPositions ();
        }

        protected override void OnResized () {
            base.OnResized ();
            ResetChildPositions ();
        }

        void ResetChildPositions () {

            if (AlignmentH == Alignment.None && AlignmentV == Alignment.None) {
                return;
            }

            Vect2i minCoords = new Vect2i ();
            Vect2i maxCoords = new Vect2i ();

            foreach (Widget child in Children) {
                if (child.PositionRelative.X < minCoords.X && child.PositionRelative.X > 0) {
                    minCoords = new Vect2i (child.PositionRelative.X, minCoords.Y);
                }
                if (child.PositionRelative.Y < minCoords.Y && child.PositionRelative.Y > 0) {
                    minCoords = new Vect2i (minCoords.X, child.PositionRelative.Y);
                }
                if (child.PositionRelative.X + child.Size.X > maxCoords.X) {
                    maxCoords = new Vect2i (child.PositionRelative.X + child.Size.X, maxCoords.Y);
                }
                if (child.PositionRelative.Y + child.Size.Y > maxCoords.Y) {
                    maxCoords = new Vect2i (maxCoords.X, child.PositionRelative.Y + child.Size.Y);
                }
            }

            Vect2i covered = maxCoords - minCoords;
            Vect2i spacing = Size - covered;
            Vect2i adjust = new Vect2i (AdjustHAlign (spacing), AdjustVAlign (spacing));

            _adjust = adjust;
        }

        int AdjustHAlign (Vect2i spacing) {
            if (AlignmentH == Alignment.Center) {
                return spacing.X / 2;
            }
            if (AlignmentH == Alignment.Right) {
                return spacing.X;
            }
            return 0;
        }

        int AdjustVAlign (Vect2i spacing) {
            if (AlignmentV == Alignment.Center) {
                return spacing.Y / 2;
            }
            if (AlignmentV == Alignment.Bottom) {
                return spacing.Y;
            }
            return 0;
        }
    }
}
