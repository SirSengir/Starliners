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

﻿using BLibrary.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BLibrary.Util;
using Starliners.Graphics;

namespace Starliners.Game {

    /// <summary>
    /// Blueprints define immutable entity behaviour and properties.
    /// </summary>
    [Serializable]
    public class Blueprint : Asset {
        #region Properties

        [GameData (Remote = true, Key = "RenderType")]
        public ushort RenderId {
            get;
            set;
        }

        [GameData (Remote = true, Key = "UILayer")]
        public UILayer UILayer {
            get;
            set;
        }

        [GameData (Remote = true, Key = "Interaction")]
        public InteractionHandler Interaction {
            get;
            set;
        }

        /// <summary>
        /// Id for the tooltip type to use. Defaults to the standard tooltip for entities.
        /// </summary>
        /// <value>The tooltip identifier.</value>
        [GameData (Remote = true, Key = "TooltipId")]
        public ushort TooltipId {
            get;
            set;
        }

        /// <summary>
        /// Id for the tag type to use. Defaults to the standard tooltip for entities.
        /// </summary>
        /// <value>The tag identifier.</value>
        [GameData (Remote = true, Key = "TagId")]
        public ushort TagId {
            get;
            set;
        }

        [GameData (Remote = true, Key = "EntityIdent")]
        public string EntityIdent {
            get;
            protected set;
        }

        public Type EntityType {
            get;
            protected set;
        }

        /// <summary>
        /// Indicates whether the entity needs to be seen even if other entities draw on top of it.
        /// </summary>
        /// <value><c>true</c> if this instance needs to be unhidden; otherwise, <c>false</c>.</value>
        [GameData (Remote = true, Key = "Unhiding")]
        public UnhidingBehaviour Unhiding {
            get;
            set;
        }

        [GameData (Remote = true, Key = "Luminosity")]
        public byte Luminosity {
            get;
            set;
        }

        [GameData (Remote = true, Key = "LightModel")]
        public LightModel LightModel {
            get;
            set;
        }

        [GameData (Remote = true, Key = "Category")]
        public ObjectCategory Category {
            get;
            private set;
        }

        /// <summary>
        /// Attributes of the entity generated from this blueprint.
        /// </summary>
        public ISet<string> Attributes {
            get {
                return _attributes;
            }
        }

        /// <summary>
        /// IdObject.Name of the help page for entities created from this blueprint. Can be empty.
        /// </summary>
        public string HelpLink {
            get;
            set;
        }

        [GameData (Remote = true, Key = "ProgressInfos")]
        public IReadOnlyList<ProgressInfo> ProgressInfos {
            get { return _progressInfos; }
            set { _progressInfos = value; }
        }

        #endregion

        #region Fields

        [GameData (Remote = true, Key = "Attributes")]
        HashSet<string> _attributes = new HashSet<string> ();
        IReadOnlyList<ProgressInfo> _progressInfos = new List<ProgressInfo> ();

        #endregion

        #region Constructor

        public Blueprint (IWorldAccess access, string name, AssetKeyMap keyMap, ObjectCategory category, params string[] attributes)
            : base (access, name, keyMap) {

            EntityIdent = name;

            //TooltipId = GameAccess.Game.IdTooltipEntity;
            //TagId = GameAccess.Game.IdTagEntity;
            IsTickable = true;
            LightModel = LightModel.Static;
            Category = category;

            foreach (string attribute in attributes) {
                Attributes.Add (attribute);
            }

        }

        #endregion

        #region Serialization

        public Blueprint (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        #region Entity Creation

        public virtual Entity CreateEntity (Player owner, Vect2f coordinates) {
            if (EntityType == null) {
                throw new SystemException ("Cannot create a entity for " + Name);
            }

            Entity entity = (Entity)Activator.CreateInstance (EntityType, Access, this, owner);
            entity.Location = coordinates;
            return entity;
        }

        #endregion

        public IReadOnlyList<ProgressInfo> GetProgressInfoList (Entity entity) {
            if (ProgressInfos.Count <= 0) {
                return ProgressInfos;
            }

            List<ProgressInfo> infos = new List<ProgressInfo> ();
            for (int i = 0; i < ProgressInfos.Count; i++) {
                infos.Add (ProgressInfos [i].Copy (entity));
            }

            return infos;
        }
    }
}
