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
using System.Collections.Generic;
using BLibrary.Util;
using BLibrary.Graphics;
using System.Linq;
using BLibrary.Graphics.Text;

namespace BLibrary.Gui.Interface {
    public class GuiChat : GuiLocal {

        static GuiChat _instance;

        public static GuiChat Instance {
            get {
                if (_instance == null) {
                    _instance = new GuiChat ();
                }
                return _instance;
            }
        }

        #region Constants

        static readonly Vect2i WINDOW_SIZE = new Vect2i (960, 256);
        static readonly WindowPresets WINDOW_SETTING = new WindowPresets ("ig_chat", WINDOW_SIZE, Positioning.UpperRight, true) {
            Style = string.Empty
        };

        #endregion

        #region Classes

        sealed class ChatMessage {
            public readonly long TimeStamp;
            public readonly TextComposition Text;

            public TextBuffer Buffer;

            public ChatMessage (TextComposition text) {
                TimeStamp = DateTime.Now.Ticks;
                Text = text;
            }
        }

        #endregion

        List<ChatMessage> _messages = new List<ChatMessage> ();

        public GuiChat ()
            : base (WINDOW_SETTING) {
            IsCloseable = false;
            IsDisembodied = true;
        }

        public override void Draw (RenderTarget target, RenderStates states) {
            base.Draw (target, states);

            states.Transform.Translate (PositionAbsolute.X, PositionAbsolute.Y + Size.Y);
            foreach (ChatMessage message in _messages.Where(p => p.TimeStamp > DateTime.Now.Ticks - TimeSpan.TicksPerMinute).OrderByDescending(p => p.TimeStamp).Take(15)) {
                if (message.Buffer == null) {
                    message.Buffer = new TextBuffer (string.Format ("§+b§<{0}> {1}", new DateTime (message.TimeStamp).ToString ("h:mm"), message.Text.ToString ())) {
                        HAlign = Alignment.Right
                    };
                    message.Buffer.SetMaxWidth (Size.X);
                }

                states.Transform.Translate (0, -message.Buffer.LocalBounds.Height);
                target.Draw (message.Buffer, states);
            }
        }

        public void PushMessage (TextComposition composition) {
            _messages.Add (new ChatMessage (composition));
        }
    }
}

