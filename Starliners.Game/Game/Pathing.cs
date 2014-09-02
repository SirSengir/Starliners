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

namespace Starliners.Game {
    sealed class Pathing {

        INavPoint _destination;

        public Movement UpdateMovement (IMobile mobile) {
            // Keep standing still if there is no destination on the entity.
            if (mobile.Destination == null) {
                _destination = null;
                return Movement.NEUTRAL;
            }

            // If our own cached destination is not set or different from the entity's actual one,
            // reset to the entity's.
            if (_destination == null || _destination.Serial != mobile.Destination.Serial) {
                _destination = mobile.Destination;
            }

            // Stop if the path has been traversed.
            if (_destination.CollisionBox.IntersectsWith ((Vect2f)mobile.Location)) {
                _destination = null;
                mobile.OnDestinationReached ();
                return Movement.NEUTRAL;
            }

            // Calculate movement and return it.
            double angle = MathUtils.CalcAngle (mobile.Location, _destination.Location);
            return new Movement (CalcSpeed (angle, 0.02f), angle);
        }

        Vect2d CalcSpeed (double angle, float maxSpeed) {
            double scaleX = Math.Cos (angle);
            double scaleY = Math.Sin (angle);

            return new Vect2d (maxSpeed * scaleX, maxSpeed * scaleY);
        }
    }
}

