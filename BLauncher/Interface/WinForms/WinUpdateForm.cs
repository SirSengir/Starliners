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
using System.Windows.Forms;
using System.Drawing;

namespace BLauncher.Interface.WinForms {
    public sealed class WinUpdateForm : Form, IUpdateWindow {

        public bool IsReady {
            get {
                return _isReady;
            }
            set {
                _isReady = value;
                if (_isReady) {
                    _btnUpdater.Text = "Start Game";
                } else {
                    _btnUpdater.Text = "Cancel";
                }
            }
        }

        public string Status {
            get {
                return Text;
            }
            set {
                Text = value;
            }
        }

        public int Progress {
            get {
                return _progressBar.Value;
            }
            set {
                _progressBar.Value = value;
            }
        }

        #region Fields

        bool _isReady;
        ProgressBar _progressBar;
        Button _btnUpdater;

        #endregion

        public WinUpdateForm () {
            Build ();

            _btnUpdater.Click += new EventHandler (OnBtnUpdaterClick);
        }

        void OnBtnUpdaterClick (object sender, EventArgs e) {
            if (IsReady) {
                LaunchInstance.Instance.Launch ();
            } else {
                MainClass.Interface.Quit ();
            }
        }

        void Build () {

            Text = string.Empty;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size (350, 66);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            SuspendLayout ();

            _progressBar = new ProgressBar ();
            _progressBar.Minimum = 0;
            _progressBar.Maximum = 100;
            _progressBar.Value = 0;
            _progressBar.Location = new Point (6, 6);
            _progressBar.Size = new Size (ClientSize.Width - 12, 24);
            Controls.Add (_progressBar);

            _btnUpdater = new Button ();
            _btnUpdater.Size = new Size (ClientSize.Width - 48, 24);
            _btnUpdater.Location = new Point ((ClientSize.Width - _btnUpdater.Size.Width) / 2, 36);
            _btnUpdater.Text = "Cancel";
            Controls.Add (_btnUpdater);

            ResumeLayout ();
        }
    }
}

