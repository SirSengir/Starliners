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
using System.IO;
using System.Linq;
using BLibrary;
using BLibrary.Graphics;
using BLibrary.Json;
using BLibrary.Resources;
using BLibrary.Util;
using Starliners;

namespace BLibrary.Graphics.Sprites {

    public sealed class SpriteManager : IIconRegister {
        #region Constants

        public const int MAX_RENDER_BUFFER = 2048;
        public const int TILE_DIMENSION = 32;

        #endregion

        public static SpriteManager Instance {
            get;
            set;
        }

        #region Properties

        public Random Rand {
            get;
            set;
        }

        public float Metronom0 {
            get;
            private set;
        }

        public float Metronom1 {
            get;
            private set;
        }

        public float Metronom2 {
            get;
            private set;
        }

        public float Metronom3 {
            get;
            private set;
        }

        public long Ticks {
            get;
            private set;
        }

        public double LastFrameTime {
            get;
            private set;
        }

        /// <summary>
        /// Retrieves the spritesheet for the given sheet and iconId.
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="iconId"></param>
        /// <returns></returns>
        public Sprite this [IconLayer icon] {
            get {
                return this [icon.Index, icon.Colour];
            }
        }

        /// <summary>
        /// Retrieves the spritesheet for the given sheet and iconId.
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="iconId"></param>
        /// <returns></returns>
        public Sprite this [uint index] {
            get {
                return this [index, Colour.White];
            }
        }

        /// <summary>
        /// Retrieves the spritesheet for the given sheet, iconId and animationSpeed.
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="iconId"></param>
        /// <returns></returns>
        public Sprite this [uint index, Colour colour] {
            get {
                IconMapping mapping = _atlas [index];
                if (mapping == null) {
                    return MissingSprite;
                }

                return mapping [colour];
            }
        }

        public Sprite this [uint index, AnimationClock clock] {
            get {
                IconMapping mapping = _atlas [index];
                if (mapping == null) {
                    return MissingSprite;
                }

                return mapping [clock];
            }
        }

        internal IconMapping GetMapping (IconLayer layer) {
            IconMapping mapping = _atlas [layer.Index];
            if (mapping == null) {
                return _atlas [_missingIndex];
            }

            return mapping;
        }

        public Sprite MissingSprite {
            get {
                return _atlas [_missingIndex] [Colour.White];
            }
        }

        public Sprite DummySprite {
            get {
                return _atlas [_dummyIndex] [Colour.White];
            }
        }

        #endregion

        #region Fields

        uint _lastIconIndex = 0;

        uint _missingIndex;
        uint _dummyIndex;
        SpriteAtlas _atlas;

        readonly double _metronom1Measure = Math.Pow (10, 7) * 2;
        readonly double _metronom2Measure = Math.Pow (10, 7) * 16;
        readonly double _metronom3Measure = Math.Pow (10, 7) * 32;

        Dictionary<string, Texture> _cachedTextures = new Dictionary<string, Texture> ();

        #endregion

        #region Constructor

        public SpriteManager () {
            Rand = new Random ();
        }

        #endregion

        /// <summary>
        /// Called from the main draw function on frame start.
        /// </summary>
        public void OnFrameStart (double elapsedTime) {
            Metronom0 = (float)(DateTime.UtcNow.Millisecond % 1000) / 1000;
            Metronom1 = (float)((DateTime.UtcNow.Ticks % _metronom1Measure) / _metronom1Measure);
            Metronom2 = (float)((DateTime.UtcNow.Ticks % _metronom2Measure) / _metronom2Measure);
            Metronom3 = (float)((DateTime.UtcNow.Ticks % _metronom3Measure) / _metronom3Measure);
            Ticks = DateTime.UtcNow.Ticks;
            LastFrameTime = elapsedTime;
        }

        #region Texture loading

        /// <summary>
        /// Load a texture from the given file name.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Texture LoadTexture (string fileName) {
            if (string.IsNullOrEmpty (fileName))
                throw new ArgumentNullException (fileName);

            if (!_cachedTextures.ContainsKey (fileName)) {
                Texture texture;
                using (Stream stream = GameAccess.Resources.SearchResource (fileName.Replace ('/', '.')).OpenRead ()) {
                    texture = new Texture (stream);
                }
                _cachedTextures [fileName] = texture;
            }

