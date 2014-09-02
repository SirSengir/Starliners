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

﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using BLibrary.Graphics;
using BLibrary.Gui;
using BLibrary.Util;
using Starliners.Game.Forces;
using BLibrary.Gui.Widgets;

namespace Starliners.Gui.Battlefield {
    sealed class BattlefieldViewer : Widget {

        #region Constants

        public static readonly Vect2i ICON_SIZE = new Vect2i (80, 40);

        static readonly Vect2i[] SLOTS_FORMATION = new Vect2i[] {
            new Vect2i (2, 4),
            new Vect2i (2, 3), new Vect2i (3, 3), new Vect2i (3, 4), new Vect2i (3, 5), new Vect2i (2, 5), new Vect2i (1, 5), new Vect2i (1, 4), new Vect2i (1, 3),
            new Vect2i (2, 2), new Vect2i (3, 2), new Vect2i (4, 2), new Vect2i (4, 3), new Vect2i (4, 4), new Vect2i (4, 5), new Vect2i (4, 6),
            new Vect2i (3, 6), new Vect2i (2, 6), new Vect2i (1, 6), new Vect2i (0, 6), new Vect2i (0, 5), new Vect2i (0, 4), new Vect2i (0, 3), new Vect2i (0, 2), new Vect2i (1, 2),

            new Vect2i (2, 1), new Vect2i (2, 7), new Vect2i (1, 1), new Vect2i (1, 7), new Vect2i (3, 1), new Vect2i (3, 7),
            new Vect2i (2, 0), new Vect2i (2, 8), new Vect2i (3, 0), new Vect2i (3, 8),
            new Vect2i (4, 1), new Vect2i (4, 7), new Vect2i (4, 0), new Vect2i (4, 8),
            new Vect2i (1, 0), new Vect2i (1, 8), new Vect2i (0, 1), new Vect2i (0, 7),
            new Vect2i (0, 0), new Vect2i (0, 8)
        };

        const string SHADER_ALPHA_CHANGE = @"uniform float intensity;
uniform sampler2D texture0;

void main() {
    vec4 textureColor = texture2D(texture0, gl_TexCoord[0].st);

    gl_FragColor = vec4(textureColor.rgb*gl_Color.rgb, textureColor.a*intensity);
}";

        #endregion

        #region Fields

        DataReference<BattleGrid> _attacker;
        DataReference<BattleGrid> _defender;

        ShipSlot[] _attacking;
        ShipSlot[] _defending;

        List<IBattleToken> _tokens = new List<IBattleToken> ();
        Queue<IBattleToken> _queuedTokens = new Queue<IBattleToken> ();

        long _lastShotTick;

        internal static Shader Alpha;
        internal static int MiddleColumnWidth;

        #endregion

        public BattlefieldViewer (Vect2i position, Vect2i size, DataReference<BattleGrid> attacker, DataReference<BattleGrid> defender)
            : base (position, size) {
            _attacker = attacker;
            _defender = defender;

            _attacking = CreateShipSlots (_attacker, false);
            _defending = CreateShipSlots (_defender, true);

            if (Alpha == null) {
                Alpha = new Shader (ShaderType.FragmentShader, new MemoryStream (System.Text.Encoding.UTF8.GetBytes (SHADER_ALPHA_CHANGE)));
            }
        }

        ShipSlot[] CreateShipSlots (DataReference<BattleGrid> grid, bool mirrored) {
            ShipSlot[] created = new ShipSlot[grid.Value.MaxCount];
            for (int i = 0; i < created.Length; i++) {
                created [i] = new ShipSlot (i, SLOTS_FORMATION [i], grid, mirrored);
            }
            return created;
        }

