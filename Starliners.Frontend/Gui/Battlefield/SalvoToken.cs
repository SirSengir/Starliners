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
using BLibrary.Audio;
using System.Collections.Generic;

namespace Starliners.Gui.Battlefield {
    /// <summary>
    /// Represents fire salvos flying around the battlefield.
    /// </summary>
    sealed class SalvoToken : IBattleToken {

        #region Constants

        static readonly Vect2i ICON_SIZE = BattlefieldViewer.ICON_SIZE;
        const int SALVO_FLIGHT_TIME = 100;

        #endregion

        public bool IsCompleted {
            get {
                return _elapsed >= SALVO_FLIGHT_TIME;
            }
        }

        Salvo _salvo;
        Drawable _drawable;

        Vect2i _start;
        Vect2i _end;
        Vect2i _delta;
        double _elapsed = 0;

        bool _sounded;

        public SalvoToken (Salvo salvo, Vect2i start, Vect2i end) {
            _salvo = salvo;

            _start = start;
            _end = end;

            _end = _end + new Vect2i (GameAccess.Interface.Local.Rand.Next (32), GameAccess.Interface.Local.Rand.Next (16)) - new Vect2i (16, 8);
            _delta = _end - _start;
        }

        public void Render (RenderTarget target, RenderStates states) {
            if (IsCompleted) {
                return;
            }
            if (_salvo.Tick > GameAccess.Interface.Local.Clock.Ticks) {
                return;
            } else if (_drawable == null) {
                _drawable = SpriteManager.Instance [SpriteManager.Instance.RegisterSingle (_salvo.Shot.Kind.ToString ().ToLowerInvariant ()), _salvo.Colour];
            }

            if (!_sounded) {
                _sounded = true;
                SoundManager.Instance.Play (SoundKeys.LASER_FIRE);
            }

            Vect2d pos = new Vect2d (CalculateLinearFlight (_elapsed, SALVO_FLIGHT_TIME, _start.X, _delta.X),
                             CalculateLinearFlight (_elapsed, SALVO_FLIGHT_TIME, _start.Y, _delta.Y));
            states.Transform.Translate (pos);

            //states.Transform.Rotate (360 * (MathUtils.CalcAngle (_start, _end) + Math.PI / 2));
            target.Draw (_drawable, states);

            _elapsed++;
        }

        double CalculateLinearFlight (double timeElapsed, double totalTime, double start, double delta) {
            return start + (delta / totalTime) * timeElapsed;
        }

        public IEnumerable<IBattleToken> GetSubsequentTokens () {
            return _salvo.Damage.NoEffect ? new List<IBattleToken> () {
                new InfoToken (new string[] { "Missed!" }, _end, -1)
            }
                : new List<IBattleToken> () {
                new ImpactToken (_salvo, _end),
                new InfoToken (_salvo, _end)
            };
        }

        public static Vect2i GetStartPoint (Vect2i gridcoord, int middle, bool mirrored) {
            Vect2i start = gridcoord;

            if (mirrored) {
                start = new Vect2i (BattleGrid.MAX_COLUMNS - 1, 8) - start;
                start += new Vect2i (BattleGrid.MAX_COLUMNS, 0);

                start *= ICON_SIZE;
                start += new Vect2i (middle, 0);
            } else {
                start = new Vect2i (BattleGrid.MAX_COLUMNS - 1, 8) - start;
                start *= ICON_SIZE;
            }

            return (Vect2i)(start + ICON_SIZE / 2);
        }

        public static Vect2i GetEndPoint (Vect2i gridcoord, int middle, bool mirrored) {
            Vect2i end = gridcoord;

            if (mirrored) {
                end = new Vect2i (BattleGrid.MAX_COLUMNS - 1, 8) - end;
                end *= ICON_SIZE;
            } else {
                end = new Vect2i (BattleGrid.MAX_COLUMNS - 1, 8) - end;
                end += new Vect2i (BattleGrid.MAX_COLUMNS, 0);

                end *= ICON_SIZE;
                end += new Vect2i (middle, 0);
            }

            return (Vect2i)(end + ICON_SIZE / 2);
        }
    }

}

