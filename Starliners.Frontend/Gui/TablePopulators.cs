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
using BLibrary.Gui.Widgets;
using BLibrary.Util;
using Starliners.Game;
using BLibrary.Gui;
using System.Collections.Generic;
using Starliners.Gui.Widgets;
using BLibrary.Resources;
using Starliners.Game.Forces;
using Starliners.Game.Planets;
using System.Linq;
using BLibrary.Gui.Tooltips;
using Starliners.Gui.Tooltips;
using Starliners.Network;

namespace Starliners.Gui {

    sealed class PopulatorFleetTable : Table.IPopulator {

        IDataReference<List<ulong>> _reference;

        public PopulatorFleetTable (IDataReference<List<ulong>> reference) {
            _reference = reference;
        }

        public void Populate (Table table) {
            foreach (ulong serial in _reference.Value) {
                Fleet fleet = GameAccess.Interface.Local.RequireState<Fleet> (serial);
                table.AddCellContent (new Grouping (Vect2i.ZERO, Vect2i.ZERO, new IconBlazon (Vect2i.ZERO, new Vect2i (32, 32), fleet.Owner)) {
                    AlignmentH = Alignment.Center,
                    AlignmentV = Alignment.Center
                });
                table.NextColumn ();
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, fleet.FullName) {
                    ActionOnClick = new Widget.ClickPlain (KeysActions.GUI_OPEN, (ushort)GuiIds.Fleet, fleet.Serial)
                });
                table.NextColumn ();

