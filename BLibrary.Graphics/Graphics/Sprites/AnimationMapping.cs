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

using System;
using System.Collections.Generic;
using BLibrary.Graphics;
using BLibrary.Util;


namespace BLibrary.Graphics.Sprites {

    internal sealed class AnimationMapping : IconMapping {
        #region Classes

        sealed class AnimationStep {
            public readonly int Frame;
            public readonly long Time;

            public AnimationStep (int frame, long ticks) {
                Frame = frame;
                Time = ticks;
            }
        }

        #endregion

        #region Properties

        public override Sprite this [AnimationClock clock] {
            get {
                return _sprites [DetermineFrame (DateTime.UtcNow.Ticks - clock.Start, clock.Duration)];
            }
        }

        public override Sprite this [Colour colour] {
            get {
                return _sprites [DetermineFrame (DateTime.UtcNow.Ticks, -1)];
            }
        }

        public override Sprite this [int index] {
            get {
                return _sprites [index];
            }
        }

        public override Rect2i[] Parts {
            get {
                List<Rect2i> parts = new List<Rect2i> ();
                for (int i = 0; i < _sprites.Length; i++) {
                    parts.Add (_sprites [i].SourceRect);
                }
                return parts.ToArray ();
            }
        }

        #endregion

        Sprite[] _sprites;
        long _duration;
        AnimationStep[] _steps;

        public AnimationMapping (Texture texture, Vect2f position, Rect2i[] rects)
            : base (texture, rects.Length) {
            _sprites = new Sprite[rects.Length];
            for (int i = 0; i < rects.Length; i++) {
                _sprites [i] = new Sprite (texture);
                _sprites [i].Position = position;
                _sprites [i].SourceRect = rects [i];
            }

            _steps = new AnimationStep[_sprites.Length];
            int duration = 1000;
            for (int i = 0; i < _steps.Length; i++) {
                _steps [i] = new AnimationStep (i, duration * TimeSpan.TicksPerMillisecond / _steps.Length);
            }

            _duration = duration * TimeSpan.TicksPerMillisecond;
        }

        int DetermineFrame (long elapsed, int duration) {
            long current = elapsed % (duration > 0 ? duration : _duration);
            long count = 0;
            for (int i = 0; i < _steps.Length; i++) {
                count += _steps [i].Time;
                if (current < count) {
                    return _steps [i].Frame;
                }
            }

            return _steps [0].Frame;
        }

        public void SetAnimation (int[][] steps) {
            _steps = new AnimationStep[steps.GetLength (0)];
            long duration = 0;
            for (int i = 0; i < steps.GetLength (0); i++) {
                _steps [i] = new AnimationStep (steps [i] [0], steps [i] [1] * TimeSpan.TicksPerMillisecond);
                duration += _steps [i].Time;
            }

            _duration = duration;
        }

        public override void Adjust (Texture sheet, Rect2i[] parts) {
            base.Adjust (sheet, parts);
            _sprites = new Sprite[parts.Length];
            for (int i = 0; i < parts.Length; i++) {
                _sprites [i] = new Sprite (sheet);
                _sprites [i].SourceRect = parts [i];
            }
        }
    }
}
