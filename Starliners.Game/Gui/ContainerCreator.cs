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
using BLibrary.Gui.Data;
using Starliners.Game;
using Starliners.Gui.Interface;
using Starliners.Game.Forces;
using Starliners.Game.Planets;

namespace Starliners.Gui {

    public sealed class ContainerCreator : IContainerCreator {
        public Container GetContainer (ushort id, Player player, params object[] args) {
            switch ((GuiIds)id) {
                case GuiIds.Battle:
                    return new ContainerBattle (id, player, (Battle)args [0]);
                case GuiIds.BattleReport:
                    return new ContainerBattleReport (id, player, (BattleReport)args [0]);
                case GuiIds.Chatline:
                    return new ContainerChatline (id, player);
                case GuiIds.Elimination:
                    return new ContainerElimination (id, player);
                case GuiIds.Fleet:
                    return new ContainerFleet (id, player, args [0] is Fleet ? (Fleet)args [0] : ((EntityFleet)args [0]).Contained);
                case GuiIds.History:
                    return new ContainerHistory (id, player);
                case GuiIds.Hud:
                    return new ContainerHud (player);
                case GuiIds.Notifications:
                    return new ContainerNotifications (id, player);
                case GuiIds.Planet:
                    return new ContainerPlanet (id, player, (EntityPlanet)args [0]);
                default:
                    return null;
            }
        }
    }
}

