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

using BLibrary.Util;

namespace BLibrary.Gui.Widgets {

    public sealed class ListItemText : Widget {
        #region Properties

        public Alignment AlignmentH {
            get;
            set;
        }

        public Alignment AlignmentV {
            get;
            set;
        }

        #endregion

        string _text;

        public ListItemText (Vect2i position, Vect2i size, string key, string text)
            : base (position, size, key) {
            _text = text;
            AlignmentV = Alignment.Center;
        }

        protected override void Regenerate () {
            base.Regenerate ();

            AddWidget (new Label (new Vect2i (2, 0), Size, _text) {
                AlignmentH = AlignmentH,
                AlignmentV = AlignmentV
            });
        }
    }
}