        public override void Update () {
            base.Update ();

            MiddleColumnWidth = Size.X - (2 * BattleGrid.MAX_COLUMNS * ICON_SIZE.X);

            // Let ships spawn additional particles.
            for (int i = 0; i < _attacking.Length; i++) {
                foreach (IBattleToken spawned in _attacking[i].GetSpawnedTokens()) {
                    _queuedTokens.Enqueue (spawned);
                }
            }
            for (int i = 0; i < _defending.Length; i++) {
                foreach (IBattleToken spawned in _defending[i].GetSpawnedTokens()) {
                    _queuedTokens.Enqueue (spawned);
                }
            }
            // Manage battle tokens
            foreach (IBattleToken token in _tokens.Where(p => p.IsCompleted)) {
                foreach (IBattleToken subsequent in token.GetSubsequentTokens()) {
                    _queuedTokens.Enqueue (subsequent);
                }
            }
            _tokens.RemoveAll (delegate(IBattleToken token) {
                return token.IsCompleted;
            });
            while (_queuedTokens.Count > 0) {
                _tokens.Add (_queuedTokens.Dequeue ());
            }

            // Create new salvo tokens as needed.
            long stick = _lastShotTick;
            UpdateLastTick (DistributeShots (stick, _attacker.Value, _attacking, _defending, true));
            UpdateLastTick (DistributeShots (stick, _defender.Value, _defending, _attacking, false));

            // Create new regen tokens
            UpdateLastTick (DistributeSupport (stick, _attacker.Value, _attacking, true));
            UpdateLastTick (DistributeSupport (stick, _defender.Value, _defending, false));
        }

        void UpdateLastTick (long tick) {
            _lastShotTick = tick > _lastShotTick ? tick : _lastShotTick;
        }

        long DistributeShots (long stick, BattleGrid grid, ShipSlot[] origin, ShipSlot[] target, bool mirrored) {
            long lastTick = 0;
            foreach (Salvo salvo in grid.Salvos.Where(p => p.Tick > stick)) {
                Vect2i start = origin [salvo.OriginSlot].Position;
                Vect2i end = target [salvo.TargetSlot].Position;

                _tokens.Add (new SalvoToken (salvo,
                    SalvoToken.GetStartPoint (start, MiddleColumnWidth, mirrored),
                    SalvoToken.GetEndPoint (end, MiddleColumnWidth, mirrored)));

                lastTick = salvo.Tick > lastTick ? salvo.Tick : lastTick;
            }

            return lastTick;
        }

        long DistributeSupport (long stick, BattleGrid grid, ShipSlot[] ships, bool mirrored) {
            long lastTick = 0;
            foreach (Regen support in grid.Support.Where(p => p.Tick > stick)) {
                _tokens.Add (new InfoToken (new string[] { string.Format ("{0}{1}", Colour.LightPink.ToString ("#§"), support.Healed) },
                    SalvoToken.GetEndPoint (ships [support.OriginSlot].Position, MiddleColumnWidth, mirrored), support.Tick));
                lastTick = support.Tick > lastTick ? support.Tick : lastTick;
            }
            return lastTick;
        }

        protected override void Regenerate () {
            base.Regenerate ();
            AddWidget (new Canvas (Vect2i.ZERO, Size, "battlefield0"));
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);
            states.Transform.Translate (PositionRelative);

            target.EnableScissor (new Rect2i (PositionAbsolute, Size));
            for (int i = 0; i < _attacking.Length; i++) {
                _attacking [i].Render (target, states);
            }
            RenderStates sstates = states;
            sstates.Transform.Translate (Size.X - (BattleGrid.MAX_COLUMNS * ICON_SIZE.X), 0);
            for (int i = 0; i < _defending.Length; i++) {
                _defending [i].Render (target, sstates);
            }

            for (int i = 0; i < _tokens.Count; i++) {
                _tokens [i].Render (target, states);
            }
            target.DisableScissor ();
        }

        public override Tooltip GetTooltip (Vect2i coordinates) {
            Tooltip tooltip = null;

            foreach (ShipSlot slot in _attacking) {
                Rect2i area = new Rect2i (slot.Position * ICON_SIZE + PositionAbsolute, ICON_SIZE);
                if (area.IntersectsWith (coordinates)) {
                    tooltip = slot.GetTooltip ();
                    slot.TooltipArea = area;
                    break;
                }
            }

            foreach (ShipSlot slot in _defending) {
                Rect2i area = new Rect2i (slot.Position * ICON_SIZE + PositionAbsolute + new Vect2i (BattleGrid.MAX_COLUMNS * ICON_SIZE.X + MiddleColumnWidth, 0), ICON_SIZE);
                if (area.IntersectsWith (coordinates)) {
                    tooltip = slot.GetTooltip ();
                    slot.TooltipArea = area;
                    break;
                }
            }

            return tooltip ?? base.GetTooltip (coordinates);
        }
    }
}

