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
using BLibrary.Util;


namespace Starliners.Game.Notifications {

    public sealed class NotificationManager {
        static NotificationManager _instance;

        public static NotificationManager Instance {
            get {
                if (_instance == null)
                    _instance = new NotificationManager ();
                return _instance;
            }
        }

        #region Handling

        Dictionary<string, NotificationHandling> _defaultHandling = new Dictionary<string, NotificationHandling> ();

        /// <summary>
        /// Sets the default notification handling for the given category.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="handling">Handling.</param>
        public void SetDefaultHandling (string category, NotificationHandling handling) {
            _defaultHandling [Utils.BuildName (NotificationCategory.NAME_PREFIX, category)] = handling;
        }

        /// <summary>
        /// Gets default notification handling for the given category
        /// </summary>
        /// <returns>The default handling.</returns>
        /// <param name="category">Category.</param>
        internal NotificationHandling GetDefaultHandling (NotificationCategory category) {
            if (!_defaultHandling.ContainsKey (category.Name)) {
                return NotificationHandling.Log | NotificationHandling.Notify;
            }

            return _defaultHandling [category.Name];
        }

        #endregion

        /// <summary>
        /// Sends an actionless notification to the given player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public void Notify (Player player, string category, TextComposition compact, params TextComponent[] verbose) {
            Notify (player, new Notification (player, NotificationCategory.GetCategoryForWorld (player.Access, category), new AutoActionNone (), compact, new TextComposition (verbose)));
        }

        public void Notify (INotifiable notifiable, string category, TextComposition compact, params TextComponent[] verbose) {
            foreach (Player player in notifiable.GetListeningPlayers()) {
                Notify (player, category, compact, verbose);
            }
        }

        public void Notify (Player player, string category, TextComponent compact, params TextComponent[] args) {
            Notify (player, category, new TextComposition (CombineTextComponents (compact, args)));
        }

        /// <summary>
        /// Sends a notification to the given player with an action to open a gui with the given argument.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="guiId"></param>
        /// <param name="guiarg"></param>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public void Notify (Player player, string category, ushort guiId, object guiarg, TextComposition compact, params TextComponent[] verbose) {
            Notify (player, category, new AutoActionGui (guiId, guiarg), compact, new TextComposition (verbose));
        }

        public void Notify (INotifiable notifiable, string category, ushort guiId, object guiarg, TextComposition compact, params TextComponent[] verbose) {
            foreach (Player player in notifiable.GetListeningPlayers()) {
                Notify (player, category, guiId, guiarg, compact, verbose);
            }
        }

        public void Notify (Player player, string category, ushort guiId, object guiarg, TextComponent compact, params TextComponent[] args) {
            Notify (player, category, guiId, guiarg, new TextComposition (CombineTextComponents (compact, args)));
        }

        /// <summary>
        /// Sends a notification to the given player with an action to open a gui with the given arguments.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="guiId"></param>
        /// <param name="guiargs"></param>
        /// <param name="text"></param>
        /// <param name="args"></param>
        public void Notify (Player player, string category, ushort guiId, object[] guiargs, TextComposition compact, params TextComponent[] verbose) {
            Notify (player, category, new AutoActionGui (guiId, guiargs), compact, new TextComposition (verbose));
        }

        public void Notify (Player player, string category, ushort guiId, object[] guiargs, TextComponent compact, params TextComponent[] args) {
            Notify (player, category, guiId, guiargs, new TextComposition (CombineTextComponents (compact, args)));
        }

        /// <summary>
        /// Sends a notification with the given action to the given player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="action"></param>
        /// <param name="text"></param>
        /// <param name="args"></param>
        void Notify (Player player, string category, AutoAction action, TextComposition compact, TextComposition verbose) {
            Notify (player, new Notification (player, NotificationCategory.GetCategoryForWorld (player.Access, category), action, compact, verbose));
        }

        void Notify (Player player, Notification notification) {
            player.PostNotification (notification);
        }

        AutoAction CreateGuiAction (ushort guiId, params object[] args) {
            return new AutoActionGui (guiId, args);
        }

        TextComponent[] CombineTextComponents (TextComponent format, params TextComponent[] args) {
            TextComponent[] combined = new TextComponent[args.Length + 1];
            combined [0] = format;
            Array.Copy (args, 0, combined, 1, args.Length);
            return combined;
        }
    }
}