                int[] composition = fleet.GetFleetComposition ();
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty,
                    string.Format ("{0}/{1}/{2}/{3}/{4}", composition [0], composition [1], composition [2], composition [3], composition [4])) {
                    ActionOnClick = new Widget.ClickPlain (KeysActions.GUI_OPEN, (ushort)GuiIds.Fleet, fleet.Serial)
                });
                table.NextRow ();
            }
        }

    }

    sealed class PopulatorSquadronTable : Table.IPopulator {
        IDataReference<Levy> _levy;
        IDataReference<List<ShipInstance>> _ships;
        IDataReference<IReadOnlyDictionary<ulong, Levy.SquadInfo>> _squads;

        public PopulatorSquadronTable (IDataReference<Levy> levy, IDataReference<List<ShipInstance>> ships, IDataReference<IReadOnlyDictionary<ulong, Levy.SquadInfo>> squads) {
            _levy = levy;
            _ships = ships;
            _squads = squads;
        }

        public void Populate (Table table) {

            ShipClass sclass = null;
            IReadOnlyDictionary<ulong, Levy.SquadInfo> squads = _squads.Value;
            IEnumerable<ShipInstance> ships = _ships.Value.OrderBy (p => p.ShipClass.Serial).ThenByDescending (p => p.State);

            foreach (ShipInstance ship in ships) {

                if (sclass != ship.ShipClass) {
                    sclass = ship.ShipClass;

                    Levy.SquadInfo sinfo = squads.ContainsKey (sclass.Serial) ? squads [sclass.Serial] : new Levy.SquadInfo (ships.Count (p => p.ShipClass == sclass));
                    table.AddIntertitle (string.Format ("{0} ({1}/{2})", Localization.Instance [sclass.Name], ships.Count (p => p.ShipClass == sclass), sinfo.Cap));
                }

                string fcolour;
                switch (ship.State) {
                    case ShipState.UnderConstruction:
                        fcolour = Colour.DarkGray.ToString ("#§");
                        break;
                    case ShipState.ArmourDamage:
                        fcolour = Colour.Yellow.ToString ("#§");
                        break;
                    case ShipState.HullDamage:
                        fcolour = Colour.OrangeRed.ToString ("#§");
                        break;
                    default:
                        fcolour = Colour.White.ToString ("#§");
                        break;
                }

                table.AddCellContent (new Grouping (Vect2i.ZERO, Vect2i.ZERO, new IconVessel (Vect2i.ZERO, new Vect2i (64, 64), ship.Projector)) {
                    AlignmentH = Alignment.Center,
                    AlignmentV = Alignment.Center,
                    FixedTooltip = new TooltipShip (new DataPod<ShipInstance> (ship))
                });
                table.NextColumn ();
                table.AddCellContent (new Grouping (Vect2i.ZERO, Vect2i.ZERO, new IconSymbol (Vect2i.ZERO, new Vect2i (32, 32), string.Format ("symbol{0}", ship.Level))) {
                    AlignmentH = Alignment.Center,
                    AlignmentV = Alignment.Center,
                    FixedTooltip = new TooltipSimple (string.Format ("{0} Level", ship.Level.ToString ()), string.Format ("Experience: {0}", ship.Experience))
                });
                table.NextColumn ();
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, string.Format ("{0}{1}", fcolour, Localization.Instance [ship.ShipClass.Name])) {
                    FixedTooltip = new TooltipShip (new DataPod<ShipInstance> (ship))
                });

                float status = ship.State != ShipState.UnderConstruction ? ship.Health : ship.Construction;
                table.NextColumn ();
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, string.Format ("{0}{1:P0}", fcolour, status)) {
                    FixedTooltip = new TooltipShip (new DataPod<ShipInstance> (ship))
                });

                /*
                table.NextColumn ();
                List<string> reenforceinfo = new List<string> ();
                if (squadron.Cap - squadron.Strength > 0) {
                    reenforceinfo.Add (string.Format ("Build Progress: {0:P0}", squadron.BuildProgress));
                    if (squadron.LastResupply > 0) {
                        reenforceinfo.Add (string.Format ("Built per Day: {0:##.##}", squadron.LastResupply));
                        reenforceinfo.Add (string.Format ("Estimated {0} days until full reenforcement.", (int)Math.Ceiling (((float)squadron.Cap - squadron.Strength - squadron.BuildProgress) / squadron.LastResupply)));
                    } else {
                        reenforceinfo.Add (string.Format ("{0}{1}", Colour.Crimson.ToString ("#§"), Localization.Instance ["tt_squad_reenforcement_stalled"]));
                    }
                } else {
                    reenforceinfo.Add (string.Format ("{0}{1}", Colour.LimeGreen.ToString ("#§"), Localization.Instance ["tt_squad_reenforced_full"]));
                }
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, string.Format ("{0}/{1}", (int)squadron.Strength, squadron.Cap)) {
                    //FixedTooltip = new TooltipSimple (string.Format ("{0}/{1}", (int)squadron.Strength, squadron.Cap), reenforceinfo.ToArray ())
                });
                table.NextColumn ();
                table.AddCellContent (new IconModifiers (Vect2i.ZERO, ship.Modifiers) {
                    //FixedTooltip = new TooltipDescribable (squadron.Modifiers)
                    FixedTooltip = new TooltipShip (new DataPod<ShipInstance> (ship))
                });
                                                */
                table.NextRow ();
            }

        }
    }

    sealed class PopulatorBuildingTable : Table.IPopulator {
        IDataReference<List<BuildingSector>> _reference;

        public PopulatorBuildingTable (IDataReference<List<BuildingSector>> reference) {
            _reference = reference;
        }

        public void Populate (Table table) {

            Improvement.Category category = null;

            foreach (BuildingSector building in _reference.Value) {
                if (category == null || !string.Equals (building.Improvement.Categorized.Name, category.Name)) {
                    category = building.Improvement.Categorized;
                    table.AddIntertitle (Localization.Instance [string.Format ("build_{0}", category.Name)]);
                }

                table.AddCellContent (new Grouping (Vect2i.ZERO, Vect2i.ZERO, new IconSymbol (Vect2i.ZERO, new Vect2i (32, 32), building.Improvement.Icon)) {
                    AlignmentH = Alignment.Center,
                    AlignmentV = Alignment.Center
                });
                table.NextColumn ();
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, Localization.Instance [building.Improvement.Name]));
                table.NextColumn ();
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, building.Amount.ToString ()) {
                    AlignmentH = Alignment.Center,
                    AlignmentV = Alignment.Center
                });
                table.NextColumn ();
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, string.Format ("$ {0}", building.Improvement.Cost.DeterminePurchasePrice (building.Amount))) {
                    AlignmentH = Alignment.Center,
                    AlignmentV = Alignment.Center
                });
                table.NextColumn ();

                Button button;
                table.AddCellContent (button = new Button (Vect2i.ZERO, new Vect2i (36, 36), KeysActions.PLANET_BUILDING_BUY, new IconSymbol (Vect2i.ZERO, new Vect2i (32, 32), "money"), building.Improvement.Serial));
                button.SetState (ElementState.Disabled, building.Improvement.Maximum > 0 && building.Amount >= building.Improvement.Maximum);
                table.NextColumn ();

                table.AddCellContent (button = new Button (Vect2i.ZERO, new Vect2i (36, 36), KeysActions.PLANET_BUILDING_SELL, new IconSymbol (Vect2i.ZERO, new Vect2i (32, 32), "invalid"), building.Improvement.Serial));
                button.SetState (ElementState.Disabled, building.Amount <= 0);
                table.NextRow ();
            }

        }
    }

    sealed class PopulatorHistoryTable : Table.IPopulator {
        IDataReference<List<IIncident>> _reference;

        public PopulatorHistoryTable (IDataReference<List<IIncident>> reference) {
            _reference = reference;
        }

        public void Populate (Table table) {
            foreach (IIncident incident in _reference.Value) {
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, incident.Description) {
                    ActionOnClick = new Widget.ClickPlain (KeysActions.GUI_OPEN_INCIDENT, incident.Serial)
                });
                table.NextRow ();
            }
        }
    }

    sealed class PopulatorServerTable : Table.IPopulator {

        InputText _ipAddress;
        InputText _ipPort;

        public PopulatorServerTable (InputText ipAddress, InputText ipPort) {
            _ipAddress = ipAddress;
            _ipPort = ipPort;
        }

        public void Populate (Table table) {
            table.AddIntertitle ("Recently Connected");

            foreach (ServerCache.ServerInfo server in GameAccess.Interface.ServerCache.Servers.OrderByDescending(p => p.LastConnection)) {

                Widget.ClickAction action = new Widget.ClickCustom (delegate {
                    _ipAddress.Entered = server.IPAddress.ToString ();
                    _ipPort.Entered = server.Port.ToString ();
                });

                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, string.Format ("{0} : {1}", server.IPAddress.ToString (), server.Port.ToString ())) {
                    ActionOnClick = action
                });
                table.NextColumn ();
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty,
                    string.IsNullOrWhiteSpace (server.Description) ? string.Format ("{0}{1}", Colour.DarkGray.ToString ("#§"), "<No Description>") : server.Description) {
                    ActionOnClick = action
                });
                table.NextColumn ();

                Colour colour;
                switch (server.Status) {
                    case ServerCache.ServerStatus.Offline:
                        colour = Colour.Crimson;
                        break;
                    case ServerCache.ServerStatus.Online:
                        colour = Colour.LimeGreen;
                        break;
                    default:
                        colour = Colour.DarkGray;
                        break;
                }

                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, string.Format ("{0}{1}", colour.ToString ("#§"), server.Status.ToString ())) {
                    ActionOnClick = action
                });
                table.NextRow ();
            }
        }
    }

    sealed class PopulatorStatsTable : Table.IPopulator {
        IDataReference<StatsRecorder<int>> _reference;
        IEnumerable<StatsRecorder<int>.StatisticSlot> _slots;

        public PopulatorStatsTable (IDataReference<StatsRecorder<int>> reference, IEnumerable<StatsRecorder<int>.StatisticSlot> slots) {
            _reference = reference;
            _slots = slots;
        }

        public void Populate (Table table) {

            StatsRecorder<int> statistics = _reference.Value;
            string category = string.Empty;
            foreach (StatsRecorder<int>.StatisticSlot slot in _slots) {
                if (!slot.IsDisplayed (statistics)) {
                    continue;
                }
                if (!string.Equals (category, slot.Category)) {
                    table.AddIntertitle (Localization.Instance [string.Format ("info_{0}", slot.Category)]);
                    category = slot.Category;
                }

                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, slot.Attribute));
                table.NextColumn ();
                table.AddCellContent (new ListItemText (Vect2i.ZERO, Vect2i.ZERO, string.Empty, slot.ToString (statistics)) { AlignmentH = Alignment.Center });
                table.NextRow ();
            }

        }
    }
}

