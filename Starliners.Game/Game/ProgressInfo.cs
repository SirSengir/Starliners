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
using System.Runtime.Serialization;
using BLibrary.Util;

namespace Starliners.Game {

    [Serializable]
    public abstract class ProgressInfo : IDataReference<float>, ISerializable {

        public string Icon {
            get;
            private set;
        }

        public float Value {
            get { return Fraction; }
        }

        public long LastUpdated {
            get {
                return 0;
            }
        }

        /// <summary>
        /// Gets the current value of the progress as a fraction float from 0f - 1.0f.
        /// </summary>
        /// <value>The fraction.</value>
        public float Fraction {
            get;
            set;
        }

        #region Constructor

        public ProgressInfo (string icon) {
            Icon = icon;
        }

        #endregion

        #region Serialization

        public ProgressInfo (SerializationInfo info, StreamingContext context) {
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
        }

        #endregion

        public abstract float CalculateFraction ();

        /// <summary>
        /// Copy this instance.
        /// </summary>
        public abstract ProgressInfo Copy (Entity entity);
    }
}

