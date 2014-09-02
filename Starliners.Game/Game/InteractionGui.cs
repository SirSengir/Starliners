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
using BLibrary.Resources;
using BLibrary.Util;

namespace Starliners.Game {

    [Serializable]
    public sealed class InteractionGui : InteractionHandler {
        #region Constants

        static readonly IList<string> USAGE_INFORMATION = new List<string> () {
            string.Format ("{0} {1}", Constants.CONTROL_SCHEME_GUI_OPEN, Localization.Instance ["tt_gui_open"])
        };

        #endregion

        ushort _guiId;
        int _activationDelay;

        public InteractionGui (ushort guiId)
            : this (guiId, InteractionHandler.ACTIVATION_DELAY_DEFAULT) {
        }

        public InteractionGui (ushort guiId, int activationDelay) {
            _guiId = guiId;
            _activationDelay = activationDelay;
        }

        public override int GetEstimatedActivation (Entity entity, Player player) {
            return _activationDelay;
        }

        public override bool CanActivate (Entity entity, Player player, ControlState control) {
            return control.HasFlag (ControlState.MouseRight) && entity.CanBreak (player);
        }

        public override bool IsActivated (Entity entity, Player player, ControlState control, int duration) {
            return duration >= _activationDelay;
        }

        public override void OnPlayerCollision (Entity entity, Player player) {
        }

        public override void OnActivated (Entity entity, Player player, ControlState control) {
            // The entity broke, handle that.
            if (control.HasFlag (ControlState.MouseRight)) {
                entity.Break (player);
            }
        }

        public override void OnInteracted (Entity entity, Player player, ControlState control) {
            if (control.HasFlag (ControlState.MouseLeft)) {
                player.SelectedEntity = entity;
                player.OpenGUI (_guiId, entity);
            }
        }

        public override IList<string> GetUsage (Entity entity, Player player) {
            return USAGE_INFORMATION;
        }
    }
}

