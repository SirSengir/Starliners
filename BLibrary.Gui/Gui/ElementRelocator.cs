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

    delegate float EaseCalculate (float timeElapsed, float start, float delta, float totalTime);
    sealed class ElementRelocator {
        public Vect2i Destination {
            get;
            private set;
        }

        EaseCalculate _calculate;
        Vect2i _start;
        Vect2i _delta;
        float _elapsedTime;
        float _totalTime;

        public ElementRelocator (GuiElement element, Vect2f destination, Easing easing) {
            switch (easing) {
                case Easing.ElasticEaseOut:
                    _calculate = new EaseCalculate (ElasticEaseOut);
                    break;
                default:
                    _calculate = new EaseCalculate (Linear);
                    break;
            }

            Destination = (Vect2i)destination;
            _start = element.PositionRelative;
            _delta = Destination - element.PositionRelative;
            _totalTime = (float)(Math.Max (Math.Abs (_delta.X), Math.Abs (_delta.Y))) * 0.25f;
        }

        public bool Move (GuiElement element) {
            if (_elapsedTime == _totalTime) {
                element.PositionRelative = Destination;
                return true;
            }

            element.PositionRelative = (Vect2i)CalculateCurrent ();
            _elapsedTime++;

            return element.PositionRelative == Destination;
        }

        Vect2f CalculateCurrent () {
            return new Vect2f (_calculate (_elapsedTime, _start.X, _delta.X, _totalTime),
                _calculate (_elapsedTime, _start.Y, _delta.Y, _totalTime));
        }

        static float ElasticEaseOut (float timeElapsed, float start, float delta, float totalTime) {
            if ((timeElapsed /= totalTime) == 1) {
                return delta + start;
            }

            float p = totalTime * 0.3f;
            float s = p / 4;

            return (int)(delta * Math.Pow (2, -10 * timeElapsed) * Math.Sin ((timeElapsed * totalTime - s) * (2 * Math.PI) / p) + delta + start);
        }

        static float Linear (float timeElapsed, float start, float delta, float totalTime) {
            return start + delta * (timeElapsed / totalTime);
        }
    }
}

