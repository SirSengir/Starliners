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
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace BLibrary.Resources {

    public class Localization {
        public static Localization Instance {
            get;
            set;
        }

        LinkedList<ResourceManager> resourceManagers = new LinkedList<ResourceManager> ();

        public string this [string key] {
            get {
                string localized;
                foreach (ResourceManager manager in resourceManagers) {
                    localized = manager.GetString (key, Culture);
                    if (localized != null)
                        return localized;
                }
                return key;
            }
        }

        public string this [string key, params object[] args] {
            get {
                return string.Format (this [key], args);
            }
        }

        public string this [IFormatProvider formatter, string key, params object[] args] {
            get {
                return string.Format (formatter, this [key], args);
            }
        }

        public CultureInfo Culture {
            get;
            set;
        }

        public Localization () {
            Culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

        }

        public void AddLocalization (ResourceCollection collection, Assembly assembly) {
            foreach (string localization in collection.LocalizationResources)
                AddLocalization (localization, assembly);
        }

        public void AddLocalization (string localization, Assembly assembly) {
            resourceManagers.AddLast (new ResourceManager (string.Format ("{0}.Resources.Localization.{1}", assembly.GetName ().Name, localization), assembly));
        }
    }
}
