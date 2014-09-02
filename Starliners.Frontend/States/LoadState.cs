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

﻿using System.Threading;
using BLibrary.Saves;
using BLibrary.Util;
using BLibrary.Graphics;
using BLibrary.Gui.Data;
using BLibrary.Gui;
using BLibrary.Gui.Interface;


namespace Starliners.States {

    sealed class LoadState : InterfaceState {
        public MapState NextState { get; set; }

        int _stage;
        LoadStage _current;
        LoadStage[] _stages;
        GuiLoadStatus _gui;

        public LoadState (RenderScene scene)
            : base (scene) {

            Canvas = new FlatCanvas ("Textures/Menus/Loading");
            _stages = new LoadStage[] {
                new StageWaitServer (),
                new StageMapInit (this),
                new StageConnect (),
                new StageReceiveSetup (this),
                new StageRenderSetup (this)
            };
        }

        public override void OnSwitchedTo (IInterfaceState previous) {

            base.OnSwitchedTo (previous);

            _gui = new GuiLoadStatus ();
            GuiManager.Instance.OpenGui (_gui);

            GameAccess.Interface.Launch.Initialize ();
        }

        public override void Draw (RenderScene scene, double elapsedTime) {

            if (NextState == null) {
                NextState = new MapState (scene);
            }

            _gui.UpdateFragment ("load.status", TickState ());
            _gui.UpdateFragment ("load.current", _stage + 1);
            _gui.UpdateFragment ("load.stages", _stages.Length + 1);
            _gui.UpdateFragment ("load.percent", string.Format ("{0:0}", (_current.Percent * 100)));

            if (Canvas != null) {
                Canvas.Draw (scene, RenderStates.DEFAULT);
            }
            DrawPrevious (scene, RenderStates.DEFAULT);
            GuiManager.Instance.Draw (scene, RenderStates.DEFAULT);

        }

        string TickState () {

            if (_current == null) {

                _current = _stages [_stage];
                _current.Begin ();
                GameAccess.Interface.GameConsole.Debug ("Status: " + _current.Status);
                return _current.Status;

            } else {

                if (!_current.IsComplete)
                    return _current.Status;

                _current.End ();
                _stage++;
                if (_stage >= _stages.Length) {

                    GameAccess.Interface.SwitchTo (NextState);
                    return "Loading...";

                } else {

                    _current = _stages [_stage];
                    _current.Begin ();
                    GameAccess.Interface.GameConsole.Debug ("Status: " + _current.Status);
                    return _current.Status;

                }
            }

        }
    }
}
