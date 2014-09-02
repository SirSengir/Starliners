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
using System.Linq;
using System.Collections.Generic;
using BLibrary.Serialization;
using System.Runtime.Serialization;

namespace Starliners.Game {
    [Serializable]
    sealed class HistoryTracker : StateObject {
        #region Constants

        const string TRACKER_NAME = "HistoryTracker";

        #endregion

        public IReadOnlyList<IIncident> Incidents {
            get {
                return _incidents;
            }
        }

        [GameData (Remote = true)]
        List<IIncident> _incidents = new List<IIncident> ();

        public HistoryTracker (IWorldAccess access)
            : base (access, TRACKER_NAME) {
        }

        #region Serialization

        public HistoryTracker (SerializationInfo info, StreamingContext context)
            : base (info, context) {
        }

        #endregion

        public T RequireIncident<T> (ulong serial) {
            return (T)_incidents.Where (p => p.Serial == serial).First ();
        }

        public void RegisterIncident (IIncident incident) {
            _incidents.Add (incident);
        }

        public static HistoryTracker GetForWorld (IWorldAccess access) {
            return access.States.Values.OfType<HistoryTracker> ().Where (p => string.Equals (p.Name, TRACKER_NAME)).First ();
        }
    }
}

