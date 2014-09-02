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
using BLibrary.Util;
using BLibrary.Gui.Widgets;
using Starliners.Gui.Widgets;
using BLibrary.Resources;
using BLibrary.Gui.Backgrounds;
using Starliners.Game;

namespace Starliners.Gui.Interface {
    sealed class GuiElimination : GuiRemote {
        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (960, 640);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_elimination", WINDOW_SIZE, Positioning.Centered, true);

        const string BUTTON_QUIT = "button.quit";
        const string BUTTON_RETIRE = "button.retire";

        const string TAB_INFO = "tab.info";
        const string TAB_STATISTICS = "tab.statistics";

        #endregion

        Table _tblScore;
        Table _tblStatistics;

        public GuiElimination (int containerId)
            : base (WINDOW_SETTING, containerId) {

            AddControlOption (new TabOption (TAB_INFO, "info"));
            AddControlOption (new TabOption (TAB_STATISTICS, "help"));

        }

        protected override void Regenerate () {
            base.Regenerate ();

            AddHeader (WindowButton.None, Localization.Instance ["gui_eliminated"]);
            AddWidget (new Canvas (CornerTopLeft, new Vect2i (960, 288), "banner0"));
            AddWidget (new IconBlazon (CornerTopLeft + new Vect2i (UIProvider.Margin.X, 288 - UIProvider.Margin.Y - 160), new Vect2i (160, 160), GameAccess.Interface.Local.RequireState<Faction> (DataProvider.GetValue<ulong> (KeysFragments.FACTION_SERIAL))));

            Vect2i start = CornerTopLeft + new Vect2i (0, 288 + UIProvider.Margin.Y);
            Vect2i tabsize = new Vect2i (Presets.InnerArea.X, Presets.InnerArea.Y - 288 - 2 * UIProvider.Margin.Y - 40);
            Vect2i tblsize = tabsize;

            // Info tab
            CreateTab (TAB_INFO, start, tabsize);
            Vect2i tabstart = Vect2i.ZERO;

            AddWidget (TAB_INFO, _tblScore = new Table (tabstart, tblsize) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 36
            });

            // Statistics tab
            CreateTab (TAB_STATISTICS, start, tabsize);
            tabstart = Vect2i.ZERO;

            AddWidget (TAB_STATISTICS, _tblStatistics = new Table (tabstart, tblsize) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 36
            });


            int padd = UIProvider.MarginSmall.X;
            Vect2i btnstart = CornerTopLeft + new Vect2i (0, Presets.InnerArea.Y - 40);
            Vect2i btnsize = new Vect2i ((Presets.InnerArea.X - padd) / 2, 40);
            AddWidget (new Button (btnstart, btnsize, BUTTON_QUIT, Localization.Instance ["btn_game_quit_save"]));
            AddWidget (new Button (btnstart + new Vect2i (btnsize.X + UIProvider.MarginSmall.X, 0), btnsize, BUTTON_RETIRE, Localization.Instance ["btn_game_retire_save"]));
        }

        protected override void Refresh () {
            base.Refresh ();

            _tblScore.Reset ();
            ScoreKeeper highscore = DataProvider.GetValue<ScoreKeeper> (KeysFragments.PLAYER_SCORE);
            string category = string.Empty;
            foreach (ScoreKeeper.ScoreSlot slot in ScoreKeeper.INFO_SLOTS) {
                if (!slot.IsDisplayed (highscore)) {
                    continue;
                }
                if (!string.Equals (category, slot.Category)) {
                    _tblScore.AddIntertitle (Localization.Instance [string.Format ("info_{0}", slot.Category)]);
                    category = slot.Category;
                }

                _tblScore.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, slot.Attribute));
                _tblScore.NextColumn ();
                _tblScore.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, slot.ToString (highscore)) { AlignmentH = Alignment.Center });
                _tblScore.NextRow ();
            }

            _tblStatistics.Reset (new PopulatorStatsTable (new DataReference<StatsRecorder<int>> (this, KeysFragments.FACTION_STATISTICS), Faction.INFO_SLOTS));
        }

        public override bool DoAction (string key, params object[] args) {
            switch (key) {
                case BUTTON_QUIT:
                    GameAccess.Interface.Close ();
                    return true;
                case BUTTON_RETIRE:
                    GuiManager.Instance.CloseGuiAll ();
                    GameAccess.Interface.Retire ();
                    return true;
                default:
                    return base.DoAction (key, args);
            }
        }
    }
}

