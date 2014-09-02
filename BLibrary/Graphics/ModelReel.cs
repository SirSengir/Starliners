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

using System.Collections.Generic;
using BLibrary.Util;

namespace BLibrary.Graphics {

    /// <summary>
    /// Contains abstract information to construct a Sprite from.
    /// </summary>
    public sealed class ModelReel {
        #region Constants

        public static readonly ModelReel[] EMPTY_ARRAY = new ModelReel[0];

        #endregion

        #region Properties

        public Anchor Anchor {
            get;
            set;
        }

        public Vect2f Translation {
            get;
            set;
        }

        public IconLayer[] Layers {
            get;
            private set;
        }

        #endregion

        public ModelReel (params IconLayer[] layers) {
            Layers = layers;
        }

        public static ModelReel[] IconsToModelReels (IconLayer[] icons) {
            List<ModelReel> reels = new List<ModelReel> ();
            for (int i = 0; i < icons.Length; i++) {
                reels.Add (new ModelReel (icons [i]));
            }
            return reels.ToArray ();
        }
    }
}

