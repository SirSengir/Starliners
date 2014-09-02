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

    public struct Transform {
        static readonly Transform IDENTIY = new Transform (
                                                1, 0, 0,
                                                0, 1, 0,
                                                0, 0, 1);

        public static Transform Identity {
            get {
                return IDENTIY;
            }
        }

        double[] _matrix;

        public double[] Matrix {
            get { return _matrix; }
        }

        /// <summary>
        /// Construct a transform from a 3x3 matrix
        /// </summary>
        /// <param name="a00">Element (0, 0) of the matrix</param>
        /// <param name="a01">Element (0, 1) of the matrix</param>
        /// <param name="a02">Element (0, 2) of the matrix</param>
        /// <param name="a10">Element (1, 0) of the matrix</param>
        /// <param name="a11">Element (1, 1) of the matrix</param>
        /// <param name="a12">Element (1, 2) of the matrix</param>
        /// <param name="a20">Element (2, 0) of the matrix</param>
        /// <param name="a21">Element (2, 1) of the matrix</param>
        /// <param name="a22">Element (2, 2) of the matrix</param>
        public Transform (double a00, double a01, double a02,
                          double a10, double a11, double a12,
                          double a20, double a21, double a22) {
            _matrix = new double[16];
            SetMatrix (a00, a01, a02, a10, a11, a12, a20, a21, a22);
        }

        void SetMatrix (double a00, double a01, double a02,
                        double a10, double a11, double a12,
                        double a20, double a21, double a22) {
            _matrix [0] = a00;
            _matrix [4] = a01;
            _matrix [8] = 0.0f;
            _matrix [12] = a02;

            _matrix [1] = a10;
            _matrix [5] = a11;
            _matrix [9] = 0.0f;
            _matrix [13] = a12;

            _matrix [2] = 0.0f;
            _matrix [6] = 0.0f;
            _matrix [10] = 1.0f;
            _matrix [14] = 0.0f;

            _matrix [3] = a20;
            _matrix [7] = a21;
            _matrix [11] = 0.0f;
            _matrix [15] = a22;
        }

        /// <summary>
        /// Return the inverse of the transform.
        /// 
        /// If the inverse cannot be computed, an identity transform
        /// is returned.
        /// </summary>
        /// <returns>A new transform which is the inverse of self</returns>
        public Transform GetInverse () {

            // Compute the determinant
            double det = _matrix [0] * (_matrix [15] * _matrix [5] - _matrix [7] * _matrix [13]) -
                         _matrix [1] * (_matrix [15] * _matrix [4] - _matrix [7] * _matrix [12]) +
                         _matrix [3] * (_matrix [13] * _matrix [4] - _matrix [5] * _matrix [12]);

            // Compute the inverse if the determinant is not zero
            // (don't use an epsilon because the determinant may *really* be tiny)
            if (det != 0f) {
                return new Transform ((_matrix [15] * _matrix [5] - _matrix [7] * _matrix [13]) / det,
                    -(_matrix [15] * _matrix [4] - _matrix [7] * _matrix [12]) / det,
                    (_matrix [13] * _matrix [4] - _matrix [5] * _matrix [12]) / det,
                    -(_matrix [15] * _matrix [1] - _matrix [3] * _matrix [13]) / det,
                    (_matrix [15] * _matrix [0] - _matrix [3] * _matrix [12]) / det,
                    -(_matrix [13] * _matrix [0] - _matrix [1] * _matrix [12]) / det,
                    (_matrix [7] * _matrix [1] - _matrix [3] * _matrix [5]) / det,
                    -(_matrix [7] * _matrix [0] - _matrix [3] * _matrix [4]) / det,
                    (_matrix [5] * _matrix [0] - _matrix [1] * _matrix [4]) / det);
            } else {
                return Identity;
            }
        }

        /// <summary>
        /// Transform a 2D point.
        /// </summary>
        /// <param name="x">X coordinate of the point to transform</param>
        /// <param name="y">Y coordinate of the point to transform</param>
        /// <returns>Transformed point</returns>
        public Vect2d TransformPoint (double x, double y) {
            return new Vect2d (_matrix [0] * x + _matrix [4] * y + _matrix [12],
                _matrix [1] * x + _matrix [5] * y + _matrix [13]);
        }

        /// <summary>
        /// Transform a 2D point.
        /// </summary>
        /// <param name="point">Point to transform</param>
        /// <returns>Transformed point</returns>
        public Vect2d TransformPoint (Vect2d point) {
            return TransformPoint (point.X, point.Y);
        }

        /// <summary>
        /// Transform a rectangle.
        /// 
        /// Since SFML doesn't provide support for oriented rectangles,
        /// the result of this function is always an axis-aligned
        /// rectangle. Which means that if the transform contains a
        /// rotation, the bounding rectangle of the transformed rectangle
        /// is returned.
        /// </summary>
        /// <param name="rectangle">Rectangle to transform</param>
        /// <returns>Transformed rectangle</returns>
        public Rect2f TransformRect (Rect2f rectangle) {
            // Transform the 4 corners of the rectangle
            Vect2d[] points = new Vect2d[] {
                TransformPoint (rectangle.Coordinates.X, rectangle.Coordinates.Y),
                TransformPoint (rectangle.Coordinates.X, rectangle.Coordinates.Y + rectangle.Size.Y),
                TransformPoint (rectangle.Coordinates.X + rectangle.Size.X, rectangle.Coordinates.Y),
                TransformPoint (rectangle.Coordinates.X + rectangle.Size.X, rectangle.Coordinates.Y + rectangle.Size.Y)
            };

            // Compute the bounding rectangle of the transformed points
            double left = points [0].X;
            double top = points [0].Y;
            double right = points [0].X;
            double bottom = points [0].Y;
            for (int i = 1; i < 4; ++i) {
                if (points [i].X < left)
                    left = points [i].X;
                else if (points [i].X > right)
                    right = points [i].X;
                if (points [i].Y < top)
                    top = points [i].Y;
                else if (points [i].Y > bottom)
                    bottom = points [i].Y;
            }

            return new Rect2f ((float)left, (float)top, (float)(right - left), (float)(bottom - top));
        }

        /// <summary>
        /// Combine the current transform with another one.
        /// 
        /// The result is a transform that is equivalent to applying
        /// this followed by transform. Mathematically, it is
        /// equivalent to a matrix multiplication.
        /// </summary>
        /// <param name="transform">Transform to combine to this transform</param>
        public void Combine (Transform transform) {
            double[] a = _matrix;
            double[] b = transform.Matrix;

            this = new Transform (a [0] * b [0] + a [4] * b [1] + a [12] * b [3],
                a [0] * b [4] + a [4] * b [5] + a [12] * b [7],
                a [0] * b [12] + a [4] * b [13] + a [12] * b [15],
                a [1] * b [0] + a [5] * b [1] + a [13] * b [3],
                a [1] * b [4] + a [5] * b [5] + a [13] * b [7],
                a [1] * b [12] + a [5] * b [13] + a [13] * b [15],
                a [3] * b [0] + a [7] * b [1] + a [15] * b [3],
                a [3] * b [4] + a [7] * b [5] + a [15] * b [7],
                a [3] * b [12] + a [7] * b [13] + a [15] * b [15]);

        }

        /// <summary>
        /// Combine the current transform with a translation.
        /// </summary>
        /// <param name="x">Offset to apply on X axis</param>
        /// <param name="y">Offset to apply on Y axis</param>
        public void Translate (double x, double y) {
            Transform translation = new Transform (1, 0, x,
                                        0, 1, y,
                                        0, 0, 1);
            Combine (translation);
        }

        /// <summary>
        /// Combine the current transform with a translation.
        /// </summary>
        /// <param name="offset">Translation offset to apply</param>
        public void Translate (Vect2d offset) {
            Translate (offset.X, offset.Y);
        }

        /// <summary>
        /// Combine the current transform with a rotation.
        /// </summary>
        /// <param name="angle">Rotation angle, in degrees</param>
        public void Rotate (double angle) {
            double rad = angle * 3.141592654f / 180f;
            double cos = Math.Cos (rad);
            double sin = Math.Sin (rad);

            Transform rotation = new Transform (
                                     cos, -sin, 0,
                                     sin, cos, 0,
                                     0, 0, 1);

            Combine (rotation);
        }

        /// <summary>
        /// Combine the current transform with a rotation.
        /// 
        /// The center of rotation is provided for convenience as a second
        /// argument, so that you can build rotations around arbitrary points
        /// more easily (and efficiently) than the usual
        /// Translate(-center); Rotate(angle); Translate(center).
        /// </summary>
        /// <param name="angle">Rotation angle, in degrees</param>
        /// <param name="centerX">X coordinate of the center of rotation</param>
        /// <param name="centerY">Y coordinate of the center of rotation</param>
        public void Rotate (double angle, double centerX, double centerY) {
            double rad = angle * 3.141592654f / 180f;
            double cos = Math.Cos (rad);
            double sin = Math.Sin (rad);

            Transform rotation = new Transform (
                                     cos, -sin, centerX * (1 - cos) + centerY * sin,
                                     sin, cos, centerY * (1 - cos) - centerX * sin,
                                     0, 0, 1);

            Combine (rotation);
        }

        /// <summary>
        /// Combine the current transform with a rotation.
        /// 
        /// The center of rotation is provided for convenience as a second
        /// argument, so that you can build rotations around arbitrary points
        /// more easily (and efficiently) than the usual
        /// Translate(-center); Rotate(angle); Translate(center).
        /// </summary>
        /// <param name="angle">Rotation angle, in degrees</param>
        /// <param name="center">Center of rotation</param>
        public void Rotate (double angle, Vect2f center) {
            Rotate (angle, center.X, center.Y);
        }
        /*
        public void RotateZ (double angle, double axisX, double axisY, double axisZ) {

            double rad = angle * 3.141592654f / 180f;
            double cos = Math.Cos (rad);
            double sin = Math.Sin (rad);
            double t = 1 - cos;

            Transform rotation = new Transform (
                                     t * axisX * axisX + cos, t * axisX * axisY - sin * axisZ, t * axisX * axisZ + sin * axisY,
                                     t * axisX * axisY + sin * axisZ, t * axisY * axisY + cos, -t * axisY * axisZ - sin * axisX,
                                     t * axisX * axisZ - sin * axisY, t * axisY * axisZ + sin * axisX, t * axisZ * axisZ + cos);

            Combine (rotation);
        }
                                                 */
        /// <summary>
        /// Combine the current transform with a scaling.
        /// </summary>
        /// <param name="scaleX">Scaling factor on the X axis</param>
        /// <param name="scaleY">Scaling factor on the Y axis</param>
        public void Scale (double scaleX, double scaleY) {
            Transform scaling = new Transform (scaleX, 0, 0,
                                    0, scaleY, 0,
                                    0, 0, 1);

            Combine (scaling);
        }

        /// <summary>
        /// Combine the current transform with a scaling.
        /// 
        /// The center of scaling is provided for convenience as a second
        /// argument, so that you can build scaling around arbitrary points
        /// more easily (and efficiently) than the usual
        /// Translate(-center); Scale(factors); Translate(center).
        /// </summary>
        /// <param name="scaleX">Scaling factor on X axis</param>
        /// <param name="scaleY">Scaling factor on Y axis</param>
        /// <param name="centerX">X coordinate of the center of scaling</param>
        /// <param name="centerY">Y coordinate of the center of scaling</param>
        public void Scale (double scaleX, double scaleY, double centerX, double centerY) {
            Transform scaling = new Transform (scaleX, 0, centerX * (1 - scaleX),
                                    0, scaleY, centerY * (1 - scaleY),
                                    0, 0, 1);

            Combine (scaling);
        }

        /// <summary>
        /// Combine the current transform with a scaling.
        /// </summary>
        /// <param name="factors">Scaling factors</param>
        public void Scale (Vect2f factors) {
            Scale (factors.X, factors.Y);
        }

        /// <summary>
        /// Combine the current transform with a scaling.
        /// 
        /// The center of scaling is provided for convenience as a second
        /// argument, so that you can build scaling around arbitrary points
        /// more easily (and efficiently) than the usual
        /// Translate(-center); Scale(factors); Translate(center).
        /// </summary>
        /// <param name="factors">Scaling factors</param>
        /// <param name="center">Center of scaling</param>
        public void Scale (Vect2d factors, Vect2d center) {
            Scale (factors.X, factors.Y, center.X, center.Y);
        }

        public void Shear (double shearX, double shearY) {
            Transform shearing = new Transform (1, shearX, 0,
                                     shearY, 1, 0,
                                     0, 0, 1);

            Combine (shearing);
        }

        /// <summary>
        /// Overload of binary operator * to combine two transforms.
        /// This call is equivalent to calling new Transform(left).Combine(right).
        /// </summary>
        /// <param name="left">Left operand (the first transform)</param>
        /// <param name="right">Right operand (the second transform)</param>
        /// <returns>New combined transform</returns>
        public static Transform operator * (Transform left, Transform right) {
            left.Combine (right);
            return left;
        }

        /// <summary>
        /// Overload of binary operator * to transform a point.
        /// This call is equivalent to calling left.TransformPoint(right).
        /// </summary>
        /// <param name="left">Left operand (the transform)</param>
        /// <param name="right">Right operand (the point to transform)</param>
        /// <returns>New transformed point</returns>
        public static Vect2d operator * (Transform left, Vect2f right) {
            return left.TransformPoint (right);
        }

        /// <summary>
        /// Provide a string describing the object
        /// </summary>
        /// <returns>String description of the object</returns>
        public override string ToString () {
            return "[Transform]" +
            " Matrix(" +
            _matrix [0] + ", " + _matrix [4] + ", " + _matrix [12] +
            _matrix [1] + ", " + _matrix [5] + ", " + _matrix [13] +
            _matrix [3] + ", " + _matrix [7] + ", " + _matrix [15] +
            ")";
        }
    }
}
