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
using BLibrary.Json;
using System.Runtime.Serialization;
using Starliners.Game.Planets;

namespace Starliners.Game {
    [Serializable]
    sealed class TriggerCulture : Trigger {

        Culture _culture;

        #region Constructor

        public TriggerCulture (IWorldAccess access, IPopulator populator, JsonObject json)
            : base (access) {
            AssetHolder<Culture> cultures = (AssetHolder<Culture>)populator.Holders [AssetKeys.CULTURES];
            _culture = json.ContainsKey ("value") ? cultures.GetAsset (json ["value"].GetValue<string> ()) : cultures.GetAsset ("imperial");
        }

        #endregion

        #region Serialization

        public TriggerCulture (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        /// <summary>
        /// Determines whether this instance is tripped by the specified planet.
        /// </summary>
        /// <returns><c>true</c> if this instance is tripped the specified planet; otherwise, <c>false</c>.</returns>
        /// <param name="planet">Planet.</param>
        public override bool IsTripped (ILevyProvider planet) {
            return planet.Culture == _culture;
        }

        public override bool IsTripped (Planet planet) {
            return planet.Culture == _culture;
        }

    }
}

