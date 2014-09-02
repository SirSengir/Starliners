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
using BLibrary.Util;

namespace Starliners.Game.Forces {
    public sealed class ShipProjector : IRenderableEntity, ISpriteDeclarant {

        #region Properties

        public bool RenderChanged {
            get;
            set;
        }

        public int RenderHash {
            get {
                return _hash;
            }
        }

        public RenderFlags RenderFlags {
            get {
                return RenderFlags.None;
            }
        }

        public RenderHint RenderHint {
            get {
                return RenderHint.Static;
            }
        }

        public Vect2f BoundingSize {
            get {
                return new Vect2i (1, 1);
            }
        }

        public ShipState State {
            get {
                return _state;
            }
            set {
                if (value != _state) {
                    _state = value;
                    RenderChanged = true;
                }
            }
        }

        #endregion

        #region Fields

        string _skin;
        Colour _colour0;
        Colour _colour1;
        int _hash;

        ShipState _state = ShipState.Shielded;
        uint[] _icons;

        #endregion

        public ShipProjector (string skin, Colour colour0, Colour colour1, int hash)
            : this (colour0, colour1, hash) {
            _skin = skin;
        }

        public ShipProjector (uint[] icons, Colour colour0, Colour colour1, int hash)
            : this (colour0, colour1, hash) {
            _icons = icons;
        }

        ShipProjector (Colour colour0, Colour colour1, int hash) {
            _colour0 = colour0;
            _colour1 = colour1;
            _hash = hash;
        }

        public void MakeSane () {
        }

        public int GetCurrentReel (ModelPart part) {
            return 0;
        }

        public ModelReel[] GetReels (ModelPart part) {

            if (part == ModelPart.Flavour) {
                return new ModelReel[] { new ModelReel (new IconLayer (_icons [3])) { Anchor = Anchor.Center } };

            } else if (part == ModelPart.Sprite) {

                if (_state == ShipState.Wreck) {
                    return new ModelReel[] { new ModelReel (new IconLayer (_icons [6], _colour0)) { Anchor = Anchor.Center } };
                } else {
                    return new ModelReel[] { new ModelReel (new IconLayer (_icons [0], _colour0), new IconLayer (_icons [1])) { Anchor = Anchor.Center } };
                }
            }

            // Now damage overlay.
            switch (_state) {
                case ShipState.ArmourDamage:
                    return new ModelReel[] { new ModelReel (new IconLayer (_icons [4], _colour0)) { Anchor = Anchor.Center } };
                case ShipState.HullDamage:
                    return new ModelReel[] { new ModelReel (new IconLayer (_icons [4], _colour0), new IconLayer (_icons [5])) { Anchor = Anchor.Center } };
                default:
                case ShipState.Shielded:
                    return new ModelReel[] { new ModelReel (new IconLayer (_icons [2], _colour1)) { Anchor = Anchor.Center } };
            }
        }

        public bool HasPart (ModelPart part) {
            if (_state == ShipState.Wreck) {
                return part == ModelPart.Sprite;
            }
            return part == ModelPart.Sprite || part == ModelPart.Flavour || (_state != ShipState.None && part == ModelPart.Damage);
        }

        public void RegisterIcons (IIconRegister register) {
            _icons = register.RegisterIcon (_skin);
        }

        public ShipProjector Copy (int newHash) {
            ShipProjector copy = new ShipProjector (_skin, _colour0, _colour1, newHash);
            copy._icons = _icons;
            copy._state = _state;
            return copy;
        }
    }
}

