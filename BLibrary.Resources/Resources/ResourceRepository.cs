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
using System.IO;
using System.Linq;
using System.Reflection;
using BLibrary.Saves;
using BLibrary.Util;
using System.IO.Compression;
using Starliners;

namespace BLibrary.Resources {

    /// <summary>
    /// Holds all resources for images and game data.
    /// </summary>
    public sealed class ResourceRepository {
        List<ResourceCollection> _collections = new List<ResourceCollection> ();
        bool _fileOverride;

        public IReadOnlyCollection<ResourceCollection> Collections {
            get {
                return _collections.AsReadOnly ();
            }
        }

        public ResourceRepository () {

            Assembly assemblyData = ReflectionUtils.SearchOrLoadAssembly (GameAccess.Game.DataAssembly);
            if (assemblyData == null)
                throw new SystemException ("Main data assembly was not loaded.");


            RegisterResourceAssembly (assemblyData, 0);
            Assembly assemblyResources = ReflectionUtils.SearchOrLoadAssembly (GameAccess.Game.ResourceAssembly);
            if (assemblyResources != null) {
                RegisterResourceAssembly (assemblyResources, 0);
            }

            foreach (FileInfo plugin in GetPluginsZip()) {
                RegisterResourcePackage (plugin, 99);
            }

            if (GameAccess.Folders [Constants.PATH_RESOURCES].Location.Exists) {
                Console.Out.WriteLine ("Enabled file override using location {0}.", GameAccess.Folders [Constants.PATH_RESOURCES].Location.FullName);
                _fileOverride = true;
            } else {
                Console.Out.WriteLine ("Disabled file override, location {0} does not exist.", GameAccess.Folders [Constants.PATH_RESOURCES].Location.FullName);
            }
        }

        FileInfo[] GetPluginsZip () {
            DirectoryInfo plugindir = GameAccess.Folders [Constants.PATH_PLUGINS].Location;
            if (!plugindir.Exists) {
                return new FileInfo[0];
            }

            return plugindir.GetFiles ("*?.zip").OrderBy (f => f.FullName).ToArray ();
        }

        /// <summary>
        /// Register the given assembly as a resource assembly.
        /// </summary>
        /// <param name="assembly">Assembly to register.</param>
        /// <param name="weight">The weight to attach to the assembly. Resource files of the same name in heavier resource collections override those in lighter ones.</param>
        /// <param name="hasLocalizations">Whether or not the passed assemblies contains localizations.</param>
        public void RegisterResourceAssembly (Assembly assembly, int weight) {
            ResourceCollection collection = new ResourceAssembly (assembly, weight);
            _collections.Add (collection);
            _collections.Sort ();

            if (collection.LocalizationResources.Length > 0)
                Localization.Instance.AddLocalization (collection, assembly);
        }

        /// <summary>
        /// Register the given package as a resource package.
        /// </summary>
        /// <param name="package">Package to register.</param>
        /// <param name="weight">The weight to attach to the package. Resource files of the same name in heavier resource collections override those in lighter ones.</param>
        public void RegisterResourcePackage (FileInfo file, int weight) {
            _collections.Add (new ResourceArchive (file,
                new ZipArchive (new FileStream (file.FullName, FileMode.Open), ZipArchiveMode.Read), weight));
            _collections.Sort ();
        }

        /// <summary>
        /// Returns the first resource file which ends in the given name.
        /// </summary>
        /// <param name="name">Pattern to search for.</param>
        /// <returns>Resource file if found, null if not.</returns>
        public ResourceFile SearchResource (string ident) {
            if (_fileOverride) {
                DirectoryInfo resdir = GameAccess.Folders [Constants.PATH_RESOURCES].Location;
                FileInfo res = new FileInfo (Path.Combine (resdir.FullName, StringUtils.ToFilePath (ident)).ToString ());
                if (res.Exists) {
                    GameAccess.Interface.GameConsole.Debug ("Found overridden file {0}, ignoring compiled resources.", res.FullName);
                    return new SystemFile (res);
                }
            }
            return SearchExact ("Resources." + ident);
        }

        /// <summary>
        /// Returns the first resource file which ends in the given name.
        /// </summary>
        /// <param name="name">Pattern to search for.</param>
        /// <returns>Resource file if found, null if not.</returns>
        public ResourceFile SearchExact (string name) {

            foreach (ResourceCollection collection in _collections) {
                // Skip disabled collections.
                if (!collection.IsEnabled)
                    continue;

                ResourceFile found = collection.SearchExact (name);
                if (found != null)
                    return found;
            }

            return null;
        }

        HashSet<string> overridden = new HashSet<string> ();

