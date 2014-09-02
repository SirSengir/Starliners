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
using System.Linq;
using System.Collections.Generic;
using Starliners.Game.Planets;
using Starliners.Game.Forces;

namespace Starliners.Game.Scenario {
    sealed class MapGenerator : IMapGenerator {

        #region Constants

        static readonly Vect2f[] EMPIRE_DISTRIBUTION = new Vect2f[] {
            new Vect2f (0, 0), new Vect2f (1, 0), new Vect2f (1, 1), new Vect2f (0, 1),
            new Vect2i (0.5f, -1), new Vect2f (2, 0.5f), new Vect2f (0.5f, 2), new Vect2f (-1, 0.5f)
        };

        #endregion

        IPopulator _populator;

        public MapGenerator (IPopulator populator) {
            _populator = populator;
        }

        public void CreateMap (IWorldEditor editor) {
            AssetHolder<Blueprint> blueprints = (AssetHolder<Blueprint>)_populator.Holders [AssetKeys.BLUEPRINTS];
            AssetHolder<Culture> races = (AssetHolder<Culture>)_populator.Holders [AssetKeys.CULTURES];
            AssetHolder<Improvement> improvements = (AssetHolder<Improvement>)_populator.Holders [AssetKeys.IMPROVEMENTS];

            Eponym eponym = Eponym.GetForWorld (editor);
            Blueprint planetbp = blueprints.GetAsset ("planet");
            List<Improvement> buildings = new List<Improvement> () {
                improvements ["shipyard0-0"]
            };

            int planetcount = editor.GetParameter<int> (ParameterKeys.EMPIRE_SIZE);
            int delta = (int)Math.Sqrt (planetcount) * 18;
            Vect2f area = new Vect2f (delta, delta);
            List<Vect2f> setcoords = new List<Vect2f> ();

            for (int ecount = 0; ecount < editor.GetParameter<int> (ParameterKeys.EMPIRE_COUNT); ecount++) {
                Faction faction = editor.States.Values.OfType<Faction> ().Where (p => p.Flags.Contains (string.Format ("empire{0}", ecount.ToString ()))).First ();
                editor.GameConsole.Debug ("Creating empire {0}.", faction.FullName);

                // Create the initial start fleets
                Dictionary<int, Dictionary<ShipClass, int>> legions = new Dictionary<int, Dictionary<ShipClass, int>> ();
                float lfactor = editor.GetParameter<float> (ParameterKeys.EMPIRE_LEGION);
                if (lfactor > 0) {
                    for (int i = 0; i < 1; i++) {
                        Dictionary<ShipClass, int> legion = new Dictionary<ShipClass, int> ();
                        legions [editor.Seed.Next (planetcount)] = legion;
                        foreach (ShipClass sclass in ShipClass.GetClassesForWorld(editor).Where(p => p.Flags.Contains("legion"))) {
                            int strength = 0;
                            switch (sclass.Size) {
                                case ShipSize.Frigate:
                                    strength = 36;
                                    break;
                                case ShipSize.Destroyer:
                                    strength = 18;
                                    break;
                                case ShipSize.Cruiser:
                                    strength = 9;
                                    break;
                                case ShipSize.Battleship:
                                    strength = 4;
                                    break;
                                case ShipSize.Dreadnought:
                                default:
                                    strength = 1;
                                    break;
                            }

                            legion [sclass] = (int)(strength * lfactor);
                        }
                    }
                }

                Vect2f mincoords = EMPIRE_DISTRIBUTION [ecount] * area;
                //int minX = -delta / 2, minY = -delta / 2;

                for (int i = 0; i < planetcount; i++) {

                    bool collision = true;
                    Vect2i coords = Vect2i.ZERO;
                    while (collision) {
                        collision = false;
                        coords = new Vect2i (mincoords.X + editor.Seed.Next (delta), mincoords.Y + editor.Seed.Next (delta));
                        foreach (Vect2f exists in setcoords) {
                            if (MathUtils.GetDistanceBetween (exists, coords) < 6f) {
                                collision = true;
                                break;
                            }
                        }
                    }
                    setcoords.Add (coords);

                    PlanetType type = PlanetTypes.VALUES [editor.Seed.Next (PlanetTypes.VALUES.Length - 1) + 1];
                    Culture culture = faction.Cultures.OrderBy (p => editor.Seed.Next ()).First ();
                    int size = editor.Seed.Next (1000000);
                    int population = editor.Seed.Next (size) + 1;
                    Planet data = new Planet (editor, type, faction, eponym.GeneratePlanetName (), size, culture, population) { Location = coords };
                    foreach (Improvement building in buildings) {
                        data.AddImprovement (building);
                    }

                    // Add weapon facility
                    Improvement weaponeer;
                    switch (faction.CombatProperties.WeaponStrength) {
                        case DamageKind.Radiation:
                            weaponeer = improvements ["weaponeer2"];
                            break;
                        case DamageKind.Kinetic:
                            weaponeer = improvements ["weaponeer1"];
                            break;
                        default:
                            weaponeer = improvements ["weaponeer0"];
                            break;
                    }
                    data.AddImprovement (weaponeer);

                    EntityPlanet planet = new EntityPlanet (editor, string.Format ("planet-{0}", i), planetbp, data);
                    planet.PlanetData.Levy.Rejuvenate ();
                    editor.Controller.QueueEntity (planet);
                    editor.GameConsole.Debug ("Generated planet {0}.", planet.PlanetData.FullName);

                    // Add squadrons
                    if (legions.ContainsKey (i)) {
                        foreach (var entry in legions[i]) {
                            for (int scount = 0; scount < entry.Value; scount++) {
                                ShipInstance ship = new ShipInstance (editor, planet.PlanetData.Levy, entry.Key, new ShipModifiers ());
                                ship.ApplyConstruction (1);
                                ship.GainExperience (1000);
                                planet.PlanetData.Levy.SpawnShip (ship);
                            }
                        }
                        planet.RaiseLevy ();
                    }

                }
            }
        }

    }
}

