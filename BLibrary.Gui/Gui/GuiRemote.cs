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

﻿using BLibrary.Resources;
using BLibrary.Graphics;
using BLibrary.Graphics.Text;
using BLibrary.Graphics.Sprites;
using BLibrary.Gui.Data;
using Starliners.Game;
using BLibrary.Util;
using BLibrary.Gui.Tooltips;
using Starliners;
using Starliners.States;
using System;
using BLibrary.Gui.Backgrounds;
using BLibrary.Gui.Interface;

namespace BLibrary.Gui {

    /// <summary>
    /// A container gui is associated with a Container object providing the actual gui data and logic.
    /// </summary>
    public abstract class GuiRemote : GuiWindow, IDataContainer {

        #region Properties

        /// <summary>
        /// Returns the container associated with this gui.
        /// </summary>
        public Container Container {
            get {
                return GameAccess.Interface.ThePlayer.ContainerManager [_containerId];
            }
        }

        public IDataProvider DataProvider {
            get {
                return Container;
            }
        }

        protected override bool CanDraw {
            get {
                return !Container.NeedsOpening;
            }
        }

        /// <summary>
        /// Indicates whether entity status should be rendered or not.
        /// </summary>
        /// <value><c>true</c> if display status; otherwise, <c>false</c>.</value>
        protected bool DisplayStatus {
            get {
                return _displayStatus && !IsCompacted;
            }
            set {
                _displayStatus = value;
            }
        }

        #endregion

        #region Fields

        int _containerId;
        bool _displayStatus = true;

        TextBuffer _buffer;
        EntityStatus.StatusLevel _laststatus;
        Background _statusbg;

        #endregion

        #region Constructor

        public GuiRemote (WindowPresets setting, int containerId)
            : base (setting) {
            _containerId = containerId;
            DisplayStatus = true;
            Subscribe (Container);
        }

        #endregion

        #region Updating

        public override void Update () {
            base.Update ();
            if (Container.BringToFront) {
                Container.BringToFront = false;
                IsCompacted = false;
                GuiManager.Instance.BringToFront (this);
            }

            if (IsUpdated && IsCompacted) {
                UpdateIcon ();
            }
        }

        #endregion

        #region Rendering

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            if (!DisplayStatus) {
                return;
            }
            EntityStatus status = DataProvider.GetValue<EntityStatus> (Container.KEY_STATUS);
            if (status == null || status.Level == EntityStatus.StatusLevel.None) {
                return;
            }

            if (status.Level != _laststatus) {
                _statusbg = UIProvider.Backgrounds [status.Level.ToString ().ToLowerInvariant ()].Copy ();
                _laststatus = status.Level;
            }
            states.Transform.Translate (PositionAbsolute.X + Size.X + 16, PositionAbsolute.Y);

            Vect2i boxsize = new Vect2i ((1024 - 16 - Size.X) / 2, Size.Y);
            _statusbg.Render (boxsize, target, states, _statusbg.Colour);

            if (_buffer == null) {
                _buffer = new TextBuffer (string.Format ("§#ffff00§{0}", Localization.Instance [status.Message]), Localization.Instance [status.Message + "_help"]);
                _buffer.Box = boxsize - new Vect2i (32, 16);
            } else {
                _buffer.SetLines (string.Format ("§#ffff00§{0}", Localization.Instance [status.Message]), Localization.Instance [status.Message + "_help"]);
            }

            states.Transform.Translate (16, 8);
            _buffer.Draw (target, states);
        }

        void UpdateIcon () {
            if (!DataProvider.HasFragment (Constants.FRAGMENT_GUI_ICON))
                return;

            string icon = DataProvider.GetValue<string> (Constants.FRAGMENT_GUI_ICON);
            Icon = new IconLayer (SpriteManager.Instance.RegisterSingle (icon));

        }

        #endregion

        #region Tooltip

        protected override Tooltip CompactTooltip {
            get {
                if (!DataProvider.HasFragment (Constants.FRAGMENT_GUI_HEADER))
                    return base.CompactTooltip;

                return new TooltipSimple (DataProvider.GetValue<string> (Constants.FRAGMENT_GUI_HEADER), TT_COMPACT_HELP) { Parent = this };

            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// Executes a gui action by sending it to the local or remote container.
        /// </summary>
        /// <param name='key'>
        /// Key.
        /// </param>
        /// <param name='args'>
        /// Arguments.
        /// </param>
        public override bool DoAction (string key, params object[] args) {
            if (Constants.CONTAINER_KEY_BTN_PROMPT_TAG.Equals (key)) {
                GuiManager.Instance.OpenGui (new GuiPrompt (this, "prompt.tag", new TextComponent ("prompt_tag") { IsLocalizable = true }, new TextComponent ("btn_ok") { IsLocalizable = true }, new TextComponent ("enter_tag") { IsLocalizable = true }));
                return true;
            } else if (base.DoAction (key, args))
                return true;

            MapState.Instance.Controller.ActionGui (_containerId, key, args);
            return true;
        }

        protected override void Close () {
            GuiManager.Instance.CloseGui (_containerId);
        }

        protected override void OnCompactChange () {
            base.OnCompactChange ();
            UpdateIcon ();
        }

        #endregion
    }
}
