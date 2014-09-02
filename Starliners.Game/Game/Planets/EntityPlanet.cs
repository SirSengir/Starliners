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
using System.Runtime.Serialization;
using BLibrary.Util;
using BLibrary.Serialization;
using BLibrary.Graphics;
using Starliners.Graphics;
using System.Collections.Generic;
using BLibrary.Network;
using Starliners.Network;
using Starliners.Game.Forces;

namespace Starliners.Game.Planets {

    [Serializable]
    public sealed class EntityPlanet : Entity, ISpriteDeclarant, IRenderableEntity, INavPoint {
        #region Constants

        static readonly Vect2f BOUNDING_SIZE = new Vect2f (2, 2);
        static readonly Vect2f BOUNDING_OFFSET = new Vect2f (-1, -1);

        #endregion

        #region Properties

        public override Blueprint Blueprint {
            get {
                return _blueprint;
            }
        }

        public override Vect2f BoundingSize {
            get {
                return BOUNDING_SIZE;
            }
        }

        protected override Vect2f BoundingOffset {
            get {
                return BOUNDING_OFFSET;
            }
        }

        public override string Description {
            get {
                return PlanetData.Name;
            }
        }

        public override IList<string> GetInformation (Player player) {
            return PlanetData.Levy.GetInformation (player);
        }

        public override Vect2d Location {
            get {
                return PlanetData.Location;
            }
            set {
                PlanetData.Location = value;
            }
        }

        public override Faction Owner {
            get {
                return PlanetData.Owner;
            }
        }

        public Orbit Orbit {
            get {
                return PlanetData.Orbit;
            }
        }

        [GameData (Remote = true, Key = "PlanetData")]
        public Planet PlanetData {
            get;
            private set;
        }

        public override long LastUpdated {
            get {
                return PlanetData.LastUpdated > base.LastUpdated ? PlanetData.LastUpdated : base.LastUpdated;
            }
        }

        #endregion

        [GameData (Remote = true, Key = "Blueprint")]
        Blueprint _blueprint;
        uint[] _icons;

        #region Constructor

        public EntityPlanet (IWorldAccess access, string name, Blueprint blueprint, Planet data)
            : base (access, name, blueprint) {
            _blueprint = blueprint;
            PlanetData = data;
            Access.Controller.QueueState (PlanetData);
        }

        #endregion

        #region Serialization

        public EntityPlanet (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        /// <summary>
        /// Raises the levy of this planet, creating a new fleet containing it in orbit.
        /// </summary>
        public void RaiseLevy () {
            if (PlanetData.Levy.State != LevyState.Available) {
                return;
            }

            PlanetData.EnterOrbit (PlanetData.Levy.Raise (this));
        }

        public void EnterOrbit (Fleet fleet) {
            PlanetData.EnterOrbit (fleet);
        }

        public void LeaveOrbit (Fleet fleet) {
            PlanetData.LeaveOrbit (fleet);
        }

        #region Rendering

        public void RegisterIcons (IIconRegister register) {
            _icons = register.RegisterIcon (PlanetData.Skin);
        }

        public int GetCurrentReel (ModelPart part) {
            return 0;
        }

        public ModelReel[] GetReels (ModelPart part) {
            return new ModelReel[] { new ModelReel (new IconLayer (_icons [0]), new IconLayer (_icons [1])) { Anchor = Anchor.Center } };
        }

        public bool HasPart (ModelPart part) {
            return part == ModelPart.Sprite;
        }

        #endregion

    }
}

