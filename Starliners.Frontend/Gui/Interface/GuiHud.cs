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
using BLibrary.Util;
using BLibrary.Gui;
using BLibrary.Gui.Widgets;
using System.Collections.Generic;
using System.Linq;
using Starliners.Gui;
using Starliners.Gui.Widgets;
using Starliners.Game;

namespace Starliners.Gui.Interface {
    sealed class GuiHud : GuiRemote {
        #region Constants

        static readonly Vect2i WINDOW_SIZE = Constants.HUD_SIZE_TOP;
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_hud", WINDOW_SIZE, Positioning.UpperMiddle, false) {
            Group = ScreenGroup.Hud,
            Style = "hud"
        };
        const float NOTIFICATION_SPEED = 0.1f;
        const float NOTIFICATION_AMPLITUDE = 48;

        #endregion

        #region Classes

        sealed class FloatingNotification {
            public readonly Vect2i Start;
            public readonly Label Widget;
            public int Age;
            public readonly int MaxAge;
            public readonly int Linger;
            public float Way = 0.01f;

            public FloatingNotification (Label widget, int maxAge, int linger) {
                Start = widget.PositionRelative;
                Widget = widget;
                MaxAge = maxAge;
                Linger = linger;
            }
        }

        #endregion

        #region Member

        //long _lastTransaction = -1;
        List<FloatingNotification> _floaters = new List<FloatingNotification> ();
        List<FloatingNotification> _removal = new List<FloatingNotification> ();

        #endregion

        #region Constructor

        long _lastTransaction = -1;
        Vect2i _anchorTransactions;
        Vect2i _anchorScoring;

        public GuiHud (int containerId)
            : base (WINDOW_SETTING, containerId) {

            IsCloseable = false;
            _lastTransaction = GameAccess.Interface.Local.Clock.Ticks;
        }

        #endregion

        public override void Update () {
            base.Update ();

            _removal.Clear ();
            // Limit the max amount of floaters
            if (_floaters.Count > 3) {
                foreach (FloatingNotification floater in _floaters.OrderByDescending(p => p.Age).Take(_floaters.Count - 3)) {
                    floater.Age = floater.MaxAge;
                }
            }
            for (int i = 0; i < _floaters.Count; i++) {
                FloatingNotification floater = _floaters [i];
                floater.Age++;
                // Mark old floaters for removal
                if (floater.Age > floater.MaxAge) {
                    _removal.Add (floater);
                    continue;
                }

                // Increase the way progressed. 
                floater.Way = NOTIFICATION_AMPLITUDE * (float)Math.Sqrt ((float)floater.Age / floater.MaxAge);
                floater.Widget.PositionRelative = floater.Start + new Vect2i (0, floater.Way); 
            }
            // Remove dead floaters
            for (int i = 0; i < _removal.Count; i++) {
                _floaters.Remove (_removal [i]);
                RemoveWidget (_removal [i].Widget);
            }
        }

        protected override void Regenerate () {
            base.Regenerate ();

            int menuItems = 4;
            Vect2i widgetSize = new Vect2i ((float)(WINDOW_SIZE.X - 32) / menuItems, 48);
            Vect2i widgetSpacing = new Vect2i (widgetSize.X, 0);

            Vect2i start = new Vect2i ((Size.X - menuItems * widgetSize.X) / 2, 0);

            // Faction thingy
            Faction faction = GameAccess.Interface.ThePlayer.MainFaction;
            Grouping grouped = new Grouping (start, widgetSize) {
                Backgrounds = UIProvider.Styles ["hud"].ButtonStyle.CreateBackgrounds (),
                BackgroundStates = GuiWindow.BG_STATES_SENSITIVE,
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);
            grouped.AddWidget (new IconBlazon (Vect2i.ZERO, new Vect2i (32, 32), faction));
            grouped.AddWidget (new Label (new Vect2i (32 + 8, 0), new Vect2i (widgetSize.X - 32 - 3 * 8, 32), faction.FullName) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            });

            // Funds
            start += widgetSpacing;
            _anchorTransactions = start;
            grouped = new Grouping (start, widgetSize) {
                Backgrounds = UIProvider.Styles ["hud"].ButtonStyle.CreateBackgrounds (),
                BackgroundStates = GuiWindow.BG_STATES_SENSITIVE,
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);
            grouped.AddWidget (new Label (Vect2i.ZERO, new Vect2i (widgetSize.X, 32), new DataReference<decimal> (this, KeysFragments.PLAYER_FUNDS) { Template = "$ {0}" }) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            });

            // Score
            start += widgetSpacing;
            _anchorScoring = start;
            grouped = new Grouping (start, widgetSize) {
                Backgrounds = UIProvider.Styles ["hud"].ButtonStyle.CreateBackgrounds (),
                BackgroundStates = GuiWindow.BG_STATES_SENSITIVE,
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            };
            AddWidget (grouped);
            grouped.AddWidget (new Label (Vect2i.ZERO, new Vect2i (widgetSize.X, 32), new DataReference<int> (this, KeysFragments.PLAYER_SCORE) { Template = "≠ {0}" }) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Center
            });

            start += widgetSpacing;
            AddWidget (new MenuClock (start, widgetSize, "clock"));
        }

        protected override void Refresh () {
            base.Refresh ();

            IList<Bookkeeping.TransactionRecord> transactions = DataProvider.GetValue<List<Bookkeeping.TransactionRecord>> (KeysFragments.PLAYER_FUNDS_TRANSACTIONS).Where (p => p.TimeStamp > _lastTransaction).ToList ();
            if (transactions.Count > 0) {
                _lastTransaction = transactions.Max (p => p.TimeStamp);
                foreach (Bookkeeping.TransactionRecord record in transactions) {
                    Label note = new Label (new Vect2i (_anchorTransactions.X, 48), new Vect2i ((WINDOW_SIZE.X / 90) * 15, 40),
                                     string.Format ("§{0}?+b§$ {1:n}", record.Amount < 0 ? Colour.Crimson.ToString ("#") : Colour.LimeGreen.ToString ("#"), record.Amount)) {
                        Tinting = NoTinting.INSTANCE,
                        AlignmentH = Alignment.Center,
                        AlignmentV = Alignment.Center
                    };
                    AddWidget (note);
                    _floaters.Add (new FloatingNotification (note, 400, 100));
                }
            }

            IList<ScoreKeeper.TransactionRecord> scorings = DataProvider.GetValue<List<ScoreKeeper.TransactionRecord>> (KeysFragments.PLAYER_SCORE_TRANSACTIONS).Where (p => p.TimeStamp > _lastTransaction).ToList ();
            if (scorings.Count > 0) {
                _lastTransaction = scorings.Max (p => p.TimeStamp);
                foreach (ScoreKeeper.TransactionRecord record in scorings) {
                    Label note = new Label (new Vect2i (_anchorScoring.X, 48), new Vect2i ((WINDOW_SIZE.X / 90) * 15, 40),
                                     string.Format ("§{0}?+b§$ {1:n}", Colour.Yellow.ToString ("#"), record.Amount)) {
                        Tinting = NoTinting.INSTANCE,
                        AlignmentH = Alignment.Center,
                        AlignmentV = Alignment.Center
                    };
                    AddWidget (note);
                    _floaters.Add (new FloatingNotification (note, 400, 100));
                }
            }
        }
    }
}

