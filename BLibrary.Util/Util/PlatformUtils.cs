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
using System.Reflection;
using System.Runtime.InteropServices;

namespace BLibrary.Util {
    public sealed class PlatformUtils {

        #region Platform

        /// <summary>
        /// Attempts to detect the platform the application is currently running on.
        /// </summary>
        /// <remarks>>Will take into account Mono's bizarre decision to continue identifying MacOS as Unix.</remarks>
        /// <returns>The platform.</returns>
        public static PlatformOS DeterminePlatform () {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Unix:
                    return IsRunningOnMac () ? PlatformOS.MacOS : PlatformOS.Unix;
                case PlatformID.MacOSX:
                    return PlatformOS.MacOS;
                default:
                    return PlatformOS.Windows;
            }
        }

        static bool IsRunningOnMac () {
            IntPtr buf = IntPtr.Zero;
            try {
                buf = Marshal.AllocHGlobal (8192);
                if (uname (buf) == 0) {
                    string os = Marshal.PtrToStringAnsi (buf);
                    if (os == "Darwin") {
                        return true;
                    }
                }
            } catch {
            } finally {
                if (buf != IntPtr.Zero) {
                    Marshal.FreeHGlobal (buf);
                }
            }
            return false;
        }

        [DllImport ("libc")]
        static extern int uname (IntPtr buf);

        #endregion

        #region Version & CLR

        /// <summary>
        /// Will return the version of the entry assembly.
        /// </summary>
        /// <returns>The EXE version.</returns>
        public static Version GetEXEVersion () {
            return System.Reflection.Assembly.GetEntryAssembly ().GetName ().Version;
        }

        /// <summary>
        /// Will attempt to detect the CLR the application is running on (.NET or Mono) and returns an informational string.
        /// </summary>
        /// <returns>The CLR information.</returns>
        public static string GetCLRInformation (bool compact) {
            string runtime = compact ? string.Format ("{0}.{1}", Environment.Version.Major, Environment.Version.Minor) : Environment.Version.ToString ();

            if (GetCLRType () == CLRType.Mono) {
                runtime += " (Mono " + GetMonoVersion (compact) + ")";
            } else {
                runtime += " (.NET)";
            }
            return runtime;
        }

        static string GetMonoVersion (bool compact) {
            Type monoType = Type.GetType ("Mono.Runtime");
            MethodInfo displayName = monoType.GetMethod ("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
            if (displayName != null) {
                string version = displayName.Invoke (null, null).ToString ();
                if (!compact) {
                    return version;
                }

                return version.Split (' ') [0];
            }
            return "?.?.?";
        }

        /// <summary>
        /// Determines what type of CLR the program is running on. (.NET or Mono.)
        /// </summary>
        /// <returns></returns>
        public static CLRType GetCLRType () {
            Type monoType = Type.GetType ("Mono.Runtime");
            if (monoType != null) {
                return CLRType.Mono;
            }

            return CLRType.NET;
        }

        #endregion

    }
}
