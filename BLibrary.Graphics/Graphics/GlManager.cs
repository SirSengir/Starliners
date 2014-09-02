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
using System.Collections.Concurrent;
using Starliners;

namespace BLibrary.Graphics {

    sealed class GlManager {
        static ConcurrentQueue<IDisposable> _orphans = new ConcurrentQueue<IDisposable> ();

        /// <summary>
        /// Enqueues an orphaned object which may contain OpenGL resources and need to be safely disposed off.
        /// </summary>
        /// <param name="disposable">Disposable.</param>
        public static void EnqueueOrphan (IDisposable orphan) {
            if (orphan == null) {
                throw new SystemException ("Cannot enqueue a null object as an orphan.");
            }
            _orphans.Enqueue (orphan);
        }

        /// <summary>
        /// Cleans up queued orphans, freeing unmanaged OpenGL resources.
        /// </summary>
        public static void CleanOrphans () {
            if (_orphans.Count <= 0) {
                return;
            }

            GameAccess.Interface.GameConsole.Rendering ("Cleaning up {0} orphans with unmanaged OpenGL resources.", _orphans.Count);
            IDisposable dequeued = null;
            bool didDequeue = false;
            while (_orphans.Count > 0) {
                didDequeue = _orphans.TryDequeue (out dequeued);
                if (didDequeue) {
                    dequeued.Dispose ();
                }
            }
        }
    }
}