            return _cachedTextures [fileName];
        }

        /// <summary>
        /// Loads a texture from the given resource file.
        /// </summary>
        /// <returns>The texture.</returns>
        /// <param name="resource">Resource.</param>
        public Texture LoadTexture (ResourceFile resource) {
            if (resource == null) {
                throw new ArgumentNullException ("resource");
            }

            if (!_cachedTextures.ContainsKey (resource.Name)) {
                Texture texture;
                using (Stream stream = resource.OpenRead ()) {
                    texture = new Texture (stream);
                }
                _cachedTextures [resource.Name] = texture;
            }

            return _cachedTextures [resource.Name];
        }

        /// <summary>
        /// Loads all textures matching the given pattern.
        /// </summary>
        /// <returns>The textures.</returns>
        /// <param name="pattern">Pattern.</param>
        public IList<Texture> LoadTextures (string pattern) {
            if (string.IsNullOrEmpty (pattern)) {
                throw new ArgumentNullException (pattern);
            }

            List<Texture> textures = new List<Texture> ();
            IList<ResourceFile> matching = GameAccess.Resources.Search (GetResourceIdent (pattern)).ToList ();
            foreach (ResourceFile file in matching) {
                using (Stream stream = file.OpenRead ()) {
                    textures.Add (new Texture (stream));
                }
            }

            return textures;
        }

        /// <summary>
        /// Loads a random texture matching the given pattern.
        /// </summary>
        /// <returns>The random texture.</returns>
        /// <param name="pattern">Pattern.</param>
        public Texture LoadRandomTexture (string pattern) {
            if (string.IsNullOrEmpty (pattern)) {
                throw new ArgumentNullException (pattern);
            }

            IList<ResourceFile> matching = GameAccess.Resources.Search (GetResourceIdent (pattern)).ToList ();
            Texture texture;
            using (Stream stream = matching [Rand.Next (matching.Count)].OpenRead ()) {
                texture = new Texture (stream);
            }
            return texture;
        }

        string GetResourceIdent (string path) {
            return (Constants.PATH_RESOURCES + path).Replace ('/', '.');
        }

        #endregion

        public uint GetNextIconIndex () {
            _lastIconIndex++;
            return _lastIconIndex;
        }

        #region IIconRegister

        public bool HasPart (ModelPart part, string key) {
            return _atlas.HasPart (part, key);
        }

        public uint RegisterSingle (string key) {
            return RegisterIcon (key) [0];
        }

        public uint[] RegisterIcon (string key) {
            return RegisterIcon (ModelPart.Sprite, key);
        }

        public uint[] RegisterIcon (ModelPart part, string key) {
            string[] tokens = key.Split (LibraryConstants.NAME_DELIM);
            if (tokens.Length > 1)
                key = tokens [1];

            return _atlas [part, key];
        }

        #endregion

        /// <summary>
        /// Regenerates the sprite sheets and recreates necessary mappings from definition files. Needs to be called whenever a new world is loaded.
        /// </summary>
        internal void RegenerateAtlas (SpriteState state) {

            GameAccess.Interface.GameConsole.Debug ("Regenerating SpriteAtlas for state {0}.", state);

            _lastIconIndex = 0;
            SpriteAtlas atlas = new SpriteAtlas (this);

            foreach (Stitchable stitchable in DetermineTextureSheets()) {
                if (stitchable.States.HasFlag (state)) {
                    atlas.LoadIconDefinitions (stitchable);
                }
            }

            atlas.Harden ();
            _atlas = atlas;

            _missingIndex = RegisterSingle ("missing");
            _dummyIndex = RegisterSingle ("dummy");
        }

