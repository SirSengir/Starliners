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

﻿using OpenTK.Input;
using BLibrary.Graphics;

namespace Starliners {

    public interface IInterfaceState : IGameState {
        /// <summary>
        /// Called after the IInterfaceState was created and made the active one.
        /// </summary>
        /// <param name="previous">Previous.</param>
        void OnSwitchedTo (IInterfaceState previous);

        /// <summary>
        /// Called for drawing.
        /// </summary>
        /// <param name="scene">Scene.</param>
        void Draw (RenderScene scene, double elapsedTime);

        void OnTextEntered (char unicode);

        void OnKeyPress (Key key);
    }
}
