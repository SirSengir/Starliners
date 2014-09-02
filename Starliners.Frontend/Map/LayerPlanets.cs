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
using Starliners.Graphics;
using BLibrary.Graphics;
using Starliners.Game;
using System.Collections.Generic;
using BLibrary.Gui;
using BLibrary.Util;
using BLibrary.Graphics.Sprites;
using System.Linq;

namespace Starliners.Map {
    sealed class LayerPlanets : MapLayer {

        //Shader _changeAlpha;
        //List<Entity> _needUnhide = new List<Entity> ();
        Rectangle _point0;

        public LayerPlanets (MapRenderable map, UILayer layer)
            : base (map, layer) {

            _point0 = new Rectangle (new Vect2i (2, 2));
            // Create shaders
            /*
            ResourceFile resource = GameAccess.Resources.SearchResource ("Shaders.ChangeAlpha.frag");
            using (Stream stream = resource.OpenRead ()) {
                _changeAlpha = new Shader (ShaderType.FragmentShader, stream);
            }
            _changeAlpha.SetUniform ("intensity", 0.6f);
            */
        }

        public override void Draw (RenderTarget target, RenderStates states, View view) {

            // Draw Entities
            /*
            _needUnhide.Clear ();

            foreach (Entity entity in Map.VisibleEntities.Where(p => p.UILayer == UILayer).OrderBy(p => p.Location.Y)) {

                UnhidingBehaviour unhiding = GuiManager.Instance.KeyboardState.HasFlag (ControlState.Alt) ? UnhidingBehaviour.Request : UnhidingBehaviour.Always;
                // Reduce the alpha if an entity with precedence exists.
                if ((entity.Blueprint.Unhiding & unhiding) == unhiding) {
                    _needUnhide.Add (entity);
                }

                foreach (Entity unhidden in _needUnhide) {
                    if (!Utils.IsWithin (unhidden.Center, entity.Bounding)) {
                        continue;
                    }
                    if (unhidden == entity) {
                        continue;
                    }
                    if (unhidden.Location.Y > entity.Location.Y) {
                        continue;
                    }

                    unhidden.RenderEffects |= RenderEffect.Outline;
                    entity.RenderEffects |= RenderEffect.Translucency;
                    break;
                }
            }
            */

            foreach (Entity entity in Map.VisibleEntities.Where(p => p.UILayer == UILayer).OrderBy(p => p.Location.Y)) {

                RenderStates tilestate = states;
                tilestate.Transform.Translate ((int)(entity.Location.X * SpriteManager.TILE_DIMENSION), (int)(entity.Location.Y * SpriteManager.TILE_DIMENSION));

                //if ((entity.RenderEffects & RenderEffect.Translucency) == RenderEffect.Translucency) {
                //tilestate.Shader = _changeAlpha;
                //}

                // Render the entity with it's render type.
                IObjectRenderer renderer = MapRendering.Instance.ObjectRenderers [entity.RenderType];
                RenderStates entitystate = tilestate;
                entitystate.Transform.Scale (0.25, 0.25);
                renderer.DrawRenderable (target, entitystate, entity, RenderFlags.Legend | RenderFlags.Outline);

                entity.RenderFlags = RenderFlags.None;
                target.Draw (_point0, tilestate);
                /*
                tilestate.Transform.Translate (SpriteManager.TILE_DIMENSION * entity.BoundingSize.X - 2, 0);
                target.Draw (_point0, tilestate);
                tilestate.Transform.Translate (0, SpriteManager.TILE_DIMENSION * entity.BoundingSize.Y - 2);
                target.Draw (_point0, tilestate);
                tilestate.Transform.Translate (-SpriteManager.TILE_DIMENSION * entity.BoundingSize.X + 2, 0);
                target.Draw (_point0, tilestate);
                */
            }

            // Render overlays
            //foreach (Entity entity in Map.VisibleEntities) {
            //DrawOverlay (target, states, entity);
            //}

            // Render tags if alt-key is pressed.
            if (GuiManager.Instance.KeyboardState.HasFlag (ControlState.Alt)) {

                foreach (Entity entity in Map.VisibleEntities) {
                    if (entity.TagId == 0) {
                        continue;
                    }

                    MapRendering.Instance.TagRenderers [entity.TagId].DrawTag (target, states, entity);
                }
            }

            // Render particles.
            foreach (Particle particle in Access.Particles) {
                if (!Map.DrawnArea.IsWithinRenderedArea (particle.Location)) {
                    continue;
                }
                MapRendering.Instance.ParticleRenderers [(ushort)particle.Type].DrawParticle (target, states, particle);
            }

        }

        /*
        void DrawOverlay (RenderTarget target, RenderStates states, Entity entity) {

            IRenderableEntity renderable = entity as IRenderableEntity;
            if (renderable == null) {
                return;
            }

            if (renderable.HasPart (ModelPart.Overlay)) {

                float amplitude = MathUtils.AsAmplitude (SpriteManager.Instance.Metronom0);
                RenderStates ostates = states;

                IObjectRenderer renderer = MapRendering.Instance.ObjectRenderers [entity.RenderType];
                ostates.Transform.Translate ((int)(entity.Location.X * SpriteManager.TILE_DIMENSION), (int)(entity.Location.Y * SpriteManager.TILE_DIMENSION));

                SpriteModel model = renderer [entity, ModelPart.Overlay];
                ostates.Transform.Translate (
                    (model.LocalBounds.Width - (model.LocalBounds.Width * amplitude)) / 2,
                    (model.LocalBounds.Height - (model.LocalBounds.Height * amplitude)) / 2);
                ostates.Transform.Scale (amplitude, amplitude);
                model.Draw (target, ostates);
            }

            if (renderable.HasPart (ModelPart.Signal)) {

                RenderStates sstates = states;

                IObjectRenderer renderer = MapRendering.Instance.ObjectRenderers [entity.RenderType];
                sstates.Transform.Translate ((int)(entity.Location.X * SpriteManager.TILE_DIMENSION), (int)(entity.Location.Y * SpriteManager.TILE_DIMENSION));
                sstates.Transform.Scale (renderer.MapScale);
                sstates.Transform.Translate (0, -SpriteManager.TILE_DIMENSION);

                SpriteModel model = renderer [entity, ModelPart.Signal];
                model.Draw (target, sstates);

            }

            /*
            if (entity == Map.HoveredEntity && entity.ProgressInfos.Count > 0) {

                RenderStates fstates = states;

                if (entity.Serial != _lastFramed) {
                    _lastFrame = _mapframe.Copy ();
                    _lastFramed = entity.Serial;
                }

                Vect2i size = (GuiManager.Instance.UIProvider.Margin * 2) + new Vect2i (entity.ProgressInfos.Count * 32, entity.BoundingSize.Y * SpriteManager.TILE_DIMENSION);
                fstates.Transform.Translate (
                    (int)((entity.Location.X + entity.BoundingSize.X) * SpriteManager.TILE_DIMENSION),
                    (int)(entity.Location.Y * SpriteManager.TILE_DIMENSION));
                fstates.Transform.Scale (0.5f, 0.5f);
                _lastFrame.Render (size, target, fstates);
            }*/
        //}
    }
}

