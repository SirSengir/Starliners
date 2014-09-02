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

﻿namespace Starliners.Network {

    public enum PacketId : byte {
        Request = 1,
        Ack = 2,
        Login = 3,
        ServerInfo = 4,
        FactionSelect = 5,
        PlayerMarked = 6,
        Calendar = 7,
        Signal = 8,
        Sound = 9,

        Chat = 10,
        Content = 11,
        World = 12,
        ViewSet = 13,

        MapUpdate = 20,
        MapTargeted = 21,
        MapClick = 22,

        GuiOpen = 30,
        GuiClosed = 31,
        GuiData = 32,
        GuiAction = 33,
        GuiPickup = 34,

        EntityTargeted = 40,
        EntityClick = 41,
        EntityStatus = 42,
        EntityTag = 43,
        EntitySelected = 44,
        /// <summary>
        /// Sent by the interface to request an out-of-sequence update.
        /// </summary>
        EntityPulse = 45,

        UpdatePayload = 50,
        UpdateStream = 51,
        UpdateRemoved = 52,

        ParticleSpawn = 60
    }
}
