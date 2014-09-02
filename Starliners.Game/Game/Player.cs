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
using System.Linq;
using System.Runtime.Serialization;
using BLibrary.Network;
using BLibrary.Serialization;
using BLibrary.Util;
using BLibrary.Gui.Data;
using Starliners.Game.Notifications;
using Starliners.Network;

namespace Starliners.Game {

    /// <summary>
    /// Represents a player.
    /// </summary>
    [Serializable]
    public sealed class Player : IdObject {
        #region Constants

        public const string CMPT_ITEMS = "items";
        public const int TOOLBAR_LENGTH = 12;
        const int ACTIVATION_TIMEOUT = 10000;

        #endregion

        #region Properties

        public override ContentType ContentType { get { return ContentType.Player; } }

        /// <summary>
        /// The container manager for this player.
        /// </summary>
        /// <value>The container manager.</value>
        public ContainerManager ContainerManager {
            get { return _containerManager; }
        }

        /// <summary>
        /// The entity currently selected by this player.
        /// </summary>
        public Entity SelectedEntity { get; set; }

        /// <summary>
        /// The object currently held by the player.
        /// </summary>
        /// <value>The held object.</value>
        public IHoldable HeldObject { get; set; }

        public Faction MainFaction {
            get {
                return _command.Count > 0 ? Access.RequireState<Faction> (_command.First ().Key) : null;
            }
        }

        /// <summary>
        /// The current location of the screen center for this player.
        /// </summary>
        [GameData (Remote = true, Key = "ViewCenter")]
        public Vect2f Location {
            get;
            set;
        }

        /// <summary>
        /// Flags set on this player.
        /// </summary>
        public HashSet<string> Flags {
            get {
                return _flags;
            }
        }

        /// <summary>
        /// Statistical records for this player.
        /// </summary>
        public StatsRecorder<int> Statistics {
            get {
                return _statistics;
            }
        }

        /// <summary>
        /// Bookkeeping for this player.
        /// </summary>
        /// <value>The bookkeeping.</value>
        [GameData (Key = "Bookkeeping")]
        public Bookkeeping Bookkeeping {
            get;
            private set;
        }

        /// <summary>
        /// Gets the high score for this player.
        /// </summary>
        /// <value>The high score.</value>
        [GameData (Key = "HighScore")]
        public ScoreKeeper HighScore {
            get;
            private set;
        }

        public float ActivationProgress {
            get {
                return Target != null ? (float)(Access.Clock.Ticks - Target.StartTime) / Target.EstimatedDuration : 0;
            }
        }

        public PlayerTarget Target {
            get;
            private set;
        }

        public bool IsOnline {
            get;
            set;
        }

        #endregion

        #region Fields

        ContainerManager _containerManager = new ContainerManager ();

        [GameData (Key = "Statistics")]
        StatsRecorder<int> _statistics = new StatsRecorder<int> ();
        List<Notification> _notifications = new List<Notification> ();
        [GameData (Remote = true, Key = "States")]
        Dictionary<string, StateObject> _states = new Dictionary<string, StateObject> ();
        [GameData (Remote = true, Key = "Flags")]
        HashSet<string> _flags = new HashSet<string> ();

        ControlState _activationControls;

