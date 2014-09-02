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
using System.IO;
using BLibrary.Util;

namespace BPublisher {
    sealed class ArgumentDefinitions : IArgumentDefinition {

        PublishingOptions _options;

        public ArgumentDefinitions (PublishingOptions options) {
            _options = options;
        }

        public bool HandleArgument (string key, string param) {
            switch (key) {
                case "base":
                    _options.Parent = Version.Parse (param);
                    return true;
                case "parentbuild":
                    _options.Parent = new Version (_options.Parent.Major, _options.Parent.Minor, _options.Parent.Build, int.Parse (param));
                    return true;
                case "version":
                    _options.Version = Version.Parse (param);
                    return true;
                case "build":
                    _options.Version = new Version (_options.Version.Major, _options.Version.Minor, _options.Version.Build, int.Parse (param));
                    return true;
                case "template":
                    _options.Template = new FileInfo (param);
                    return true;
                case "channel":
                    _options.Channel = param;
                    return true;
                case "assetroot":
                    _options.AssetRoot = new DirectoryInfo (param);
                    return true;
                default:
                    return false;
            }
        }

    }
}

