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


namespace Starliners.Game {

    [Serializable]
    public abstract class Asset : IdObject {
        #region Properties

        public sealed override ContentType ContentType {
            get {
                return ContentType.Asset;
            }
        }

        #endregion

        #region Callbacks

        public virtual void OnAddition () {
        }

        public virtual void OnRemoval () {
        }

        #endregion

        #region Constructor

        protected Asset (IWorldAccess access, string name, AssetKeyMap keyMap)
            : base (access, name, keyMap [access, name]) {
        }

        public virtual void OnCreated (IWorldAccess access, IPopulator populator) {
        }

        #endregion

        #region Serialization

        public Asset (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion
    }
}
