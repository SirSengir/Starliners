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

﻿using BLibrary.Graphics;
using BLibrary.Graphics.Sprites;
using Starliners;
using BLibrary.Gui.Widgets;
using BLibrary.Gui;
using BLibrary.Audio;
using Starliners.Map;

namespace Starliners.States {

    sealed class StartupState : InterfaceState {
        IInterfaceDefinition _interfaceDefinition;
        bool _isInited = false;

        public StartupState (RenderScene scene, IInterfaceDefinition interfaceDefinition)
            : base (scene) {
            _interfaceDefinition = interfaceDefinition;
            SpriteManager.Instance = new SpriteManager ();
            Canvas = new FlatCanvas ("Textures/Menus/Startup");
        }

        public override void OnSwitchedTo (IInterfaceState previous) {
        }

        public override void OnExit () {
        }

        public override void Tick () {
            base.Tick ();

            if (!_isInited) {

                SpriteManager.Instance.RegenerateAtlas (SpriteState.Navigation);

                MapRendering.Instance = new MapRendering ();
                _interfaceDefinition.RegisterRenderers (MapRendering.Instance);
                GuiManager.Instance = new GuiManager (Scene, _interfaceDefinition);
                _interfaceDefinition.InitGui ();

                MapRendering.Instance.OnAtlasRegeneration ();

                SoundManager.Instance = new SoundManager ();

                _isInited = true;
                GameAccess.Interface.SwitchTo (new MenuState (Scene));
            }

        }

        public override void Draw (RenderScene scene, double elapsedTime) {
            Canvas.Draw (scene, RenderStates.DEFAULT);
        }
    }
}

