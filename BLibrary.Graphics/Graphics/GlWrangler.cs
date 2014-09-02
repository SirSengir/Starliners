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

using System;
using OpenTK.Graphics.OpenGL;
using BLibrary.Util;

namespace BLibrary.Graphics {

    /// <summary>
    /// Wraps some GL functions which are platform dependent.
    /// </summary>
    public class GlWrangler {
        internal delegate int GLGenVertexArray ();

        internal static GLGenVertexArray GenVertexArray;

        internal delegate void GLBindVertexArray (int id);

        internal static GLBindVertexArray BindVertexArray;

        internal delegate void GLDeleteVertexArray (int id);

        internal static GLDeleteVertexArray DeleteVertexArray;
        /*
        internal static int GenVertexArray () {
            //return GL.Apple.GenVertexArray();
            return GL.GenVertexArray();
        }
        internal static void BindVertexArray (int id) {
            //GL.Apple.BindVertexArray(id);
            GL.BindVertexArray(id);
        }
        internal static void DeleteVertexArray (int id) {
            //GL.Apple.DeleteVertexArray(id);
            GL.DeleteVertexArray(id);
        }
        */
        static GlWrangler () {
            if (PlatformUtils.DeterminePlatform () == PlatformOS.MacOS) {
                GenVertexArray = GL.Apple.GenVertexArray;
                BindVertexArray = GL.Apple.BindVertexArray;
                DeleteVertexArray = GL.Apple.DeleteVertexArray;
            } else {
                GenVertexArray = GL.GenVertexArray;
                BindVertexArray = GL.BindVertexArray;
                DeleteVertexArray = GL.DeleteVertexArray;
            }
        }

        /// <summary>
        /// Ensures that state is disabled
        /// </summary>
        /// <param name="cap"></param>
        /// <param name="code"></param>
        public static void SafeGLEnable (EnableCap cap, Action code) {
            GL.Enable (cap);

            code ();

            GL.Disable (cap);
        }

        /// <summary>
        /// Ensures that multiple states are disabled
        /// </summary>
        /// <param name="cap"></param>
        /// <param name="code"></param>
        public static void SafeGLEnable (EnableCap[] caps, Action code) {
            foreach (var cap in caps)
                GL.Enable (cap);

            code ();

            foreach (var cap in caps)
                GL.Disable (cap);
        }

        /// <summary>
        /// Disables a cap and re-enables it, if it had been set.
        /// </summary>
        /// <param name="cap">Cap.</param>
        /// <param name="code">Code.</param>
        public static void SafeGLDisable (EnableCap cap, Action code) {
            bool disableCap = GL.IsEnabled (cap);
            if (disableCap) {
                GL.Disable (cap);
            }

            code ();

            if (disableCap) {
                GL.Enable (cap);
            }
        }

        public static void SafeGLEnableClientStates (ArrayCap[] caps, Action code) {
            foreach (var cap in caps)
                GL.EnableClientState (cap);

            code ();

            foreach (var cap in caps)
                GL.DisableClientState (cap);
        }

        public static int ToRgba (System.Drawing.Color color) {
            return color.A << 24 | color.B << 16 | color.G << 8 | color.R;
        }
    }
}

