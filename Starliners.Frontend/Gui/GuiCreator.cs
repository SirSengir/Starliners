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
using BLibrary.Gui;
using BLibrary.Gui.Data;
using BLibrary.Gui.Tooltips;
using Starliners.Game;
using Starliners.Gui.Interface;

namespace Starliners.Gui {

    sealed class GuiCreator : IGuiCreator {

        public GuiWindow GetGui (Container container) {
            switch ((GuiIds)container.GuiId) {
                case GuiIds.Battle:
                    return new GuiBattle (container.ContainerId);
                case GuiIds.BattleReport:
                    return new GuiBattleReport (container.ContainerId);
                case GuiIds.Chatline:
                    return new GuiChatline (container.ContainerId);
                case GuiIds.Elimination:
                    return new GuiElimination (container.ContainerId);
                case GuiIds.Fleet:
                    return new GuiFleet (container.ContainerId);
                case GuiIds.History:
                    return new GuiHistory (container.ContainerId);
                case GuiIds.Hud:
                    return new GuiHud (container.ContainerId);
                case GuiIds.Notifications:
                    return new GuiNotifications (container.ContainerId);
                case GuiIds.Planet:
                    return new GuiPlanet (container.ContainerId);
                default:
                    return null;
            }
        }

        public Tooltip GetTooltip (ushort id, params object[] args) {
            switch ((TooltipIds)id) {
                case TooltipIds.Describable:
                    return new TooltipDescribable ((IDescribable)args [0]);
                default:
                    return null;
            }
        }
    }
}

