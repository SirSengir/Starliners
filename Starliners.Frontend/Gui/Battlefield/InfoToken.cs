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
using BLibrary.Graphics.Text;
using Starliners.Game.Forces;
using BLibrary.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Starliners.Gui.Battlefield {
    /// <summary>
    /// Represents text information from hits.
    /// </summary>
    sealed class InfoToken : IBattleToken {

        const int SALVO_HITINFO_TIME = 40;
        static readonly IEnumerable<IBattleToken> EMPTY_TOKEN_LIST = new List<IBattleToken> ();

        public bool IsCompleted {
            get {
                return _elapsed >= SALVO_HITINFO_TIME;
            }
        }

        Vect2i _position;
        long _tick;
        double _elapsed = 0;

        TextBuffer _buffer;

        public InfoToken (Salvo salvo, Vect2i position)
            : this (salvo.Damage.GetInfo ().ToArray (), position, -1) {
        }

        public InfoToken (string[] text, Vect2i position, long tick) {
            _position = position;
            _tick = tick;
            _buffer = new TextBuffer (FontManager.Instance [FontManager.PARTICLE], text);
        }

        public void Render (RenderTarget target, RenderStates states) {
            if (IsCompleted) {
                return;
            }
            if (_tick > GameAccess.Interface.Local.Clock.Ticks) {
                return;
            }

            states.Transform.Translate (_position);
            states.Transform.Translate (_buffer.LocalBounds.Size / -2);
            states.Transform.Translate (0, -32 * Math.Sqrt (_elapsed / SALVO_HITINFO_TIME));
            target.Draw (_buffer, states);

            _elapsed++;
        }

        public IEnumerable<IBattleToken> GetSubsequentTokens () {
            return EMPTY_TOKEN_LIST;
        }
    }
}

