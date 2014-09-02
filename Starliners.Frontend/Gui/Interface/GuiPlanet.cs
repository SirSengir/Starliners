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
using Starliners.Game;
using System.Collections.Generic;
using BLibrary.Gui.Backgrounds;
using Starliners.Game.Forces;
using Starliners.Game.Planets;

namespace Starliners.Gui.Interface {
    sealed class GuiPlanet : GuiRemote {
        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (512, 512);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_planet", WINDOW_SIZE, Positioning.LowerLeft, true);

        const string TAB_FLEET = "tab.fleet";
        const string TAB_INFO = "tab.info";
        const string TAB_INFRASTRUCTURE = "tab.infrastructure";
        const string TAB_LEVY = "tab.levy";

        #endregion

        #region Fields

        Label _lblLevy;
        Button _btnLevy;

        Table _tblInfo;
        Table _tblLevy;
        Table _tblFleets;
        Table _tblBuildings;

        #endregion

        public GuiPlanet (int containerId)
            : base (WINDOW_SETTING, containerId) {

            AddControlOption (new TabOption (TAB_INFO, "info"));
            AddControlOption (new TabOption (TAB_FLEET, "symbolFleet"));
            AddControlOption (new TabOption (TAB_LEVY, "symbolMilitia"));
            AddControlOption (new TabOption (TAB_INFRASTRUCTURE, "help"));

        }

        protected override void Regenerate () {
            base.Regenerate ();
            AddHeader (DEFAULT_BUTTONS, new DataReference<string> (this, "gui.header"));

            Vect2i start = CornerTopLeft;
            Grouping frame = new Grouping (start, new Vect2i (Presets.InnerArea.X, 128)) { Backgrounds = UIProvider.Style.CreateInset () };
            frame.AddWidget (new PlanetViewer (new Vect2i (2, 2), frame.Size - new Vect2i (4, 4),
                new DataReference<ulong> (this, KeysFragments.PLANET_SERIAL),
                new DataReference<float> (this, KeysFragments.PLANET_LOYALITY) { Template = "{0:P0}" }));
            AddWidget (frame);

            Vect2i tabsize = new Vect2i (Presets.InnerArea.X, Presets.InnerArea.X - frame.Size.Y - UIProvider.MarginSmall.Y);
            Vect2i tblsize = tabsize - new Vect2i (0, UIProvider.Margin.Y);
            Vect2i btnsize = new Vect2i (192, 24);

            start += new Vect2i (0, frame.Size.Y);
            start += new Vect2i (0, UIProvider.MarginSmall.Y);

            // Info tab
            CreateTab (TAB_INFO, start, tabsize);
            Vect2i tabstart = Vect2i.ZERO;

            AddWidget (TAB_INFO, new Label (tabstart, string.Format ("{0}:", Localization.Instance ["planetary_production"])));
            tabstart += new Vect2i (0, UIProvider.Margin.Y);
            AddWidget (TAB_INFO, _tblInfo = new Table (tabstart, tblsize) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 36
            });

            // Fleet tab
            CreateTab (TAB_FLEET, start, tabsize);
            tabstart = Vect2i.ZERO;

            AddWidget (TAB_FLEET, new Label (tabstart, string.Format ("{0}:", Localization.Instance ["planetary_fleets"])));
            AddWidget (TAB_FLEET, new Button (tabstart + new Vect2i (Presets.InnerArea.X - btnsize.X, 0), btnsize, KeysActions.PLANET_FLEET_MERGE, Localization.Instance ["fleets_merge"]));

            tabstart += new Vect2i (0, UIProvider.Margin.Y);
            AddWidget (TAB_FLEET, _tblFleets = new Table (tabstart, tblsize) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 36
            });
            _tblFleets.SetColumnAbsolute (0, 48);
            _tblFleets.SetColumnRelative (1, 100);
            _tblFleets.SetColumnAbsolute (2, 160);

            // Levy tab
            CreateTab (TAB_LEVY, start, tabsize);
            tabstart = Vect2i.ZERO;
            AddWidget (TAB_LEVY, _lblLevy = new Label (tabstart, string.Empty));
            AddWidget (TAB_LEVY, _btnLevy = new Button (tabstart + new Vect2i (Presets.InnerArea.X - btnsize.X, 0), btnsize, KeysActions.PLANET_LEVY_RAISE, Localization.Instance ["levy_raise"]));

            tabstart += new Vect2i (0, UIProvider.Margin.Y);
            AddWidget (TAB_LEVY, _tblLevy = new Table (tabstart, tblsize) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 36
            });
            _tblLevy.SetColumnAbsolute (0, 80);
            _tblLevy.SetColumnAbsolute (1, 48);
            _tblLevy.SetColumnRelative (2, 100);
            _tblLevy.SetColumnAbsolute (3, 96);
            //_tblLevy.SetColumnAbsolute (4, 48);

            // Building tab
            CreateTab (TAB_INFRASTRUCTURE, start, tabsize);
            tabstart = Vect2i.ZERO;
            AddWidget (TAB_INFRASTRUCTURE, new Label (tabstart, string.Format ("{0}:", Localization.Instance ["planetary_buildings"])));
            tabstart += new Vect2i (0, UIProvider.Margin.Y);
            AddWidget (TAB_INFRASTRUCTURE, _tblBuildings = new Table (tabstart, tblsize) {
                Backgrounds = UIProvider.Style.CreateInset (),
                RowMarking = new BackgroundSimple (Constants.TABLE_SELECTION),
                RowHighlight = new BackgroundSimple (Constants.TABLE_HOVER),
                RowHeight = 36
            });
            _tblBuildings.SetColumnAbsolute (0, 36);
            _tblBuildings.SetColumnAbsolute (2, 48);
            _tblBuildings.SetColumnAbsolute (3, 96);
            _tblBuildings.SetColumnAbsolute (4, 64);
            _tblBuildings.SetColumnAbsolute (5, 64);
        }

        protected override void Refresh () {
            base.Refresh ();

            _tblInfo.Reset (new PopulatorStatsTable (new DataReference<StatsRecorder<int>> (this, KeysFragments.PLANET_ATTRIBUTES), Planet.INFO_SLOTS));
            _tblFleets.Reset (new PopulatorFleetTable (new DataReference<List<ulong>> (this, KeysFragments.PLANET_FLEETS)));

            _tblLevy.Reset (new PopulatorSquadronTable (
                new DataPod<Levy> (GameAccess.Interface.Local.RequireState<Levy> (DataProvider.GetValue<ulong> (KeysFragments.PLANET_LEVY_SERIAL))),
                new DataReference<List<ShipInstance>> (this, KeysFragments.PLANET_SHIPS),
                new DataReference<IReadOnlyDictionary<ulong, Levy.SquadInfo>> (this, KeysFragments.PLANET_SQUADS)
            ));
            LevyState levystate = DataProvider.GetValue<LevyState> (KeysFragments.PLANET_LEVY_STATUS);

            _lblLevy.Template = string.Format ("{0} ({1}):", Localization.Instance ["planetary_levy"], Localization.Instance [string.Format ("levystate_{0}", levystate.ToString ().ToLowerInvariant ())]);
            switch (levystate) {
                case LevyState.Available:
                    _btnLevy.SetState (ElementState.Disabled, false);
                    break;
                default:
                    _btnLevy.SetState (ElementState.Disabled, true);
                    break;
            }

            _tblBuildings.Reset (new PopulatorBuildingTable (new DataReference<List<BuildingSector>> (this, KeysFragments.PLANET_BUILDINGS)));

        }

    }
}

