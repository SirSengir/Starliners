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

namespace BLibrary.Graphics {

    public sealed class View {

        #region Properties

        public Vect2f TopLeft {
            get {
                return _center - _size / 2;
            }
        }

        /// <summary>
        /// Center of the view
        /// </summary>
        public Vect2f Center {
            get { return _center; }
            set {
                _center = value;
                FlagUpdate ();
            }
        }

        /// <summary>
        /// Half-size of the view
        /// </summary>
        public Vect2f Size {
            get { return _size; }
            set {
                _size = value;
                FlagUpdate ();
            }
        }

        public float Rotation {
            get { return _rotation; }
            set {
                _rotation = value % 360f;
                if (_rotation < 0) {
                    _rotation += 360;
                }
                FlagUpdate ();
            }
        }

        /// <summary>
        /// The viewport in relation to the window.
        /// </summary>
        public Rect2f Port {
            get { return _port; }
            set {
                _port = value;
            }
        }

        public Transform Transform {
            get {
                // Recompute the transform if needed
                if (_transformChanged) {
                    // Rotation
                    float angle = _rotation * 3.141592654f / 180f;
                    float cosine = (float)Math.Cos (angle);
                    float sine = (float)Math.Sin (angle);
                    float tx = -_center.X * cosine - _center.Y * sine + _center.X;
                    float ty = _center.X * sine - _center.Y * cosine + _center.Y;

                    // Projection
                    float a = 2f / _size.X;
                    float b = -2f / _size.Y;
                    float c = -a * _center.X;
                    float d = -b * _center.Y;

                    _transform = new Transform (a * cosine, a * sine, a * tx + c,
                        -b * sine, b * cosine, b * ty + d,
                        0f, 0f, 1f);
                    _transformChanged = false;
                }

                return _transform;
            }
        }

        public Transform Inverse {
            get {
                if (_inverseChanged) {
                    _inverse = Transform.GetInverse ();
                    _inverseChanged = false;
                }
                return _inverse;
            }
        }

        #endregion

        Rect2f _port = new Rect2f (0, 0, 1, 1);

        Vect2f _center;
        Vect2f _size;
        float _rotation = 0;

        Transform _transform;
        Transform _inverse;

        bool _transformChanged = true;
        bool _inverseChanged = true;

        #region Constructors

        /// <summary>
        /// Construct a view with 1024x1024 dimensions.
        /// </summary>
        public View () {
            Reset (new Rect2f (-256, -256, 1024, 1024));
        }

        /// <summary>
        /// Construct the view from a rectangle
        /// </summary>
        /// <param name="viewRect">Rectangle defining the position and size of the view</param>
        public View (Rect2f viewRect) {
            Reset (viewRect);
        }

        /// <summary>
        /// Construct the view from its center and size
        /// </summary>
        /// <param name="center">Center of the view</param>
        /// <param name="size">Size of the view</param>
        public View (Vect2f center, Vect2i size) {
            Reset (center, size);
        }

        /// <summary>
        /// Construct the view from another view
        /// </summary>
        /// <param name="copy">View to copy</param>
        public View (View copy) {
            Reset (copy.Center, copy.Size);
            Rotation = copy.Rotation;
            Port = copy.Port;
        }

        #endregion

        void FlagUpdate () {
            _transformChanged = true;
            _inverseChanged = true;
        }

        /// <summary>
        /// Rebuild the view from a rectangle
        /// </summary>
        /// <param name="rectangle">Rectangle defining the position and size of the view</param>
        public void Reset (Rect2f rectangle) {
            _center = new Vect2f (
                rectangle.Coordinates.X + rectangle.Size.X / 2f,
                rectangle.Coordinates.Y + rectangle.Size.Y / 2f);
            _size = rectangle.Size;
            _rotation = 0;

            FlagUpdate ();
        }

        public void Reset (Vect2f center, Vect2f size) {
            _center = center;
            _size = size;
            _rotation = 0;

            FlagUpdate ();
        }

        /// <summary>
        /// Move the view
        /// </summary>
        /// <param name="offset">Offset to move the view</param>
        public void Move (Vect2f offset) {
            Center += offset;
        }

        /// <summary>
        /// Rotate the view
        /// </summary>
        /// <param name="angle">Angle of rotation, in degrees</param>
        public void Rotate (float angle) {
            Rotation += angle;
        }

        /// <summary>
        /// Resize the view rectangle to simulate a zoom / unzoom effect
        /// </summary>
        /// <param name="factor">Zoom factor to apply, relative to the current zoom</param>
        public void Zoom (float factor) {
            Size *= factor;
        }

        /// <summary>
        /// Provide a string describing the object
        /// </summary>
        /// <returns>String description of the object</returns>
        public override string ToString () {
            return "[View]" +
            " Center(" + Center + ")" +
            " Size(" + Size + ")" +
            " Rotation(" + Rotation + ")" +
            " Port(" + Port + ")";
        }

    }
}
