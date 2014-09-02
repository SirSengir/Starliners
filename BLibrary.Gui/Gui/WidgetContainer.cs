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

﻿using BLibrary.Gui.Data;
using System.Collections.Generic;
using BLibrary.Util;


namespace BLibrary.Gui {

    public abstract class WidgetContainer : GuiElement {
        #region Properties

        public override WidgetContainer Window {
            get {
                return this;
            }
        }

        public bool IsUpdated {
            get;
            private set;
        }

        public bool BeingDragged {
            get;
            set;
        }

        #endregion

        #region Fields

        long _lastUpdate;
        List<IUpdateIndicator> _subscribed = new List<IUpdateIndicator> ();

        #endregion

        public override void Update () {
            base.Update ();

            IsUpdated = false;
            for (int i = 0; i < _subscribed.Count; i++) {
                if (_subscribed [i].LastUpdated <= _lastUpdate) {
                    continue;
                }

                _lastUpdate = _subscribed [i].LastUpdated;
                IsUpdated = true;
                // We do not break here, so that we get the most current update tick.
            }
            if (IsUpdated) {
                Refresh ();
            }
        }

        protected void Subscribe (IUpdateIndicator indicator) {
            _subscribed.Add (indicator);
        }

        /// <summary>
        /// Called when data has changed, but no complete rebuild is necessary. (Refill tables here.)
        /// </summary>
        protected virtual void Refresh () {
        }

        public virtual void OnRendered () {
        }

        public virtual bool DoAction (string key, params object[] args) {
            return false;
        }
    }
}