        /// <summary>
        /// Returns a list of resource files matching the given pattern.
        /// </summary>
        /// <param name="pattern">Pattern to search for.</param>
        /// <returns>List of resource files matching the given pattern. Empty if no files were found.</returns>
        public IEnumerable<ResourceFile> Search (string pattern) {
            LinkedList<ResourceFile> results = new LinkedList<ResourceFile> ();

            overridden.Clear ();

            // Search in the local resource directory if it exists.
            if (_fileOverride) {
                foreach (FileInfo file in FileUtils.RetrieveAllMatching(GameAccess.Folders [Constants.PATH_RESOURCES].Location, pattern)) {
                    SystemFile res = new SystemFile (file);
                    overridden.Add (res.Ident);
                    results.AddLast (res);
                }
            }

            foreach (ResourceCollection collection in _collections) {
                // Skip disabled collections.
                if (!collection.IsEnabled) {
                    continue;
                }

                IEnumerable<ResourceFile> found = collection.Search (pattern);
                if (found.Count () > 0) {
                    foreach (ResourceFile file in found) {
                        if (!overridden.Contains (file.Ident)) {
                            overridden.Add (file.Ident);
                            results.AddLast (file);
                        } else {
                            GameAccess.Simulator.GameConsole.Debug ("Resource file {0} in {1} was overriden in another resource collection.", file.Ident, collection.Name);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Returns the uids of all currently active and savegame modifying plugins.
        /// </summary>
        /// <returns></returns>
        public PluginInfo[] GetSavePlugins () {
            IList<PluginInfo> information = new List<PluginInfo> ();
            foreach (ResourceCollection collection in _collections) {
                if (!collection.IsEnabled || !collection.IsSaveModifier)
                    continue;
                information.Add (new PluginInfo (collection.UID, collection.Version));
            }
            return information.ToArray ();
        }

        /// <summary>
        /// Determines whether a plugin by the requested UID exists.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public PluginState GetPluginState (string uid, Version version) {
            foreach (ResourceCollection collection in _collections) {
                if (!collection.UID.Equals (uid))
                    continue;

                if (!collection.IsEnabled)
                    return PluginState.Disabled;
                if (collection.Version.Equals (version))
                    return PluginState.Available;
                else
                    return PluginState.Outdated;
            }

            return PluginState.Missing;
        }

        /// <summary>
        /// Returns the plugin name for the given UID.
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string GetPluginName (string uid) {
            foreach (ResourceCollection collection in _collections) {
                if (!collection.UID.Equals (uid))
                    continue;

                return collection.Name;
            }

            return uid;
        }

        /// <summary>
        /// Enables or disables a plugin.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public void SetPluginState (string uid, bool enabled) {
            foreach (ResourceCollection collection in _collections) {
                if (!collection.UID.Equals (uid))
                    continue;

                collection.IsEnabled = enabled;
                GameAccess.Settings.Set ("plugins", collection.UID, enabled);
                GameAccess.Settings.Flush ();
                break;
            }
        }

        /// <summary>
        /// Returns a list of strings containing version information.
        /// </summary>
        /// <returns></returns>
        public string[] GetVersionInformation () {
            IList<string> information = new List<string> ();

            information.Add (string.Format ("= EXE Version: {0}", PlatformUtils.GetEXEVersion ()));
            information.Add (string.Format ("= Game Version: {0}", GameAccess.Game.GetType ().Assembly.GetName ().Version));
            information.Add (string.Format ("= Simulator Version: {0}", GameAccess.Simulator.GetType ().Assembly.GetName ().Version));
            if (GameAccess.Interface != null) {
                information.Add (string.Format ("= Interface Version: {0}", GameAccess.Interface.GetType ().Assembly.GetName ().Version));
            }
            information.Add ("= Plugins:");
            foreach (ResourceCollection collection in _collections) {
                information.Add (string.Format ("=\t\"{0}\": {1}", collection.Name, collection.Version));
            }

            information.Add ("=");
            information.Add ("= Hash: " + GetIdentHash ());
            information.Add ("= CLR: " + PlatformUtils.GetCLRInformation (false));

            return information.ToArray ();
        }

        /// <summary>
        /// Returns a hash for this install.
        /// </summary>
        /// <returns></returns>
        public string GetIdentHash () {
            int hash = System.Reflection.Assembly.GetEntryAssembly ().GetHashCode ();
            hash ^= GameAccess.Game.GetType ().Assembly.GetHashCode ();
            if (GameAccess.Interface != null) {
                hash ^= GameAccess.Interface.GetType ().Assembly.GetHashCode ();
            }
            hash ^= GameAccess.Simulator.GetType ().Assembly.GetHashCode ();

            foreach (ResourceCollection collection in _collections) {
                hash ^= collection.GetHashCode ();
            }

            return hash.ToString ("X");
        }
    }
}
