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
using BLibrary.Resources;
using BLibrary.Serialization;
using BLibrary.Network;
using Starliners.Graphics;
using BLibrary.Util;
using Starliners.Network;
using BLibrary.Gui;

namespace Starliners.Game {

    [Serializable]
    public abstract class Entity : IdObject, ILocatable, IRenderable, IDescribable, ILegendController {
        #region Constants

        static readonly string[] NO_INFORMATION = new string[0];

        #endregion

        #region Properties

        public sealed override ContentType ContentType {
            get {
                return ContentType.Entity;
            }
        }

        /// <summary>
        /// Defines static entity properties. Needs to be defined in subclasses.
        /// </summary>
        public abstract Blueprint Blueprint {
            get;
        }

        public abstract Faction Owner {
            get;
        }

        #region State Information

        /// <summary>
        /// World tick on which the entity was created.
        /// </summary>
        [GameData (Remote = true, Key = "Inception")]
        public long Inception {
            get;
            protected set;
        }

        /// <summary>
        /// Age of the entity in ticks as determined by the current game tick and the entity's inception tick.
        /// </summary>
        public long Age {
            get {
                return Access.Clock.Ticks - Inception;
            }
        }

        /// <summary>
        /// Indicates whether this entity is dead. If true, the entity will be scheduled for removal during the next game tick.
        /// </summary>
        public bool IsDead {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this entity is rendered on the map.
        /// </summary>
        public bool IsVisible {
            get {
                return _isVisible;
            }
            set {
                if (_isVisible == value) {
                    return;
                }
                _isVisible = value;
                ScheduleUpdate (UpdateMarker.Visibility);
            }
        }

        /// <summary>
        /// Gets the entity's luminosity.
        /// </summary>
        /// <value>The luminosity.</value>
        public virtual byte Luminosity {
            get { return Blueprint.Luminosity; }
        }

        /// <summary>
        /// Indicates what type of light this entity is producing.
        /// </summary>
        /// <value>The light model.</value>
        public LightModel LightModel {
            get { return Blueprint.LightModel; }
        }

        #endregion

        #region Information

        public string Tag {
            get {
                return _tag;
            }
            set {
                _tag = value;
                MarkUpdated (UpdateMarker.Tag);
            }
        }

        public EntityStatus Status {
            get {
                return _status;
            }
        }

        public virtual ushort TooltipId {
            get {
                return Blueprint.TooltipId;
            }
        }

        public ushort TagId {
            get {
                return Blueprint.TagId;
            }
        }

        /// <summary>
        /// Used by tooltips.
        /// </summary>
        public virtual string Description {
            get {
                return Localization.Instance [Name];
            }
        }

        /// <summary>
        /// Used by tooltips.
        /// </summary>
        /// <value>The information.</value>
        public virtual IList<string> GetInformation (Player player) {
            return NO_INFORMATION;
        }

        /// <summary>
        /// Gets the usage information for this entity.
        /// </summary>
        /// <returns>The usage information.</returns>
        /// <param name="player">Player.</param>
        public IList<string> GetUsage (Player player) {
            return Blueprint.Interaction != null ? Blueprint.Interaction.GetUsage (this, player) : NO_INFORMATION;
        }

        public IReadOnlyList<ProgressInfo> ProgressInfos {
            get;
            private set;
        }

        #endregion

        #region Location and Bounding

        /// <summary>
        /// Location of this entity in the world.
        /// </summary>
        public virtual Vect2d Location {
            get {
                return _location;
            }
            set {
                _location = value;
            }
        }

        [GameData (Remote = true, Key = "Orientation")]
        public Direction Orientation {
            get;
            set;
        }

        /// <summary>
        /// Returns the center of the entity's bounding box.
        /// </summary>
        public Vect2d Center {
            get {
                return Location + BoundingOffset + new Vect2f (BoundingSize.X / 2, BoundingSize.Y / 2);
            }
        }

        /// <summary>
        /// Entity's bounding box.
        /// </summary>
        public virtual Rect2f Bounding {
            get {
                return new Rect2f ((Vect2f)Location + BoundingOffset, BoundingSize);
            }
        }

        protected virtual Vect2f BoundingOffset {
            get {
                return Vect2f.ZERO;
            }
        }

        public abstract Vect2f BoundingSize {
            get;
        }

        public Rect2f CollisionBox {
            get {
                return new Rect2f (
                    new Vect2f (Bounding.Center.X - 0.1f, Bounding.Center.Y - 0.1f),
                    CollisionSize
                );
            }
        }

        public Vect2f CollisionSize {
            get {
                return new Vect2f (0.2f, 0.2f);
            }
        }

        #endregion

        #endregion

        #region Fields

        [GameData (Remote = true, Key = "Location")]
        Vect2d _location;
        [GameData (Remote = true, Key = "IsVisible")]
        bool _isVisible;

        [GameData (Remote = true, Persists = false, Key = "Status")]
        EntityStatus _status = EntityStatus.NONE;
        Dictionary<string, EntityStatus> _currentstatus = new Dictionary<string, EntityStatus> ();

        [GameData (Remote = true, Key = "Tag")]
        string _tag;

        #endregion

        #region Constructor

        protected Entity (IWorldAccess access, string name, Blueprint blueprint)
            : base (access, name) {
            IsVisible = true;
            IsTickable = blueprint.IsTickable;
            Orientation = Direction.East;
            ProgressInfos = blueprint.GetProgressInfoList (this);
            ResetStatus ();
        }

        #endregion

        #region Serialization

        public Entity (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        protected override void OnCommissioned () {
            base.OnCommissioned ();
            ProgressInfos = Blueprint.GetProgressInfoList (this);
        }

        #endregion

        #region Status

        void ResetStatus () {
            EntityStatus status = _currentstatus.Values.OrderByDescending (p => p.Level).FirstOrDefault ();
            _status = status ?? EntityStatus.NONE;
            MarkUpdated (UpdateMarker.Status);
        }

        public void ClearStatus (string category) {
            if (_currentstatus.ContainsKey (category)) {
                _currentstatus.Remove (category);
                ResetStatus ();
            }
        }

        public void SetStatus (EntityStatus status) {
            if (!_currentstatus.ContainsKey (status.Category) || !_currentstatus [status.Category].Equals (status)) {
                _currentstatus [status.Category] = status;
                ResetStatus ();
            }
        }

        #endregion

        #region Rendering

        public RenderFlags RenderFlags {
            get;
            set;
        }

        public virtual int RenderHash {
            get {
                return Serial.GetHashCode ();
            }
        }

        public virtual ushort RenderType {
            get {
                return Blueprint.RenderId;
            }
        }

        public virtual RenderHint RenderHint {
            get {
                return RenderHint.Static;
            }
        }

        public UILayer UILayer {
            get {
                return Blueprint.UILayer;
            }
        }

        bool _renderChanged = true;

        /// <summary>
        /// Gets a value indicating whether the rendering for this <see cref="BSimulator.Game.Entities.Entity"/> has changed.
        /// </summary>
        /// <remarks>Used to re-draw heightmaps.</remarks>
        /// <value><c>true</c> if rendering changed; otherwise, <c>false</c>.</value>
        public bool RenderChanged {
            get {
                return _renderChanged;
            }
            set {
                // Also mark the map as dirty, if we are static and renderChanged was actually modified.
                if (_renderChanged != value && RenderHint == RenderHint.Static) {
                    Access.Controller.SignalMapDirtied (Location, string.Format ("Render changed for entity: {0}", ToString ()));
                }
                _renderChanged = value;
            }
        }

        #endregion

        #region Networking

        /// <summary>
        /// Called when an update pulse is received for this entity through the given player.
        /// </summary>
        /// <param name="player">Player.</param>
        public virtual void OnUpdatePulse (Player player) {
            ScheduleUpdate (UpdateMarker.Progress);
        }

        public override void HandleNetworkUpdate (IPacketMarked packet) {
            if ((PacketId)packet.Id == PacketId.EntityTag) {
                Payload payload = ((PacketUpdatePayload)packet).Payload;
                Tag = payload.HasFragment (0) ? payload.GetValue<string> (0) : string.Empty;
            } else if (packet.Marker == UpdateMarker.Status) {
                EntityStatus.StatusSymbol prev = _status.Symbol;
                _status = ((PacketUpdateStream)packet).DeserializeContent<EntityStatus> (Access);
                RenderChanged = prev != _status.Symbol;
            } else if (packet.Marker == UpdateMarker.Progress) {
                float[] fractions = ((PacketUpdatePayload)packet).Payload.GetValue<float[]> (0);
                for (int i = 0; i < fractions.Length; i++) {
                    ProgressInfos [i].Fraction = fractions [i];
                }
            } else if (packet.Marker == UpdateMarker.Visibility) {
                _isVisible = ((PacketUpdatePayload)packet).Payload.GetValue<bool> (0);
            }
        }

        public override IPacketMarked GetUpdatePacket () {

            if (UpdateSchedule.HasFlag (UpdateMarker.Status)) {
                UnscheduleUpdate (UpdateMarker.Status);
                return new PacketUpdateStream (PacketId.UpdateStream, this, UpdateMarker.Status, _status);
            }
            if (UpdateSchedule.HasFlag (UpdateMarker.Tag)) {
                UnscheduleUpdate (UpdateMarker.Tag);
                if (string.IsNullOrEmpty (Tag)) {
                    return new PacketUpdatePayload (PacketId.EntityTag, this, UpdateMarker.Tag);
                } else {
                    return new PacketUpdatePayload (PacketId.EntityTag, this, UpdateMarker.Tag, Tag);
                }
            }
            if (UpdateSchedule.HasFlag (UpdateMarker.Progress)) {
                UnscheduleUpdate (UpdateMarker.Progress);
                if (ProgressInfos.Count > 0) {
                    float[] fractions = new float[ProgressInfos.Count];
                    for (int i = 0; i < ProgressInfos.Count; i++) {
                        fractions [i] = ProgressInfos [i].CalculateFraction ();
                    }
                    return new PacketUpdatePayload (PacketId.UpdatePayload, this, UpdateMarker.Progress, fractions);
                }
            }
            if (UpdateSchedule.HasFlag (UpdateMarker.Visibility)) {
                UnscheduleUpdate (UpdateMarker.Visibility);
                return new PacketUpdatePayload (PacketId.UpdatePayload, this, UpdateMarker.Visibility, _isVisible);
            }

            return null;
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Raises the placed event. Called when the entity is placed into the world. Entity is not actually added yet to the world!
        /// </summary>
        /// <param name="control">Control state active when placed.</param>
        public virtual void OnPlaced () {
        }

        /// <summary>
        /// Raises the neighbour change event. Called when a neighbouring cell entity changes.
        /// </summary>
        /// <param name="entity">Entity associated with the changed neighbouring tile. Null if removed.</param>
        public virtual void OnNeighbourChange (Entity entity) {
        }

        #endregion

        #region Interaction

        public void OnPlayerCollision (Player player) {
            if (Blueprint.Interaction != null) {
                Blueprint.Interaction.OnPlayerCollision (this, player);
            }
        }

        public bool OnActivationTick (Player player, int duration, ControlState control) {
            if (Blueprint.Interaction == null || !Blueprint.Interaction.IsActivated (this, player, control, duration)) {
                return false;
            }

            //if (player.CanReach (Location)) {
            if (Blueprint.Interaction != null) {
                Blueprint.Interaction.OnActivated (this, player, control);
            }
            //}
            return true;
        }

        /// <summary>
        /// Raises the clicked event. Called when the entity is clicked on the map or in lists.
        /// </summary>
        /// <param name='player'>
        /// Player who clicked.
        /// </param>
        /// <param name="control">Control state active when clicked.</param>
        public void OnClicked (Player player, ControlState control) {
            //if (player.CanReach (Location)) {
            if (Blueprint.Interaction != null) {
                Blueprint.Interaction.OnInteracted (this, player, control);
            }
            //}
        }

        public virtual void Break (Player player) {
        }

        /// <summary>
        /// Called to break the entity.
        /// </summary>
        /// <remarks>Called to perform entity breaking.</remarks>
        /// <param name="player">Player.</param>
        /// <returns>true, if the entity was successfully broken, false otherwise.</returns>
        public virtual bool CanBreak (Player player) {
            return false;
        }

        /// <summary>
        /// Gets the gating identifier. Can be used for fence gates.
        /// </summary>
        /// <value>The gating identifier.</value>
        public virtual ulong GatingId {
            get { return Blueprint.Serial; }
        }

        /// <summary>
        /// Indicates whether this entity blocks movement for the other entity (i.e. the two entities cannot occupy the same space).
        /// </summary>
        /// <returns><c>true</c>, if movement was blocked, <c>false</c> otherwise.</returns>
        /// <param name="other">Another entity</param>
        /// <param name="mirror">Reverses the angle of approach as given by from.</param>
        public virtual bool AllowsPassage (Entity other, Vect2f from, bool mirror) {
            return true;
        }

        #endregion

        public override string ToString () {
            return string.Format ("[Entity: Name={0}, Serial={1}]", Name, Serial);
        }
    }
}