        IEnumerable<Stitchable> DetermineTextureSheets () {
            Dictionary<string, Stitchable> stitchables = new Dictionary<string, Stitchable> ();

            IEnumerable<ResourceFile> resources = GameAccess.Resources.Search ("Resources.Textures.Stitchable");
            GameAccess.Interface.GameConsole.Debug ("Found {0} matching resource files.", resources.Count ());
            // Grab all image files.
            foreach (ResourceFile imageres in resources.Where(p => p.Ident.EndsWith(".png"))) {
                GameAccess.Interface.GameConsole.Debug ("Queuing the image '{0}' for stitching.", imageres.Name);
                Stitchable stitchable = stitchables [FileUtils.RemoveFileExtension (imageres.Ident)] = new Stitchable (imageres);

                // Make sure gui stuff is available in navigation state.
                if (imageres.Ident.Contains ("Textures.Stitchable.Gui")) {
                    stitchable.States |= SpriteState.Navigation;
                }
            }
            // Set corresponding image descriptions if available.
            foreach (ResourceFile descres in resources.Where(p => p.Ident.EndsWith(".icons"))) {
                string ident = FileUtils.RemoveFileExtension (descres.Ident);
                if (!stitchables.ContainsKey (ident)) {
                    GameAccess.Interface.GameConsole.Warning ("Ignoring orphaned image description: " + descres.Name);
                    continue;
                }

                GameAccess.Interface.GameConsole.Debug ("Adding the image description '{0}' for stitching.", descres.Name);
                stitchables [ident].Description = descres;

                JsonObject result;
                using (StreamReader reader = new StreamReader (descres.OpenRead ())) {
                    result = JsonParser.JsonDecode (reader.ReadToEnd ()).GetValue<JsonObject> ();
                }

                if (result.ContainsKey ("states")) {
                    JsonArray stateinfo = result ["states"].GetValue<JsonArray> ();
                    stitchables [ident].States = SpriteState.None;
                    foreach (JsonNode node in stateinfo) {
                        stitchables [ident].States |= (SpriteState)Enum.Parse (typeof(SpriteState), node.GetValue<string> (), true);
                    }
                }
            }

            return stitchables.Values;
        }

        #region Disposing

        /// <summary>
        /// Calls dispose on the texture which have been loaded as texture sheets.
        /// </summary>
        public void DisposeTextures () {
            _atlas.Dispose ();
            foreach (Texture texture in _cachedTextures.Values) {
                texture.Dispose ();
            }
        }

        #endregion

        #region Helper functions

        /// <summary>
        /// Compresses an array of model reels into a single SpriteBatch.
        /// </summary>
        /// <returns>SpriteBatch created from the given reels.</returns>
        /// <param name="reels">Reels.</param>
        public static SpriteBatch CompressToBatch (ModelReel[] reels) {
            if (reels.Length <= 0) {
                throw new ArgumentException ("Cannot compress 0 reels.");
            }

            Texture texture = null;
            List<Rect2i> sources = new List<Rect2i> ();
            List<Rect2f> destinations = new List<Rect2f> ();
            List<Colour> colours = new List<Colour> ();

            for (int i = 0; i < reels.Length; i++) {
                for (int j = 0; j < reels [i].Layers.Length; j++) {

                    IconMapping mapping = SpriteManager.Instance.GetMapping (reels [i].Layers [j]);
                    if (texture == null) {
                        texture = mapping.Texture;
                    } else if (texture != mapping.Texture) {
                        throw new SystemException ("Sprites which are compressed into a SpriteBatch need to reside on a common texture sheet!");
                    }

                    Sprite sprite = mapping [0];
                    sources.Add (sprite.SourceRect);
                    destinations.Add (new Rect2f (sprite.Position, sprite.SourceRect.Size));
                    colours.Add (reels [i].Layers [j].Colour);

                }
            }

            return new SpriteBatch (texture, sources.ToArray (), destinations.ToArray (), colours.ToArray ());
        }

        public static SpriteBatch CompressToBatch (Sprite[] sprites, Vect2i[] positions) {
            Texture texture = null;
            List<Rect2i> sources = new List<Rect2i> ();
            List<Rect2f> destinations = new List<Rect2f> ();
            List<Colour> colours = new List<Colour> ();

            for (int i = 0; i < sprites.Length; i++) {

                Sprite sprite = sprites [i];
                if (texture == null) {
                    texture = sprite.Texture;
                } else if (texture != sprite.Texture) {
                    throw new SystemException ("Sprites which are compressed into a SpriteBatch need to reside on a common texture sheet!");
                }

                sources.Add (sprite.SourceRect);
                if (positions != null) {
                    destinations.Add (new Rect2f (sprite.Position + positions [i], sprite.SourceRect.Size));
                } else {
                    destinations.Add (new Rect2f (sprite.Position, sprite.SourceRect.Size));
                }
                colours.Add (sprite.Colour);

            }

            return new SpriteBatch (texture, sources.ToArray (), destinations.ToArray (), colours.ToArray ());
        }

        #endregion
    }
}
