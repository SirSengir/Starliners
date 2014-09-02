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
using BLibrary.Graphics;
using BLibrary.Util;
using BLibrary.Graphics.Sprites;
using Starliners.Game;
using System.Linq;
using System.Collections.Generic;
using BLibrary.Gui;
using BLibrary.Graphics.Text;
using Starliners.States;
using BLibrary.Gui.Widgets;
using Starliners.Game.Forces;
using Starliners.Game.Planets;
using BLibrary.Gui.Backgrounds;

namespace Starliners.Graphics {
    sealed class RendererPlanet : IObjectRenderer {

        #region Instance

        static RendererPlanet _instance;

        public static RendererPlanet Instance {
            get {
                if (_instance == null) {
                    _instance = new RendererPlanet ();
                }
                return _instance;
            }
        }

        #endregion

        #region Properties

        public SpriteModel this [IRenderable renderable, ModelPart part] {
            get {
                IRenderableEntity rendered = (IRenderableEntity)renderable;
                VerifyModelCache (rendered);

                SpriteModel model = _cachedModels [renderable.RenderHash] [(int)part];
                model.ModelReel = rendered.GetCurrentReel (part);
                return model;
            }
        }

        #endregion

        #region Fields

        DisposablesCache<DisposableArray<SpriteModel>> _cachedModels = new DisposablesCache<DisposableArray<SpriteModel>> ("RendererPlanet");
        EffectRenderer _effects = new EffectRenderer ();

        #endregion

        public RendererPlanet () {
        }

        public void OnAtlasRegeneration () {
            _cachedModels.Cleanup ();
        }

        public void OnFrameStart () {
            _effects.OnFrameStart ();
            _cachedModels.Maintain ();
        }

        public void OnRenderableRemoved (IRenderable renderable) {
            _cachedModels.Remove (renderable.RenderHash);
        }

        public void DrawRenderable (RenderTarget target, RenderStates states, IRenderable renderable) {
            DrawRenderable (target, states, renderable, RenderFlags.None);
        }

        public void DrawRenderable (RenderTarget target, RenderStates states, IRenderable renderable, RenderFlags flags) {

            SpriteModel model = this [renderable, ModelPart.Sprite];
            model.Draw (target, states);
            _effects.DrawOutline (target, states, renderable, model, flags);

            EntityPlanet planet = (EntityPlanet)renderable;
            if ((flags & RenderFlags.Legend) == RenderFlags.Legend && planet.Owner != null) {

                if (!GuiManager.Instance.Legends.ContainsKey (renderable.RenderHash)) {
                    GuiManager.Instance.Legends [renderable.RenderHash] = new LegendPlanet (planet);
                }
            }

            if (planet.Orbit.Fleets.Count > 0) {
                RenderStates fstates = states;
                fstates.Transform.Scale (4, 4);
                fstates.Transform.Translate (32, -32);

                foreach (Fleet fleet in planet.Orbit.Fleets.GroupBy(p => p.Backer.FleetOwner, p => p, (x, y) => y.First()).ToList()) {
                    RendererVessel.Instance.DrawRenderable (target, fstates, fleet.Projector);
                    fstates.Transform.Translate (0, 16);
                }
            }

        }

        void VerifyModelCache (IRenderableEntity renderable) {
            if (!_cachedModels.HasCached (renderable.RenderHash)) {

                Console.Out.WriteLine ("Creating SpriteModels for entity {0} (CacheCode: {1}).", renderable.GetType ().ToString (), renderable.RenderHash);
                _cachedModels [renderable.RenderHash] = new DisposableArray<SpriteModel> (ModelParts.VALUES.Length);
                for (int i = 0; i < ModelParts.VALUES.Length; i++) {
                    if (!renderable.HasPart (ModelParts.VALUES [i])) {
                        continue;
                    }
                    _cachedModels [renderable.RenderHash] [i] = new SpriteModel (renderable.GetReels (ModelParts.VALUES [i]), ModelParts.VALUES [i] == ModelPart.Sprite);
                }

            } else if (renderable.RenderChanged) {

                Console.Out.WriteLine ("Rebuilding SpriteModels for entity {0} (CacheCode: {1}).", renderable.GetType ().ToString (), renderable.RenderHash);
                for (int i = 0; i < ModelParts.VALUES.Length; i++) {
                    if (!renderable.HasPart (ModelParts.VALUES [i])) {
                        continue;
                    }

                    if (_cachedModels [renderable.RenderHash] [i] != null) {
                        _cachedModels [renderable.RenderHash] [i].Rebuild (renderable.GetReels (ModelParts.VALUES [i]));
                    } else {
                        _cachedModels [renderable.RenderHash] [i] = new SpriteModel (renderable.GetReels (ModelParts.VALUES [i]), ModelParts.VALUES [i] == ModelPart.Sprite);
                    }
                }
                renderable.RenderChanged = false;

            }
        }

    }
}

