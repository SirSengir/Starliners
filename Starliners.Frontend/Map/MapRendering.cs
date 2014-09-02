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
using Starliners.Graphics;

namespace Starliners.Map {

    public sealed class MapRendering {
        public static MapRendering Instance {
            get;
            set;
        }

        public IReadOnlyDictionary<ushort, IObjectRenderer> ObjectRenderers {
            get {
                return _objectRenderers;
            }
        }

        public IReadOnlyDictionary<ushort, IParticleRenderer> ParticleRenderers {
            get {
                return _particleRenderers;
            }
        }

        public IReadOnlyDictionary<ushort, ITagRenderer> TagRenderers {
            get {
                return _tagRenderers;
            }
        }

        Dictionary<ushort, IObjectRenderer> _objectRenderers = new Dictionary<ushort, IObjectRenderer> ();
        Dictionary<ushort, IParticleRenderer> _particleRenderers = new Dictionary<ushort, IParticleRenderer> ();
        Dictionary<ushort, ITagRenderer> _tagRenderers = new Dictionary<ushort, ITagRenderer> ();

        public void RegisterRenderer (ushort index, IObjectRenderer renderer) {
            if (_objectRenderers.ContainsKey (index)) {
                throw new SystemException ("Cannot re-register a map renderer with index " + index);
            }
            _objectRenderers [index] = renderer;
        }

        public void RegisterRenderer (ushort index, IParticleRenderer renderer) {
            if (_particleRenderers.ContainsKey (index)) {
                throw new SystemException ("Cannot re-register a particle renderer with index " + index);
            }
            _particleRenderers [index] = renderer;
        }

        public void RegisterRenderer (ushort index, ITagRenderer renderer) {
            if (_tagRenderers.ContainsKey (index)) {
                throw new SystemException ("Cannot re-register a tag renderer with index " + index);
            }
            _tagRenderers [index] = renderer;
        }

        public void OnAtlasRegeneration () {
            foreach (IObjectRenderer renderer in _objectRenderers.Values) {
                renderer.OnAtlasRegeneration ();
            }
            foreach (IParticleRenderer renderer in _particleRenderers.Values) {
                renderer.OnAtlasRegeneration ();
            }
            foreach (ITagRenderer renderer in _tagRenderers.Values) {
                renderer.OnAtlasRegeneration ();
            }

            RendererBlazon.Instance.OnAtlasRegeneration ();
        }

        public void OnFrameStart () {
            foreach (IObjectRenderer renderer in _objectRenderers.Values) {
                renderer.OnFrameStart ();
            }
        }

        public void OnRenderableRemoved (IRenderable renderable) {
            foreach (IObjectRenderer renderer in _objectRenderers.Values)
                renderer.OnRenderableRemoved (renderable);
        }

    }
}
