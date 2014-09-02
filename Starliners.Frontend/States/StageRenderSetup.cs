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

﻿using System.Linq;
using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using Starliners.Map;


namespace Starliners.States {

    sealed class StageRenderSetup : LoadStage {
        public override string Status {
            get { return "Sending in the drones..."; }
        }

        LoadState _state;

        public StageRenderSetup (LoadState state) {
            _state = state;
        }

        public override void Begin () {
            base.Begin ();
        }

        public override void ThreadStart () {

            GameAccess.Interface.BindSecondaryContext ();

            // Init texture sheets and register all icons.
            SpriteManager.Instance.RegenerateAtlas (SpriteState.Game);
            MapRendering.Instance.OnAtlasRegeneration ();

            // Register icons
            foreach (ISpriteDeclarant provider in WorldInterface.Instance.Access.Assets.Values.OfType<ISpriteDeclarant>()) {
                provider.MakeSane ();
                provider.RegisterIcons (SpriteManager.Instance);
            }
            foreach (ISpriteDeclarant provider in WorldInterface.Instance.Access.States.Values.OfType<ISpriteDeclarant>()) {
                provider.MakeSane ();
                provider.RegisterIcons (SpriteManager.Instance);
            }
            foreach (ISpriteDeclarant provider in WorldInterface.Instance.Access.Entities.Values.OfType<ISpriteDeclarant>()) {
                provider.MakeSane ();
                provider.RegisterIcons (SpriteManager.Instance);
            }

            GameAccess.Interface.UnbindSecondaryContext ();
            IsComplete = true;
        }

        public override void End () {
            base.End ();

            // Create all map layers.
            _state.NextState.Map.AssembleLayers ();
        }
    }
}
