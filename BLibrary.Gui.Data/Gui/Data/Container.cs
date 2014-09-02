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
using BLibrary.Network;
using BLibrary.Util;
using Starliners.Game;
using Starliners;

namespace BLibrary.Gui.Data {

    /// <summary>
    /// Data container class for guis.
    /// </summary>
    public class Container : IDataProvider, IActionHandler, IUpdateIndicator {
        #region Constants

        public const string KEY_STATUS = "entity.status";
        public const string KEY_UPDATE_LAST = "update.last";

        public const string ACTION_ACTIVATE = "make.active";

        public const string TAG_NOTIFICATION = "notification";
        public const string TAG_INVENTORY = "inventory";
        public const string TAG_HOTBAR = "hotbar";
        public const string TAG_MENUBAR = "menubar";

        #endregion

        #region Delegates

        protected delegate void ActionHandler (Player plyr, Container container, string key, Payload payload);

        public enum PrecedenceBehaviour {
            None,
            KeepExisting,
            ReplaceExisting
        }

        #endregion

        #region Properties

        public IReadOnlyDictionary<string, DataFragment> DataFragments {
            get {
                return _dataFragments;
            }
        }

        /// <summary>
        /// Id of the gui type this container is part of.
        /// </summary>
        /// <value>The GUI identifier.</value>
        public ushort GuiId {
            get;
            private set;
        }

        /// <summary>
        /// A unique id identifying the container on server and client.
        /// </summary>
        /// <value>The container identifier.</value>
        public int ContainerId {
            get;
            set;
        }

        public HashSet<string> Tags {
            get { return _tags; }
        }

        /// <summary>
        /// Indicates whether the container needs (re-)opening either for the first time or from a compacted state.
        /// </summary>
        /// <value><c>true</c> if needs opening; otherwise, <c>false</c>.</value>
        public bool NeedsOpening {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether a gui associated with this container must leave compacted state.
        /// </summary>
        /// <value><c>true</c> if must maximize; otherwise, <c>false</c>.</value>
        public bool BringToFront {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the gui must close.
        /// </summary>
        /// <value><c>true</c> if must close; otherwise, <c>false</c>.</value>
        public virtual bool MustClose {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating when the Container was last updated.
        /// </summary>
        public long LastUpdated {
            get {
                return GetValue<long> (KEY_UPDATE_LAST);
            }
        }

        protected ActionHandler ActionHandling {
            get;
            set;
        }

        #endregion

        #region Fields

        Dictionary<string, DataFragment> _dataFragments = new Dictionary<string, DataFragment> ();
        LinkedList<IUpdateIndicator> _updaters = new LinkedList<IUpdateIndicator> ();
        HashSet<string> _tags = new HashSet<string> ();

        #endregion

        #region Constructor

        public Container (ushort guiId) {
            GuiId = guiId;
            NeedsOpening = true;
            MarkUpdated ();
        }

        #endregion

        #region Updating

        /// <summary>
        /// Raises the changed event.
        /// </summary>
        public void OnChanged () {
            MarkUpdated ();
            Refresh (this, new EventArgs ());
        }

        /// <summary>
        /// Updates during gui update ticks. Use for regular updating.
        /// </summary>
        public virtual void UpdateTick () {
            foreach (IUpdateIndicator indicator in _updaters) {
                if (indicator.LastUpdated > LastUpdated) {
                    OnChanged ();
                }
            }
        }

        /// <summary>
        /// Refresh the container. Use for updates only on explicit OnChange events.
        /// </summary>
        /// <param name='sender'>
        /// Sender.
        /// </param>
        /// <param name='e'>
        /// EventArgs (unused)
        /// </param>
        protected virtual void Refresh (object sender, EventArgs e) {
            //HandleSubscriptions ();
        }

        /// <summary>
        /// Raises the close event. Make sure to call this in overriden methods to unsubscribe from events.
        /// </summary>
        public virtual void OnClose () {
        }

        protected void MarkUpdated () {
            UpdateFragment (KEY_UPDATE_LAST, DateTime.Now.Ticks);
        }

        protected void Subscribe (IUpdateIndicator indicator) {
            _updaters.AddLast (indicator);
        }

        #endregion

        #region Fragment Managment

        public void UpdateIcon (string icon) {
            UpdateFragment (Constants.FRAGMENT_GUI_ICON, icon);
        }

        public void UpdateFragment (DataFragment data) {
            _dataFragments [data.Key] = data;
        }

        public void UpdateFragment<T> (string key, T value) {
            // Add key if it doesn't exist yet.
            if (!_dataFragments.ContainsKey (key)) {
                _dataFragments.Add (key, new DataFragment<T> (key, value));
                return;
            }

            ((DataFragment<T>)_dataFragments [key]).Value = value;
            _dataFragments [key].IsDirty = true;
        }

        public void UpdateStatus (EntityStatus status) {
            if (!_dataFragments.ContainsKey (KEY_STATUS)) {
                _dataFragments [KEY_STATUS] = new DataFragment<EntityStatus> (KEY_STATUS, status);
                return;
            }

            EntityStatus previous = GetValue<EntityStatus> (KEY_STATUS);
            if (status == null && previous == null) {
                return;
            }
            if (previous != null && previous.Equals (status)) {
                return;
            }

            _dataFragments [KEY_STATUS] = new DataFragment<EntityStatus> (KEY_STATUS, status);
        }

        #region Retrieval

        public bool HasFragment (string key) {
            return _dataFragments.ContainsKey (key);
        }

        public T GetValue<T> (string key) {
            if (_dataFragments.ContainsKey (key)) {
                return ((DataFragment<T>)_dataFragments [key]).Value;
            } else {
                return default(T);
            }
        }

        #endregion

        #endregion

        #region Actions

        /// <summary>
        /// Handles an action sent by the client to the container.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        public virtual bool HandleAction (Player player, Container container, string key, Payload payload) {
            return HandleAction (player, key, payload);
        }

        public virtual bool HandleAction (Player player, string key, Payload payload) {
            if (ACTION_ACTIVATE.Equals (key)) {
                // Hud elements are not to be made active.
                if (Tags.Contains (TAG_HOTBAR) ||
                    Tags.Contains (TAG_INVENTORY) ||
                    Tags.Contains (TAG_MENUBAR)) {
                    return true;
                }

                player.ContainerManager.ActiveContainer = this;

                // Pass the action to the IActionHandlers
            } else if (GameAccess.Simulator.HandleGuiAction (player, this, key, payload)) {
                return true;
            } else if (ActionHandling != null) {
                ActionHandling (player, this, key, payload);
            }

            return true;
        }

        #endregion

        #region Precedence Resolving

        /// <summary>
        /// Indicates whether later containers of the same precedence override earlier ones.
        /// </summary>
        protected bool Overrides {
            get;
            set;
        }

        /// <summary>
        /// If set, this is used to resolve precedences.
        /// </summary>
        protected string Precedence {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this container takes precedence over the given other container.
        /// </summary>
        /// <remarks>An existing container which returns true here, will block opening a gui with the given other container for a player.</remarks>
        /// <param name="other"></param>
        /// <returns>True if this container takes precedence, false otherwise.</returns>
        public virtual PrecedenceBehaviour DeterminePrecedence (Container other) {
            if (!string.IsNullOrEmpty (Precedence) && string.Equals (Precedence, other.Precedence)) {
                return Overrides ? PrecedenceBehaviour.ReplaceExisting : PrecedenceBehaviour.KeepExisting;
            }

            return PrecedenceBehaviour.None;
        }

        #endregion
    }
}
