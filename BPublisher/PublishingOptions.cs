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
using BLibrary.Json;
using System.Collections.Generic;

namespace BPublisher {
    sealed class PublishingOptions {

        /// <summary>
        /// Gets or sets the version to be published.
        /// </summary>
        /// <value>The version.</value>
        public Version Version {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the parent version to use.
        /// </summary>
        /// <value>The parent.</value>
        public Version Parent {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the manifest template.
        /// </summary>
        /// <value>The template.</value>
        public FileInfo Template {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the directory which is the source for the current files in this version.
        /// </summary>
        /// <value>The file source.</value>
        public DirectoryInfo FileSource {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the directory where old versions reside and new versions are published to.
        /// </summary>
        /// <value>The asset root.</value>
        public DirectoryInfo AssetRoot {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the channel the version is published for.
        /// </summary>
        /// <value>The channel.</value>
        public string Channel {
            get;
            set;
        }

        public DirectoryInfo ChannelRoot {
            get {
                return new DirectoryInfo (Path.Combine (AssetRoot.FullName, Channel));
            }
        }

        /// <summary>
        /// Gets the new manifest file.
        /// </summary>
        /// <value>The manifest file.</value>
        public FileInfo ManifestFile {
            get {
                return new FileInfo (Path.Combine (AssetRoot.FullName, Channel, Version.ToString (), PublisherConstants.MANIFEST_FILE));
            }
        }

        /// <summary>
        /// Gets the old manifest file.
        /// </summary>
        /// <value>The manifest file.</value>
        public FileInfo ParentManifestFile {
            get {
                return new FileInfo (Path.Combine (AssetRoot.FullName, Channel, Parent.ToString (), PublisherConstants.MANIFEST_FILE));
            }
        }

        public IReadOnlyList<BuildAction> PostBuildActions {
            get {
                return _postActions;
            }
        }

        List<BuildAction> _postActions = new List<BuildAction> ();

        public PublishingOptions (FileInfo presets) {
            try {
                string json = File.ReadAllText (presets.FullName);
                JsonObject result = JsonParser.JsonDecode (json).GetValue<JsonObject> ();
                Version = result.ContainsKey ("version") ? Version.Parse (result ["version"].GetValue<string> ()) : new Version ();
                Parent = result.ContainsKey ("parent") ? Version.Parse (result ["parent"].GetValue<string> ()) : new Version ();
                Template = result.ContainsKey ("template") ? new FileInfo (result ["template"].GetValue<string> ()) : new FileInfo ("Template.json");
                FileSource = result.ContainsKey ("fileSource") ? new DirectoryInfo (result ["fileSource"].GetValue<string> ()) : new DirectoryInfo ("bin/");
                AssetRoot = result.ContainsKey ("assetRoot") ? new DirectoryInfo (result ["assetRoot"].GetValue<string> ()) : new DirectoryInfo ("assets/");
                Channel = result.ContainsKey ("channel") ? result ["channel"].GetValue<string> () : "bnt";

                if (result.ContainsKey ("postbuild")) {
                    foreach (JsonObject action in result ["postbuild"].AsEnumerable<JsonObject>()) {
                        string ident = action ["action"].GetValue<string> ();
                        BuildAction built = null;
                        switch (ident) {
                            case "link":
                                built = new LinkAction (action);
                                break;
                            case "copy":
                                built = new CopyAction (action);
                                break;
                            default:
                                Console.Out.WriteLine ("Ignoring unrecognized action: " + ident);
                                break;
                        }

                        if (built != null) {
                            _postActions.Add (built);
                        }
                    }
                }

            } catch (Exception ex) {
                Console.Out.WriteLine ("Failed to read and parse publishing options: " + ex.Message);

                Version = new Version ();
                Parent = new Version ();
                Template = new FileInfo ("Template.json");
                FileSource = new DirectoryInfo ("bin/");
                AssetRoot = new DirectoryInfo ("assets/");
                Channel = "bnt";
            }
        }

        public bool IsGenerateable (out string reason) {

            if (Version.CompareTo (new Version ()) == 0) {
                reason = "A version needs to be given for generation.";
                return false;
            }
            if (!Template.Exists) {
                reason = string.Format ("Manifest template file '{0}' could not be found.", Template.FullName);
                return false;
            }
            if (!FileSource.Exists) {
                reason = string.Format ("File source '{0}' could not be found.", FileSource.FullName);
                return false;
            }
            if (!AssetRoot.Exists) {
                reason = string.Format ("Asset root '{0}' could not be found.", AssetRoot.FullName);
                return false;
            }
            if (string.IsNullOrWhiteSpace (Channel)) {
                reason = "A publishing channel must be given.";
                return false;
            }

            if (Parent.CompareTo (new Version ()) != 0) {
                FileInfo oldmanifest = new FileInfo (Path.Combine (AssetRoot.FullName, Channel, Parent.ToString (), PublisherConstants.MANIFEST_FILE));
                if (!oldmanifest.Exists) {
                    Console.Out.WriteLine ("Skipping parent version since its assets could not be found: " + Parent);
                    Parent = new Version ();
                }
            }

            reason = string.Empty;
            return true;
        }
    }
}

