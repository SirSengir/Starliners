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
using System.Runtime.Serialization;

namespace BLibrary.Util {
    [Serializable]
    public sealed class Credentials : ISerializable {

        public bool IsAnon {
            get;
            private set;
        }

        public string Login {
            get;
            private set;
        }

        public string Token {
            get;
            private set;
        }

        public Credentials (string login) {
            IsAnon = true;
            Login = login;
        }

        public Credentials (string login, string token) {
            IsAnon = string.IsNullOrEmpty (token);
            Login = login;
            Token = token;
        }

        public Credentials (SerializationInfo info, StreamingContext context) {
            IsAnon = info.GetBoolean ("IsAnon");
            Login = info.GetString ("Login");
            Token = info.GetString ("Token");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context) {
            info.AddValue ("IsAnon", IsAnon);
            info.AddValue ("Login", Login);
            info.AddValue ("Token", Token);
        }
    }
}

