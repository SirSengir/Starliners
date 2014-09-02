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

﻿using BLibrary.Resources;
using System.Collections.Generic;
using System.IO;
using BLibrary.Json;

namespace Starliners.Game.Scenario {

    public sealed class ParseableResource {

        public IReadOnlyList<JsonObject> Elements {
            get { return _elements; }
        }

        List<JsonObject> _elements;

        public ParseableResource (ResourceFile resource) {

            JsonArray result = null;
            using (StreamReader reader = new StreamReader (resource.OpenRead ())) {
                result = JsonParser.JsonDecode (reader.ReadToEnd ()).GetValue<JsonArray> ();
            }

            _elements = new List<JsonObject> ();
            foreach (JsonNode node in result) {
                _elements.Add (node.GetValue<JsonObject> ());
            }
        }
    }
}

