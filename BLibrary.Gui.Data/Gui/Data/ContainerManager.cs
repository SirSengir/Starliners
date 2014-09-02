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
using Starliners.Game;
using Starliners;


namespace BLibrary.Gui.Data {

    public sealed class ContainerManager {
        /// <summary>
        /// Returns the container matching the given container id.
        /// </summary>
        /// <param name="containerId">Container identifier.</param>
        public Container this [int containerId] {
            get {
                if (!_containers.ContainsKey (containerId)) {
                    return null;
                }
                return _containers [containerId];
            }
        }

        /// <summary>
        /// Returns the first container matching the given tag.
        /// </summary>
        /// <param name="tag">Tag.</param>
        public Container this [string tag] {
            get {
                foreach (KeyValuePair<int, Container> entry in _containers) {
                    if (entry.Value.Tags.Contains (tag))
                        return entry.Value;
                }

                return null;
            }
        }

        internal IReadOnlyDictionary<int, Container> Containers {
            get {
                return _containers;
            }
        }

        /// <summary>
        /// Gets or sets the currently active container for this player or null if none is set or has been closed.
        /// </summary>
        /// <value>The active container.</value>
        public Container ActiveContainer {
            get {
                if (_activeContainer < 0) {
                    return null;
                }
                return this [_activeContainer];
            }
            set {
                _activeContainer = value != null ? value.ContainerId : -1;
            }
        }

        #region Fields

        static int _windowIds;
        Dictionary<int, Container> _containers = new Dictionary<int, Container> ();
        int _activeContainer = -1;

        #endregion

        public void Clear () {
            _activeContainer = -1;
            _containers.Clear ();
        }

        int GetNextContainerId () {
            _windowIds++;
            return _windowIds;
        }

        /// <summary>
        /// Determines whether this instance has a container matching the specified tag.
        /// </summary>
        /// <returns><c>true</c> if this instance has container the specified tag; otherwise, <c>false</c>.</returns>
        /// <param name="tag">Tag.</param>
        public bool HasContainer (string tag) {
            foreach (KeyValuePair<int, Container> entry in _containers) {
                if (entry.Value.Tags.Contains (tag)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Raises the OnChanged event on the container with the given id.
        /// </summary>
        /// <param name="containerId">Container identifier.</param>
        public void MarkChanged (int containerId) {
            Container container = this [containerId];
            if (container != null)
                container.OnChanged ();
        }

        /// <summary>
        /// Raises the OnChanged event on all containers with the given attribute
        /// on this player.
        /// </summary>
        /// <param name="tag">Tag.</param>
        public void MarkChanged (string tag) {
            foreach (KeyValuePair<int, Container> entry in _containers) {
                if (entry.Value.Tags.Contains (tag))
                    entry.Value.OnChanged ();
            }
        }

        /// <summary>
        /// Sets the container into this container manager.
        /// </summary>
        /// <param name="container">Container.</param>
        public void SetContainer (Container container) {
            _containers [container.ContainerId] = container;
        }

        /// <summary>
        /// Opens a gui with the given id for the given player and with the given arguments.
        /// </summary>
        /// <param name="player">Player to open the gui for.</param>
        /// <param name="id">Id of the type of gui to open.</param>
        /// <param name="args">Arguments to pass to the opened gui.</param>
        public void OpenGUI (Player player, ushort id, params object[] args) {
            OpenGUI (player, id, true, args);
        }

        /// <summary>
        /// Opens a gui with the given id for the given player and with the given arguments.
        /// </summary>
        /// <param name="player">Player to open the gui for.</param>
        /// <param name="id">Id of the type of gui to open.</param>
        /// <param name="sound">Opening will make a sound if set to <c>true</c>.</param>
        /// <param name="args">Arguments to pass to the opened gui.</param>
        public void OpenGUI (Player player, ushort id, bool sound, params object[] args) {
            lock (_containers) {

                // Add a container for the simulator
                Container container = GameAccess.Simulator.GetGuiContainer (id, player, args);
                if (container == null) {
                    throw new SystemException ("No gui container available for gui id " + id);
                }

                // Validate existing containers.
                foreach (Container existing in _containers.Values) {
                    Container.PrecedenceBehaviour precedence = existing.DeterminePrecedence (container);
                    switch (precedence) {
                        case Container.PrecedenceBehaviour.KeepExisting:
                            existing.BringToFront = true;
                            return;
                        case Container.PrecedenceBehaviour.ReplaceExisting:
                            existing.MustClose = true;
                            break;
                    }
                }

                // Now set the container id.
                container.ContainerId = GetNextContainerId ();
                container.BringToFront = true;
                _containers [container.ContainerId] = container;
            }

            if (sound) {
                player.Access.Controller.PlaySound (SoundKeys.CLICK, player);
            }
        }

        /// <summary>
        /// Closes the gui with the given container id for the given player.
        /// </summary>
        /// <param name="player">Player.</param>
        /// <param name="containerId">Container identifier.</param>
        public void CloseGui (Player player, int containerId) {
            if (!_containers.ContainsKey (containerId)) {
                return;
            }

            this [containerId].OnClose ();
            _containers.Remove (containerId);

        }
    }
}

