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
using BLibrary.Util;
using Starliners.Graphics;

namespace Starliners.Game.Scenario {
    sealed class CreatorBlueprints : AssetCreator {
        public CreatorBlueprints (int weight)
            : base (weight) {
        }

        public override IEnumerable<AssetHolder> CreateAssets (IWorldAccess access, IPopulator populator) {
            AssetHolder<Blueprint> holder = new AssetHolder<Blueprint> (Weight, AssetKeys.BLUEPRINTS);

            holder ["planet"] = new Blueprint (access, "planet", populator.KeyMap, new ObjectCategory ("planet", Colour.LightSteelBlue)) {
                UILayer = UILayer.Planets,
                RenderId = (ushort)RenderType.Planet,
                Interaction = new InteractionPlanet ()
            };
            holder ["fleet"] = new Blueprint (access, "fleet", populator.KeyMap, new ObjectCategory ("fleet", Colour.LightSteelBlue)) {
                UILayer = UILayer.Fleets,
                RenderId = (ushort)RenderType.Vessel,
                Interaction = new InteractionGui ((ushort)GuiIds.Fleet)
            };

            return new List<AssetHolder> { holder };
        }
    }
}

