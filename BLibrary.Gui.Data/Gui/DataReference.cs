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
using BLibrary.Resources;
using BLibrary.Util;
using BLibrary.Gui.Data;

namespace BLibrary.Gui {

    /// <summary>
    /// Wraps a reference to a data fragment inside an IDataProvider.
    /// </summary>
    public sealed class DataReference<T> : ITextProvider, IDataReference<T> {
        #region Properties

        public bool IsUpdated {
            get {
                return _parent.IsUpdated;
            }
        }

        public long LastUpdated {
            get {
                return _parent.DataProvider.LastUpdated;
            }
        }

        public bool Localizable {
            get;
            set;
        }

        public string Template {
            get;
            set;
        }

        public IFormatProvider CustomFormatter {
            get;
            set;
        }

        public T Value {
            get { return _parent.DataProvider.GetValue<T> (_key); }
        }

        #endregion

        string _key;
        IDataContainer _parent;

        #region Constructor

        public DataReference (IDataContainer parent, string key) {
            _parent = parent;
            _key = key;
        }

        #endregion

        public override string ToString () {
            if (string.IsNullOrEmpty (Template)) {
                return Localizable ? Localization.Instance [Value.ToString ()] : Value.ToString ();
            } else if (Localizable) {
                return CustomFormatter != null ? string.Format (CustomFormatter, Template, Localization.Instance [Value.ToString ()]) : string.Format (Template, Localization.Instance [Value.ToString ()]);
            } else {
                return CustomFormatter != null ? string.Format (CustomFormatter, Template, Value) : string.Format (Template, Value);
            }
        }
    }
}

