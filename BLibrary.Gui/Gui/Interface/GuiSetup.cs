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

﻿using BLibrary.Resources;
using System.Collections.Generic;
using System.Linq;
using System;
using BLibrary.Util;
using BLibrary.Gui.Widgets;
using Starliners.Game;
using BLibrary.Gui.Tooltips;
using Starliners;

namespace BLibrary.Gui.Interface {

    class GuiSetup : GuiLocal {
        #region Constants

        const string WINDOW_KEY = "menu_setup";
        static readonly Vect2i WINDOW_SIZE = new Vect2i (356, 402);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets (WINDOW_KEY, WINDOW_SIZE, Positioning.Centered, true);

        #endregion

        Vect2i _start;
        Vect2i _spacing;
        Switchable _scenario;
        Dictionary<string, InputText> _inputs = new Dictionary<string, InputText> ();
        Dictionary<string, Switchable> _switchables = new Dictionary<string, Switchable> ();
        Label _message;

        IReadOnlyList<ParameterOptions> _parameters;

        public GuiSetup ()
            : base (WINDOW_SETTING) {

            IsDraggable = false;
            IsCloseable = false;

            _start = CornerTopLeft;
            _spacing = new Vect2i (0, 44);

        }

        protected override void Regenerate () {
            base.Regenerate ();

            _switchables.Clear ();
            _parameters = GameAccess.Game.SetupOptions;
            List<string> categories = _parameters.Select (p => p.Category).Distinct ().ToList ();
            int columnwidth = 356;
            int rows = _parameters.GroupBy (p => p.Category, p => p.Key, (key, g) => new { Category = key, ElementCount = g.Count () }).Max (p => p.ElementCount);
            rows = rows < 3 ? 3 : rows;
            Presets = new WindowPresets (WINDOW_SETTING.Key, new Vect2i (categories.Count * (columnwidth + UIProvider.MarginSmall.X), UIProvider.Margin.Y + 40 + rows * 40), WINDOW_SETTING.Positioning, true);

            Vect2i column1 = new Vect2i (128, 0);
            Vect2i inputSize = new Vect2i (columnwidth - column1.X, 40);

            int count = 0;

            AddHeader (WindowButton.None, Localization.Instance ["menu_setupgame"]);

            Grouping frame = CreateColumn (0, columnwidth);
            AddWidget (frame);

            for (int i = 0; i < categories.Count; i++) {
                count = 0;
                string category = categories [i];

                frame = CreateColumn (i, columnwidth);
                AddWidget (frame);

                foreach (ParameterOptions option in _parameters.Where(p => category.Equals(p.Category))) {

                    string localization = string.Format ("setup_{0}", option.Key.Replace ('.', '_'));
                    frame.AddWidget (new Label (_spacing * count, new Vect2i (column1.X, 40), Localization.Instance [localization]) {
                        AlignmentV = Alignment.Center,
                        AlignmentH = Alignment.Center,
                        FixedTooltip = new TooltipSimple (Localization.Instance [localization], Localization.Instance [localization + "_help"])
                    });

                    if (option.Type == ParameterOptions.InputType.Switchable) {
                        _switchables [option.Key] = new Switchable (_spacing * count++ + column1, inputSize, "input." + option.Key,
                            option.Values,
                            option.Readables) {
                            Value = option.Default,
                            FixedTooltip = new TooltipSimple (Localization.Instance [localization], Localization.Instance [localization + "_help"])
                        };
                        frame.AddWidget (_switchables [option.Key]);

                    } else if (option.Type == ParameterOptions.InputType.Text) {
                        _inputs [option.Key] = new InputText (_spacing * count++ + column1, inputSize, "input." + option.Key, string.Format ("<{0}>", option.Default)) {
                            Entered = option.Default.ToString (),
                            CharLimit = 24,
                            FixedTooltip = new TooltipSimple (Localization.Instance [localization], Localization.Instance [localization + "_help"])
                        };
                        frame.AddWidget (_inputs [option.Key]);

                    } else if (option.Type == ParameterOptions.InputType.Scenario) {
                        _scenario = new Switchable (_spacing * count++ + column1, inputSize, "input." + option.Key,
                            GameAccess.Game.Scenarios.ToArray (), GameAccess.Game.Scenarios.Select (p => p.Name).ToArray ()) {
                            FixedTooltip = new TooltipSimple (Localization.Instance [localization], Localization.Instance [localization + "_help"])
                        };
                        frame.AddWidget (_scenario);

                    } else {
                        throw new SystemException ("Unhandled parameter option: " + option.Type);
                    }

                }
            }

            _message = new Label (new Vect2i (_start.X, Size.Y - UIProvider.Margin.X - 40 - 2 * _spacing.Y), new Vect2i (Size.X - 2 * UIProvider.Margin.X, 40), "<empty>") {
                IsDisplayed = false,
                AlignmentV = Alignment.Center,
                AlignmentH = Alignment.Center
            };
            AddWidget (_message);

            Vect2i navSize = new Vect2i ((Presets.InnerArea.X - UIProvider.MarginSmall.X) / 2, 40);
            AddWidget (new Button (new Vect2i (_start.X, Size.Y - UIProvider.Margin.X - 40), navSize, "startgame", Localization.Instance ["btn_start_game"]));
            AddWidget (new Button (new Vect2i (_start.X + navSize.X + UIProvider.MarginSmall.X, Size.Y - UIProvider.Margin.X - 40), navSize, "mainmenu", Localization.Instance ["btn_nav_mainmenu"]));
        }

        Grouping CreateColumn (int ordinal, int columnwidth) {
            return new Grouping (CornerTopLeft + new Vect2i (ordinal * (columnwidth + UIProvider.MarginSmall.X), 0), new Vect2i (columnwidth, Presets.InnerArea.Y - 88)) {
                AlignmentH = Alignment.Center,
                AlignmentV = Alignment.Top
            };
        }

        public override bool DoAction (string key, params object[] args) {

            if ("mainmenu".Equals (key)) {
                GuiManager.Instance.CloseGuiAll ();
                GameAccess.Interface.OpenMainMenu ();
                return true;

            } else if ("startgame".Equals (key)) {
                if (string.IsNullOrWhiteSpace (_inputs [ParameterKeys.NAME].Entered)) {
                    _message.IsDisplayed = true;
                    return true;
                }
                if (string.IsNullOrWhiteSpace (_inputs [ParameterKeys.CREATOR].Entered)) {
                    _message.IsDisplayed = true;
                    return true;
                }
                if (_scenario.Value == null) {
                    _message.IsDisplayed = true;
                    return true;
                }

                MetaContainer parameters = GameAccess.Game.CreateDefaultParameters ();
                foreach (var entry in _inputs) {
                    parameters.Set (entry.Key, entry.Value.Entered);
                }
                foreach (var entry in _switchables) {
                    parameters.Set (entry.Key, entry.Value.Value);
                }
                GameAccess.Interface.CreateGame (parameters, (IScenarioProvider)_scenario.Value);
                return true;

            } else
                return base.DoAction (key, args);
        }
    }
}
