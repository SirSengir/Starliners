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
using System.IO;
using BLibrary.Json;
using BLibrary.Resources;
using BLibrary.Gui;
using BLibrary.Util;
using BLibrary.Gui.Backgrounds;
using Starliners.Map;
using Starliners.Graphics;
using Starliners.Game;

namespace Starliners {

    public class InterfaceDefinition : IInterfaceDefinition {

        public StyleWindow Style {
            get;
            private set;
        }

        public Dictionary<string, StyleWindow> Styles {
            get;
            private set;
        }

        public IDictionary<string, Background> Backgrounds {
            get;
            private set;
        }

        public Background Inset {
            get;
            private set;
        }

        public Vect2i Margin {
            get;
            private set;
        }

        public Vect2i MarginSmall {
            get;
            private set;
        }

        public void InitGui () {
            Colour defaultColour = Colour.SteelBlue;

            Backgrounds = new Dictionary<string, Background> ();
            Backgrounds ["gui.shadow"] = new BackgroundDynamic ("guiShadow") { Colour = Colour.White };
            Backgrounds ["map.plaque"] = new BackgroundTiled () {
                Colour = Colour.White,
            };
            Backgrounds ["gui.compact"] = new BackgroundTiled () {
                Colour = Colour.White,
                Shadow = Backgrounds ["gui.shadow"]
            };

            foreach (KeyValuePair<string, Background> background in ParseBackgrounds()) {
                Backgrounds [background.Key] = background.Value;
            }

            Backgrounds ["guiWindowHeader"] = new BackgroundTiled ("guiWindowHeader");
            Backgrounds ["guiWindowHeaderDragged"] = new BackgroundTiled ("guiWindowHeaderDragged");

            Backgrounds ["guiButton"] = new BackgroundTiled ("guiButton") { Colour = defaultColour };
            Backgrounds ["guiButtonHovered"] = new BackgroundTiled ("guiButtonHover") { Colour = Colour.SteelBlue };
            Backgrounds ["guiButtonDisabled"] = new BackgroundTiled ("guiButtonDisabled") { Colour = Colour.SteelBlue };
            Backgrounds ["guiButtonPressed"] = new BackgroundTiled ("guiButtonPressed") { Colour = Colour.SteelBlue };

            Backgrounds ["guiInputInactive"] = new BackgroundTiled ("guiInputInactive");
            Backgrounds ["guiInputActive"] = new BackgroundTiled ("guiInputActive");

            Backgrounds ["buttonMenu"] = new BackgroundTiled ("guiButtonMenu") {
                Colour = defaultColour,
                Shadow = Backgrounds ["gui.shadow"]
            };
            Backgrounds ["buttonMenuHovered"] = new BackgroundTiled ("guiButtonMenuHover") {
                Colour = Colour.SteelBlue,
                Shadow = Backgrounds ["gui.shadow"]
            };
            Backgrounds ["buttonMenuDisabled"] = new BackgroundTiled ("guiButtonMenuDisabled") {
                Colour = Colour.SteelBlue,
                Shadow = Backgrounds ["gui.shadow"]
            };
            Backgrounds ["buttonMenuPressed"] = new BackgroundTiled ("guiButtonMenuPressed") {
                Colour = Colour.SteelBlue,
                Shadow = Backgrounds ["gui.shadow"]
            };

            Backgrounds ["guiSlotDefault"] = new BackgroundTiled ("guiSlotDefault") { Colour = defaultColour };
            Backgrounds ["guiSlotHovered"] = new BackgroundTiled ("guiSlotDefaultHighlight");
            Backgrounds ["guiSlotDisabled"] = new BackgroundTiled ("guiButtonDisabled") { Colour = defaultColour };

            Backgrounds ["guiSlotHex"] = new BackgroundTiled ("guiSlotHex") { Colour = defaultColour };
            Backgrounds ["guiSlotHexHovered"] = new BackgroundTiled ("guiSlotHexHighlight");

            Backgrounds ["guiButton"] = new BackgroundTiled ("guiButton") { Colour = defaultColour };
            Backgrounds ["guiButtonHovered"] = new BackgroundTiled ("guiButtonHover") { Colour = defaultColour };
            Backgrounds ["guiButtonDisabled"] = new BackgroundTiled ("guiButtonDisabled") { Colour = defaultColour };

            Backgrounds ["guiSlotToken"] = new BackgroundTiled ("guiSlotToken") { Colour = defaultColour };
            Backgrounds ["guiSlotTokenHovered"] = new BackgroundTiled ("guiSlotHexHighlight");

            Backgrounds ["guiInset"] = Inset = new BackgroundTiled ("guiInset") { Colour = defaultColour };

            Styles = ParseStyles ();
            Style = Styles ["default"];

            Margin = new Vect2i (32, 32);
            MarginSmall = new Vect2i (16, 16);
        }

