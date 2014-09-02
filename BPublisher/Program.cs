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
using System.Collections.Generic;

namespace BPublisher {
    class MainClass {
        static ArgumentHandler _argumentHandler;

        public static void Main (string[] args) {
            _argumentHandler = new ArgumentHandler ();

            string presetfile = _argumentHandler.GetParams ("presets", args);
            presetfile = string.IsNullOrWhiteSpace (presetfile) ? "Presets.json" : presetfile;
            PublishingOptions options = new PublishingOptions (new FileInfo (presetfile));

            _argumentHandler.Define (new ArgumentDefinitions (options));
            _argumentHandler.HandleArgs (args);

            string reason = string.Empty;
            if (!options.IsGenerateable (out reason)) {
                Console.Out.WriteLine ("Cannot generate a manifest: " + reason);
                return;
            }

            Console.Out.WriteLine ("Generating manifest for version {0}.", options.Version);
            Console.Out.WriteLine ("Deploying to channel '{0}' in asset root '{1}'.", options.Channel, options.AssetRoot.FullName);

            FileInfo manifestfile = options.ManifestFile;
            Console.Out.WriteLine ("Manifest file {0}.", manifestfile.FullName);

            AssetLibrary template = new AssetLibrary (options.Template);
            if (template.FileAssetMap.Count > 0) {
                Console.Out.WriteLine ("Read template file {0}.", options.Template.FullName);
            } else {
                Console.Out.WriteLine ("Failed to read or parse template file '{0}'.", options.Template.FullName);
                return;
            }

            if (!manifestfile.Directory.Exists) {
                Console.Out.WriteLine ("Directory '{0}' does not exist yet, creating...", manifestfile.Directory.FullName);
                manifestfile.Directory.Create ();
            }

            AssetLibrary manifest = new AssetLibrary (options.Version);
            bool update = options.Parent.CompareTo (new Version ()) != 0;
            if (update) {
                Console.Out.WriteLine ("Using version {0} as a parent.", options.Parent);
            }

            foreach (var asset in template.FileAssetMap.Values) {

                FileAsset fasset = null;
                IList<FileInfo> sources = asset.GetLocalSources (options.FileSource);
                foreach (FileInfo source in sources) {
                    if (!source.Exists) {
                        Console.Out.WriteLine ("Required the file '{0}' as a source, but it wasn't found.", source.FullName);
                        return;
                    }
                }

                IList<FileInfo> targets = asset.GetLocalTargets (manifestfile.Directory, false);

                // Compare to old version if needed. If old and new file match,
                // just copy over the old asset and continue;
                if (update) {

                    bool equal = true;
                    AssetLibrary parentmanifest = new AssetLibrary (options.ParentManifestFile);

                    if (parentmanifest.FileAssetMap.ContainsKey (asset.Ident)) {
                        IList<FileInfo> parents = asset.GetLocalTargets (options.ParentManifestFile.Directory, false);

                        for (int i = 0; i < sources.Count; i++) {
                            if (parents [i].Exists && FileUtils.AreIdenticalFiles (sources [i], parents [i])) {
                                continue;
                            }

                            equal = false;
                            Console.Out.WriteLine ("Asset {0} is outdated and will be updated.", asset);
                            break;
                        }
                    } else {
                        equal = false;
                    }

                    if (equal) {
                        Console.Out.WriteLine ("Asset {0} matches the parent version and will not be marked for update.", asset);
                        fasset = new FileAsset (asset.Ident, asset.AssetType, parentmanifest.FileAssetMap [asset.Ident].Version, asset.IsCompressed, asset.NativeMap);
                    }
                }

                if (fasset == null) {
                    fasset = new FileAsset (asset.Ident, asset.AssetType, options.Version, asset.IsCompressed, asset.NativeMap);
                }
                manifest.AddAsset (fasset);
                Console.Out.WriteLine ("Added file asset '{0}' to manifest.", fasset);

                // Copy and compress the source files as needed
                for (int i = 0; i < sources.Count; i++) {
                    if (!targets [i].Directory.Exists) {
                        targets [i].Directory.Create ();
                    }
                    Console.Out.WriteLine ("Copying source file '{0}' to target file '{1}'.", sources [i], targets [i]);
                    sources [i].CopyTo (targets [i].FullName, true);

                    // Compress in place.
                    if (asset.IsCompressed) {
                        ZipUtils.CompressFile (targets [i], false);
                    }
                }

            }

            Console.Out.WriteLine ("Generated manifest.");
            manifest.WriteToManifest (manifestfile.FullName);
            Console.Out.WriteLine ("Wrote manifest file '{0}'.", manifestfile);

            foreach (BuildAction action in options.PostBuildActions) {
                action.Execute (options.ChannelRoot, options.ManifestFile.Directory);
            }
        }
    }
}