        [GameData (Remote = true, Key = "Command")]
        Dictionary<ulong, CommandToken> _command = new Dictionary<ulong, CommandToken> ();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StarLiners.Game.Player"/> class.
        /// </summary>
        /// <param name='galaxy'>
        /// Galaxy.
        /// </param>
        /// <param name='login'>
        /// Login.
        /// </param>
        /// <param name='faction'>
        /// Faction.
        /// </param>
        public Player (IWorldAccess access, string login)
            : base (access, login) {

            Bookkeeping = new Bookkeeping (Access, 5000);
            HighScore = new ScoreKeeper (Access);
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Initializes a new instance of the <see cref="StarLiners.Game.Player"/> class.
        /// </summary>
        /// <param name='info'>
        /// Info.
        /// </param>
        /// <param name='context'>
        /// Context.
        /// </param>
        public Player (SerializationInfo info, StreamingContext context)
            : base (info, context) {
            //if (Funds < 2000)
            //    Funds = 2000;
        }

        #endregion

        public override void Tick (TickType ticks) {
            base.Tick (ticks);

            // Throw open popup windows for notifications requiring it.
            for (int i = 0; i < _notifications.Count; i++) {
                if (!_notifications [i].Handling.HasFlag (NotificationHandling.Popup)) {
                    continue;
                }
                if (_notifications [i].IsHandled (NotificationHandling.Popup)) {
                    continue;
                }

                _notifications [i].MarkHandled (NotificationHandling.Popup);
                OpenGUI (GameAccess.Game.IdGuiPopup, _notifications [i]);
            }

            if (Target != null) {
                int duration = (int)(Access.Clock.Ticks - Target.StartTime);
                if (Target.OnTargetingTick (this, duration, _activationControls)
                    || duration > ACTIVATION_TIMEOUT/* || !CanReach (Target.Location)*/) {
                    ResetTarget ();
                }
            }
        }

        /// <summary>
        /// Makes the player controller of the given faction.
        /// </summary>
        /// <param name="faction">Faction.</param>
        public void MakeController (Faction faction) {
            _command [faction.Serial] = new CommandToken ();
        }

        /// <summary>
        /// Opens a gui screen. Needs to be called server side.
        /// </summary>
        /// <param name='id'>
        /// Identifier.
        /// </param>
        /// <param name='window'>
        /// Window.
        /// </param>
        /// <param name='args'>
        /// Arguments.
        /// </param>
        public void OpenGUI (ushort id, params object[] args) {
            ContainerManager.OpenGUI (this, id, true, args);
        }

        /// <summary>
        /// Opens a gui screen. Needs to be called server side.
        /// </summary>
        public void OpenGUI (ushort id, bool sound, params object[] args) {
            ContainerManager.OpenGUI (this, id, sound, args);
        }

        /// <summary>
        /// Starts activation of the given entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="control"></param>
        public void TargetEntity (Entity entity, ControlState control) {
            if (entity.Blueprint.Interaction == null) {
                return;
            }
            if (!entity.Blueprint.Interaction.CanActivate (entity, this, control)) {
                return;
            }

            Target = new PlayerTargetEntity (this, entity);
            _activationControls = control;
            MarkUpdated (UpdateMarker.Update4);
        }

        public void ResetTarget () {
            Target = null;
            _activationControls = ControlState.None;
            MarkUpdated (UpdateMarker.Update4);
        }

        /// <summary>
        /// Unsets the flag if it has been set previously or sets it, if it hasn't been set.
        /// </summary>
        /// <param name="flag">Flag to toggle.</param>
        public void ToggleFlag (string flag) {
            if (_flags.Contains (flag))
                _flags.Remove (flag);
            else
                _flags.Add (flag);

            MarkUpdated (UpdateMarker.Update1);
        }

        /// <summary>
        /// Gets the notifications currently on this player. (These are not persistent.)
        /// </summary>
        /// <value>The notifications.</value>
        public IList<Notification> Notifications {
            get { return _notifications; }
        }

        /// <summary>
        /// Gets the notification settings.
        /// </summary>
        /// <value>The notification settings.</value>
        public NotificationSettings NotificationSettings {
            get {
                if (!_states.ContainsKey (NotificationSettings.TRACKER_KEY)) {
                    _states [NotificationSettings.TRACKER_KEY] = new NotificationSettings (Access, NotificationSettings.TRACKER_KEY);
                    if (Access.Controller.IsSimulating) {
                        Access.Controller.QueueState (_states [NotificationSettings.TRACKER_KEY]);
                    }
                }
                return (NotificationSettings)_states [NotificationSettings.TRACKER_KEY];
            }
        }

        /// <summary>
        /// Post a notification on this player.
        /// </summary>
        /// <param name="notification">Notification.</param>
        public void PostNotification (Notification notification) {
            _notifications.Add (notification);

            ContainerManager.MarkChanged (Container.TAG_NOTIFICATION);
            Access.Controller.PlaySound (SoundKeys.NOTIFICATION, this);
        }

        /// <summary>
        /// Determines whether this player has the specified faction permission.
        /// </summary>
        /// <returns><c>true</c> if this instance has permission the specified faction permission; otherwise, <c>false</c>.</returns>
        /// <param name="faction">Faction.</param>
        /// <param name="permission">Permission.</param>
        public bool HasPermission (Faction faction, string permission) {
            if (!_command.ContainsKey (faction.Serial)) {
                return false;
            }

            return _command [faction.Serial].HasPermission (permission);
        }

        public void SendChat (string text, params string[] args) {
            SendChat (new TextComponent (text), TextComponent.ConvertToComponents (args));
        }

        public void SendChat (Colour colour, string text, params string[] args) {
            SendChat (new TextComponent (string.Format ("§{0}§{1}", colour.ToString ("#"), text)), TextComponent.ConvertToComponents (args));
        }

        public void SendChat (TextComponent text, params TextComponent[] args) {
            Access.Controller.SendChat (this, new TextComposition (text, args));
        }

        #region Movement

        /// <summary>
        /// Moves the player by the given delta as far as possible.
        /// </summary>
        /// <remarks>Called on both the simulator and the interface.</remarks>
        /// <param name="delta">Delta.</param>
        /// <returns>true if any movement occured, false otherwise.</returns>
        public bool Move (Vect2f delta) {
            if (delta == Vect2f.ZERO) {
                return false;
            }

            // If we are in an invalid location, we allow walking anywhere.
            //if (!IsValidDestination (Location)) {
            //    Vect2f destination = Location + delta;
            //    Location = destination;
            //    return true;
            //}

            // Both directions valid.
            if (TryWalkTo (delta)) {
                return true;
            }
            // Only X.
            if (TryWalkTo (new Vect2f (delta.X, 0))) {
                return true;
            }
            // Only Y.
            if (TryWalkTo (new Vect2f (0, delta.Y))) {
                return true;
            }

            return false;
        }

        bool TryWalkTo (Vect2f delta) {
            Vect2f destination = Location + delta;
            if (!IsValidDestination (destination)) {
                return false;
            }
            Location = destination;
            return true;
        }

        bool IsValidDestination (Vect2f destination) {

            /*
            if (!Access.IsWithinWorld (bounds.Center) || !Avatar.CanPass (bounds.Center, Location, false)) {
                return false;
            }
            */

            return true;
        }

        #endregion

        #region Networking

        public override IPacketMarked GetUpdatePacket () {
            if (UpdateSchedule.HasFlag (UpdateMarker.Update0)) {
                UnscheduleUpdate (UpdateMarker.Update0);
                //return new PacketUpdateStream (PacketId.UpdateStream, this, UpdateMarker.Update0, Blazon);
            }
            if (UpdateSchedule.HasFlag (UpdateMarker.Update1)) {
                UnscheduleUpdate (UpdateMarker.Update1);
                return new PacketUpdateStream (PacketId.UpdateStream, this, UpdateMarker.Update1, _flags);
            }
            if (UpdateSchedule.HasFlag (UpdateMarker.Update3)) {
                UnscheduleUpdate (UpdateMarker.Update3);
                return new PacketUpdatePayload (PacketId.UpdatePayload, this, UpdateMarker.Update3, Location);
            }
            if (UpdateSchedule.HasFlag (UpdateMarker.Update4)) {
                UnscheduleUpdate (UpdateMarker.Update4);
                return new PacketUpdateStream (PacketId.UpdateStream, this, UpdateMarker.Update4, Target);
            }
            return base.GetUpdatePacket ();
        }

        public override void HandleNetworkUpdate (IPacketMarked packet) {
            if (packet.Marker == UpdateMarker.Update0) {
                //PacketUpdateStream pack = (PacketUpdateStream)packet;
                //Blazon = pack.DeserializeContent<Blazon> (Access);
            } else if (packet.Marker == UpdateMarker.Update1) {
                PacketUpdateStream pack = (PacketUpdateStream)packet;
                _flags = pack.DeserializeContent <HashSet<string>> (Access);
            } else if (packet.Marker == UpdateMarker.Update2) {
                PacketUpdatePayload pack = (PacketUpdatePayload)packet;
                // Move and verify collisions
                Move (new Vect2f (pack.Payload.GetValue<float> (0), pack.Payload.GetValue<float> (1)));
                // Verify interface side simulation and correct it if necessary
                Vect2f relocated = new Vect2f (pack.Payload.GetValue<float> (2), pack.Payload.GetValue<float> (3));
                if (relocated != Location) {
                    MarkUpdated (UpdateMarker.Update3);
                }
            } else if (packet.Marker == UpdateMarker.Update3) {
                PacketUpdatePayload pack = (PacketUpdatePayload)packet;
                Location = pack.Payload.GetValue<Vect2f> (0);
            } else if (packet.Marker == UpdateMarker.Update4) {
                PacketUpdateStream pack = (PacketUpdateStream)packet;
                Target = pack.DeserializeContent<PlayerTarget> (Access);
            } else
                base.HandleNetworkUpdate (packet);
        }

        #endregion

        public override string ToString () {
            return string.Format ("[Player: Name={0}, UID={1}]", Name, Serial);
        }
    }
}
