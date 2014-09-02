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

namespace Starliners {

    public static class Constants {

        public const string AssemblySimulator = "Starliners";

        public const string TITLE = "Starliners";
        public const string SAVE_SUFFIX = ".starliners";
        public const int TICKS_HEARTBEAT = 50;
        public const int TICKS_GUI_REFRESH = TICKS_HEARTBEAT * 2;

        public const string PATH_FONTS = "Fonts/";
        public const string PATH_SAVES = "Saves/";
        public const string PATH_SETTINGS = "Settings/";
        public const string PATH_LOG = "Log/";
        public const string PATH_CRASHES = "CrashReports/";
        public const string PATH_SCREENSHOTS = "Screenshots/";
        public const string PATH_PLUGINS = "Plugins/";
        public const string PATH_RESOURCES = "Resources/";
        public const string PATH_TEMP = "Temp/";

        /* Parameter Keys */

        public const string FILE_CRASH = "crash.log";

        public const string SPRITE_MISSING = "missing";

        /* Window Sizes */
        public static readonly Vect2i HUD_SIZE_TOP = new Vect2i (960, 0);

        // Control Schemes
        public const string CONTROL_SCHEME_GUI_OPEN = "‡";
        public const string CONTROL_SCHEME_BREAK = "†";

        /* Container Data Keys */
        public const string FRAGMENT_GUI_HEADER = "gui.header";
        public const string FRAGMENT_GUI_ICON = "gui.icon";

        /* Container Action Keys */
        public const string CONTAINER_KEY_BTN_CLOSE_WINDOW = "button.close";
        public const string CONTAINER_KEY_BTN_DETAILS = "button.details";
        public const string CONTAINER_KEY_BTN_POSITION = "button.position";
        public const string CONTAINER_KEY_BTN_HELP = "button.help";
        public const string CONTAINER_KEY_BTN_PROMPT_TAG = "button.tag";
        public const string CONTAINER_KEY_BTN_COMPACT = "button.compact";
        public const string CONTAINER_KEY_TAB_PAGE_SELECT = "button.page.select";
        public const string CONTAINER_KEY_PROMPT_TAG = "prompt.tag";
        public const string CONTAINER_KEY_BLAZON_SAVE = "blazon.save";
        public const string CONTAINER_KEY_TAB_SWITCH = "tab.switch";

        /* Table Defines */
        public static readonly Colour TABLE_CATEGORY = new Colour (Colour.LightGray, 64);
        public static readonly Colour TABLE_SELECTION = new Colour (Colour.White, 64);
        public static readonly Colour TABLE_HOVER = new Colour (Colour.White, 32);
        public static readonly Colour TABLE_ALTERNATING = new Colour (Colour.White, 16);
        public const int TABLE_ROW_HEIGHT = 40;

    }
}

