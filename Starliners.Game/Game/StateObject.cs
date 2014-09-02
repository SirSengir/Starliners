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

﻿using BLibrary.Serialization;
using System.Runtime.Serialization;

namespace Starliners.Game {

    public abstract class StateObject : IdObject {
        #region Properties

        public sealed override ContentType ContentType {
            get { return ContentType.State; }
        }

        /// <summary>
        /// Indicates whether this state object is dead. If true, the object will be scheduled for removal during the next game tick.
        /// </summary>
        public bool IsDead {
            get;
            set;
        }

        #endregion

        #region Callbacks

        public virtual void OnStatesLoaded () {
        }

        #endregion

        #region Constructor

        protected StateObject (IWorldAccess access, string name)
            : base (access, name) {
        }

        #endregion

        #region Serialization

        public StateObject (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        protected virtual void CopyFrom (StateObject other) {
        }

        public override string ToString () {
            return string.Format ("[StateObject: Serial={0}, Name={1}]", Serial, Name);
        }
    }
}
