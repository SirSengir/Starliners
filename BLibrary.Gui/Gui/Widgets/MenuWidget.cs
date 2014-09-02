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

﻿using BLibrary.Graphics;
using BLibrary.Graphics.Text;
using BLibrary.Graphics.Sprites;
using BLibrary.Util;

namespace BLibrary.Gui.Widgets {

    public abstract class MenuWidget : Widget {
        #region Constants

        const int THROTTLE_RECIPE_UPDATE = 20;
        const int TEXT_ICON_SPACING = 16;
        const int ICON_EDGE = 32;

        #endregion

        #region Fields

        protected uint[] _iconIndeces;

        int _throttle;
        string _textString;
        readonly TextBuffer _buffer;
        readonly float _eigth;

        #endregion

        protected virtual int SpriteCount {
            get {
                return 1;
            }
        }

        public MenuWidget (Vect2i position, Vect2i size, string key, string symbol)
            : base (position, size, key) {

            IsSensitive = true;

            _iconIndeces = SpriteManager.Instance.RegisterIcon (symbol);
            Backgrounds = UIProvider.Styles ["hud"].ButtonStyle.CreateBackgrounds ();
            BackgroundStates = BG_STATES_SENSITIVE;

            _eigth = (float)(Size.X - 8) / 8;
            _buffer = new TextBuffer (0.ToString ());
            _buffer.Box = new Vect2i (_eigth * 5, Size.Y);
            _buffer.HAlign = Alignment.Center;
            _buffer.VAlign = Alignment.Center;
        }

        public override void Update () {
            base.Update ();
            _throttle++;
            if (_throttle < THROTTLE_RECIPE_UPDATE)
                return;

            _throttle = 0;
            UpdateCache ();
        }

        protected override void Regenerate () {
            base.Regenerate ();
            UpdateCache ();
        }

        void UpdateCache () {
            _textString = GenerateInfoText ();
        }

        protected virtual Sprite GetSymbolSprite (int index) {
            return SpriteManager.Instance [_iconIndeces [0]];
        }

        protected abstract string GenerateInfoText ();

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            states.Transform.Translate (PositionRelative + new Vect2i (4, 0));
            _buffer.Text = _textString;
            _buffer.Draw (target, states);

            int paddX = (Size.X - _buffer.Box.X - ICON_EDGE - 8) / 2;
            states.Transform.Translate (_eigth * 5 + paddX, (Size.Y - ICON_EDGE) / 2);
            for (int i = 0; i < SpriteCount; i++)
                target.Draw (GetSymbolSprite (i), states);
        }
    }
}
