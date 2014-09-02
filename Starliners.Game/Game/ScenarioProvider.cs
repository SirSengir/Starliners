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
using Starliners.Game;
using BLibrary.Util;
using Starliners.Game.Scenario;

namespace Starliners.Game {

    public sealed class ScenarioProvider : IScenarioProvider {
        public string Name {
            get { return string.Format ("Default"); }
        }

        public string Description {
            get { return "No description."; }
        }

        public IPopulator CreatePopulator () {
            return new Populator ();
        }
    }
}

