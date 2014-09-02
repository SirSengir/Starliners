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

    public abstract class Transformable {
        #region Properties

        public abstract Rect2f LocalBounds { get; }

        public Rect2f GlobalBounds {
            get {
                return Transform.TransformRect (LocalBounds);
            }
        }

        Vect2f _origin = new Vect2f (0, 0);

        public Vect2f Origin {
            get { return _origin; }
            set {
                _origin = value;
                FlagUpdate ();
            }
        }

        Vect2f _position = new Vect2f (0, 0);

        public Vect2f Position {
            get { return _position; }
            set {
                _position = value;
                FlagUpdate ();
            }
        }

        float _rotation = 0;

        public float Rotation {
            get { return _rotation; }
            set {
                _rotation = value % 360f;
                if (_rotation < 0)
                    _rotation += 360;
                FlagUpdate ();
            }
        }

        Vect2f _scale = new Vect2f (1, 1);

        public Vect2f Scale {
            get { return _scale; }
            set {
                _scale = value;
                FlagUpdate ();
            }
        }

        Transform _transform = Transform.Identity;
        bool _transformChanged = true;

        public Transform Transform {
            get {
                // Recompute the transform if needed
                if (_transformChanged) {
                    float angle = _rotation * 3.141592654f / 180f;
                    float cosine = (float)Math.Cos (angle);
                    float sine = (float)Math.Sin (angle);

                    float sxc = _scale.X * cosine;
                    float syc = _scale.Y * cosine;
                    float sxs = _scale.X * sine;
                    float sys = _scale.Y * sine;
                    float tx = -_origin.X * sxc - _origin.Y * sys + _position.X;
                    float ty = _origin.X * sxs - _origin.Y * syc + _position.Y;

                    _transform = new Transform (sxc, sys, tx,
                        -sxs, syc, ty,
                        0f, 0f, 1f);
                    _transformChanged = false;
                }
                return _transform;
            }
        }

        Transform _inverse = Transform.Identity.GetInverse ();
        bool _inverseChanged = true;

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

        void FlagUpdate () {
            _transformChanged = true;
            _inverseChanged = true;
        }

        /// <summary>
        /// Move the transformable
        /// </summary>
        /// <param name="offset">Offset to move the view</param>
        public void Move (Vect2f offset) {
            Position += offset;
        }

        /// <summary>
        /// Rotate the transformable
        /// </summary>
        /// <param name="angle">Angle of rotation, in degrees</param>
        public void Rotate (float angle) {
            Rotation += angle;
        }
    }
}