        public void RegisterRenderers (MapRendering rendering) {
            rendering.RegisterRenderer ((ushort)RenderType.Planet, RendererPlanet.Instance);
            rendering.RegisterRenderer ((ushort)RenderType.Vessel, RendererVessel.Instance);

            rendering.RegisterRenderer ((byte)ParticleId.Explosion, new RendererParticles ());
            /*
            rendering.RegisterRenderer ((ushort)RenderType.Item, RendererItems.Instance);
            rendering.RegisterRenderer ((ushort)RenderType.Structure, new RendererEntities ());
            rendering.RegisterRenderer ((ushort)RenderType.Tree, new RendererEntities ());
            rendering.RegisterRenderer ((ushort)RenderType.Creature, RendererCreatures.Instance);

            rendering.RegisterRenderer ((byte)ParticleType.Text, new RendererParticles (0));
            rendering.RegisterRenderer ((byte)ParticleType.ItemStack, new RendererParticles (1));
            rendering.RegisterRenderer ((byte)ParticleType.Blazoned, new RendererParticles (2));
            rendering.RegisterRenderer ((byte)ParticleType.Icon, new RendererParticles (3));
            rendering.RegisterRenderer ((byte)ParticleType.Bee, new RendererAuxParticles ());

            rendering.RegisterRenderer ((ushort)TagIds.Entity, new RendererTags ());
            rendering.RegisterRenderer ((ushort)TagIds.Stocked, new RendererAuxTags ());
            */
        }

        Dictionary<string, StyleWindow> ParseStyles () {
            Dictionary<string, StyleWindow> styles = new Dictionary<string, StyleWindow> ();

            foreach (ResourceFile resource in GameAccess.Resources.Search ("Styling.Windows")) {

                JsonArray result;
                using (StreamReader reader = new StreamReader (resource.OpenRead ())) {
                    result = JsonParser.JsonDecode (reader.ReadToEnd ()).GetValue<JsonArray> ();
                }

                foreach (JsonObject json in result.GetEnumerable<JsonObject>()) {
                    StyleWindow parsed = new StyleWindow (this, json);
                    if (styles.ContainsKey (parsed.Key)) {
                        throw new SystemException ("Cannot replace already existing window style: " + parsed.Key);
                    }
                    styles [parsed.Key] = parsed;
                }
            }

            return styles;
        }

        Dictionary<string, Background> ParseBackgrounds () {
            Dictionary<string, Background> backgrounds = new Dictionary<string, Background> ();

            foreach (ResourceFile resource in GameAccess.Resources.Search ("Styling.Backgrounds")) {

                JsonArray result;
                using (StreamReader reader = new StreamReader (resource.OpenRead ())) {
                    result = JsonParser.JsonDecode (reader.ReadToEnd ()).GetValue<JsonArray> ();
                }

                foreach (JsonObject background in result.GetEnumerable<JsonObject>()) {
                    Background parsed;

                    Colour colour = new Colour (background.ContainsKey ("colour") ? Int32.Parse (background ["colour"].GetValue<string> (), System.Globalization.NumberStyles.HexNumber) : 0xffffff);
                    string texture = background.ContainsKey ("texture") ? background ["texture"].GetValue<string> () : "guiWindow";
                    string type = background.ContainsKey ("type") ? background ["type"].GetValue<string> () : "tiled";

                    if ("dynamic".Equals (type)) {
                        BackgroundDynamic bg = new BackgroundDynamic (texture) { Colour = colour };
                        parsed = bg;
                        if (background.ContainsKey ("edges")) {
                            bg.Edges = ParseDirections (background ["edges"].GetValue<JsonArray> ());
                        }
                    } else if ("tiled".Equals (type)) {
                        BackgroundTiled bg = new BackgroundTiled (texture) { Colour = colour };
                        parsed = bg;
                        if (background.ContainsKey ("edges")) {
                            bg.Edges = ParseDirections (background ["edges"].GetValue<JsonArray> ());
                        }
                    } else {
                        throw new SystemException ("Cannot parse a background of type: " + type);
                    }

                    parsed.Shadow = background.ContainsKey ("shadow") && background ["shadow"].GetValue<bool> () ? Backgrounds ["gui.shadow"] : null;
                    backgrounds [background ["key"].GetValue<string> ()] = parsed;
                }
            }

            return backgrounds;
        }

        Direction ParseDirections (JsonArray list) {
            Direction direction = Direction.Unknown;
            foreach (string dir in list.GetEnumerable<string>()) {
                direction |= (Direction)Enum.Parse (typeof(Direction), dir, true);
            }
            return direction;
        }

        #region IDisposable

        public void Dispose () {
            RendererBlazon.Instance.Dispose ();
        }

        #endregion
    }
}
