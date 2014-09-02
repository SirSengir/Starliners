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
using BLibrary.Gui;
using BLibrary.Util;
using Starliners.Game.Forces;
using BLibrary.Graphics;
using BLibrary;
using Starliners.Graphics;
using BLibrary.Graphics.Sprites;
using System.Collections.Generic;
using BLibrary.Gui.Tooltips;
using Starliners.Gui.Tooltips;

namespace Starliners.Gui.Battlefield {
    /// <summary>
    /// Represents a "battle slot" in a battlegrid which may or may not be occupied by a ship.
    /// </summary>
    sealed class ShipSlot : ITooltipController {

        #region Constants

        static readonly Vect2i ICON_SIZE = BattlefieldViewer.ICON_SIZE;
        const float FLYIN_DURATION = 40;
        const float VESSEL_WRECK_FADEOUT = 200;

        static readonly IEnumerable<IBattleToken> EMPTY_TOKEN_LIST = new List<IBattleToken> ();

        #endregion

        public Vect2i Position {
            get;
            private set;
        }

        public Rect2i TooltipArea {
            get;
            set;
        }

        ulong _lastSerial;
        ShipState _lastState;
        float _elapsed;

        int _slot;
        bool _mirrored;
        Vect2i _amplRange;
        float _amplOffset;

        DataReference<BattleGrid> _grid;

        public ShipSlot (int slot, Vect2i position, DataReference<BattleGrid> grid, bool mirrored) {
            _slot = slot;
            Position = position;
            _grid = grid;

            _mirrored = mirrored;
            if (_mirrored) {
                Position = new Vect2i (BattleGrid.MAX_COLUMNS - 1, 8) - Position;
            }

            _amplOffset = (float)GameAccess.Interface.Local.Rand.NextDouble ();
        }

        public void Render (RenderTarget target, RenderStates states) {
            ShipInstance ship = _grid.Value [_slot];
            if (ship == null) {
                _lastSerial = LibraryConstants.NULL_ID;
                return;
            }

            if (ship.Serial != _lastSerial) {
                _lastSerial = ship.Serial;
                _elapsed = 0;
                ResetAmplitudeRange ();
            }
            if (_lastState != ship.State) {
                _lastState = ship.State;
                _elapsed = 0;
            }

            float flight = GameAccess.Interface.Local.Clock.Ticks - ship.LastJoined;
            if (flight < FLYIN_DURATION) {
                double offset = (BattleGrid.MAX_COLUMNS * ICON_SIZE.X) + ICON_SIZE.X;
                double travelled = CalculateFlightShift (flight, FLYIN_DURATION, 0, offset);
                if (_mirrored) {
                    states.Transform.Translate (offset - travelled, 0);
                } else {
                    states.Transform.Translate (-offset + travelled, 0);
                }
            } else if (ship.State == ShipState.Wreck) {
                // Fade out destroyed vessels
                float fade = 1f - _elapsed / VESSEL_WRECK_FADEOUT;
                BattlefieldViewer.Alpha.SetUniform ("intensity", fade > 0 ? fade : 0);
                states.Shader = BattlefieldViewer.Alpha;
            }

            states.Transform.Translate (Position * ICON_SIZE);
            states.Transform.Translate (ICON_SIZE / 2);

            float metronom = 0;
            switch (ship.ShipClass.Size) {
                case ShipSize.Frigate:
                    metronom = SpriteManager.Instance.Metronom1;
                    break;
                case ShipSize.Destroyer:
                case ShipSize.Cruiser:
                    metronom = SpriteManager.Instance.Metronom2;
                    break;
                default:
                    metronom = SpriteManager.Instance.Metronom3;
                    break;
            }
            double amplitude = Math.Sin (MathUtils.AsAmplitude ((metronom + _amplOffset) % 1f) * Math.PI);
            Vect2d jitter = _amplRange * 2 * amplitude - _amplRange;
            states.Transform.Translate (jitter);

            float scale = (float)ICON_SIZE.X / 32;
            states.Transform.Scale (scale, scale);

            if (_mirrored) {
                states.Transform.Scale (-1, 1);
            }

            RendererVessel.Instance.DrawRenderable (target, states, ship.Projector);

            _elapsed++;
        }

        public IEnumerable<IBattleToken> GetSpawnedTokens () {
            ShipInstance ship = _grid.Value [_slot];
            if (ship == null || ship.State != ShipState.Wreck || GameAccess.Interface.Local.Rand.NextDouble () >= 0.1) {
                return EMPTY_TOKEN_LIST;
            }

            // Spawn explosions
            Random rand = GameAccess.Interface.Local.Rand;
            Vect2i position = (Vect2i)(Position * ICON_SIZE + new Vect2i (rand.Next (ICON_SIZE.X), rand.Next (ICON_SIZE.Y)));
            if (_mirrored) {
                position += new Vect2i (BattleGrid.MAX_COLUMNS, 0) * ICON_SIZE;
                position += new Vect2i (BattlefieldViewer.MiddleColumnWidth, 0);
            }
            return new List<IBattleToken> () { new ImpactToken (position, "impactHull4", rand.Next (400 * (int)ship.ShipClass.Size)) };
        }

        void ResetAmplitudeRange () {
            _amplRange = new Vect2i (GameAccess.Interface.Local.Rand.Next (4), GameAccess.Interface.Local.Rand.Next (4));
        }

        double CalculateFlightShift (double timeElapsed, double totalTime, double start, double delta) {
            return start + delta * Math.Sqrt (Math.Sqrt (timeElapsed / totalTime));
        }

        public Tooltip GetTooltip () {
            ShipInstance ship = _grid.Value [_slot];
            if (ship == null) {
                return null;
            }

            return new TooltipShip (_slot, _grid) { Controller = this };
        }

        public bool SustainsTooltip (Vect2i coordinates) {
            return TooltipArea.IntersectsWith (coordinates) && _grid.Value [_slot] != null;
        }
    }

}

