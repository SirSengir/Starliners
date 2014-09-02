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
using Starliners.Game.Forces;
using BLibrary.Util;
using BLibrary.Graphics.Sprites;
using BLibrary.Graphics;
using System.Collections.Generic;

namespace Starliners.Gui.Battlefield {
    /// <summary>
    /// Represents impacts on the battle field.
    /// </summary>
    sealed class ImpactToken : IBattleToken {

        const int SALVO_EXPLOSION_TIME = 20;
        static readonly IEnumerable<IBattleToken> EMPTY_TOKEN_LIST = new List<IBattleToken> ();

        public bool IsCompleted {
            get {
                return _elapsed >= SALVO_EXPLOSION_TIME;
            }
        }

        uint _icon;
        AnimationClock _clock;

        Vect2i _position;
        double _elapsed = 0;
        double _radius = 0;

        bool _sounded;

        public ImpactToken (Salvo salvo, Vect2i position)
            : this (position, salvo.Damage.Penetrated == Penetration.Shield ? "impactShield0" : "impactHull0", salvo.Damage.Delivered) {
        }

        public ImpactToken (Vect2i position, string template, int damage) {
            _position = position;
            _icon = SpriteManager.Instance.RegisterSingle (template);
            _clock = new AnimationClock ((int)(SALVO_EXPLOSION_TIME * SpriteManager.Instance.LastFrameTime * TimeSpan.TicksPerSecond) / 2);

            double force = (double)damage / 500;
            _radius = (0.8f + 0.8 * (force > 1.0 ? 1.0 : force));
        }

        public void Render (RenderTarget target, RenderStates states) {
            if (IsCompleted) {
                return;
            }

            if (!_sounded) {
                _sounded = true;
                //SoundManager.Instance.Play (SoundKeys.LASER_FIRE);
            }

            states.Transform.Translate (_position);
            double scale = (0.2 + 0.8 * _elapsed / SALVO_EXPLOSION_TIME) * _radius;
            Drawable drawable = SpriteManager.Instance [_icon, _clock];
            states.Transform.Scale (new Vect2d (scale, scale), drawable.LocalBounds.Center);

            target.Draw (drawable, states);

            _elapsed++;
        }

        public IEnumerable<IBattleToken> GetSubsequentTokens () {
            return EMPTY_TOKEN_LIST;
        }
    }

}

