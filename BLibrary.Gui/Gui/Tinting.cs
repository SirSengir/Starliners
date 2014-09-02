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

namespace BLibrary.Gui {

    public abstract class Tinting {
        protected abstract bool ApplyTint (GuiElement element);

        protected abstract Colour GetActualTint (GuiElement element, Colour fallback);

        public virtual Colour GetTint (GuiElement element, Colour fallback) {
            return ApplyTint (element) ? GetActualTint (element, fallback) : fallback;
        }
    }

    public sealed class NoTinting : Tinting {
        public static readonly NoTinting INSTANCE = new NoTinting ();

        protected override bool ApplyTint (GuiElement element) {
            return false;
        }

        protected override Colour GetActualTint (GuiElement element, Colour fallback) {
            throw new InvalidOperationException ("Cannot tint with NoTinting!");
        }
    }

    public sealed class BackgroundTinting : Tinting {
        public static readonly BackgroundTinting INSTANCE = new BackgroundTinting ();

        protected override bool ApplyTint (GuiElement element) {
            return element.Backgrounds != null;
        }

        protected override Colour GetActualTint (GuiElement element, Colour fallback) {
            return element.Backgrounds.Active.Colour;
        }

    }

    public sealed class ParentTinting : Tinting {
        public static readonly ParentTinting INSTANCE = new ParentTinting ();

        protected override bool ApplyTint (GuiElement element) {
            return element.Parent != null;
        }

        protected override Colour GetActualTint (GuiElement element, Colour fallback) {
            return element.Parent.Tinting.GetTint (element.Parent, fallback);
        }

    }

    public sealed class StaticTinting : Tinting {

        protected override bool ApplyTint (GuiElement element) {
            return true;
        }

        protected override Colour GetActualTint (GuiElement element, Colour fallback) {
            return _colour;
        }

        Colour _colour;

        public StaticTinting (Colour colour) {
            _colour = colour;
        }
    }
}

