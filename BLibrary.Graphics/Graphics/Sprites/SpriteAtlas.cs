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
using BLibrary.Json;
using BLibrary.Util;
using System.Drawing;
using Starliners;

namespace BLibrary.Graphics.Sprites {

    sealed class SpriteAtlas : IDisposable {
        #region Accessors

        public IconMapping this [uint iconId] {
            get {
                return _mappings.ContainsKey (iconId) ? _mappings [iconId] : null;
            }
        }

        public uint[] this [ModelPart part, string ident] {
            get {
                return _indices.ContainsKey (part) && _indices [part].ContainsKey (ident) ? _indices [part] [ident] : new uint[] { 0 };
            }
        }

        #endregion

        #region Properties

        public Dictionary<uint, IconMapping> Mappings { get { return _mappings; } }

        #endregion

        #region Members

        readonly SpriteManager _manager;
        readonly AtlasStitcher _stitcher;
        readonly Dictionary<ModelPart, Dictionary<string, uint[]>> _indices = new Dictionary<ModelPart, Dictionary<string, uint[]>> ();
        readonly Dictionary<uint, IconMapping> _mappings = new Dictionary<uint, IconMapping> ();
        bool _hardened = false;

        #endregion

        #region Constructor

        public SpriteAtlas (SpriteManager manager) {
            _manager = manager;
            _stitcher = new AtlasStitcher ();
        }

        #endregion

        public bool HasPart (ModelPart part, string ident) {
            return _indices.ContainsKey (part) && _indices [part].ContainsKey (ident);
        }

        /// <summary>
        /// Prevents further definitions being added to this atlas, but enables it for use.
        /// </summary>
        public void Harden () {
            _stitcher.Flush ();
            _stitcher.UpdateTexture ();
            _hardened = true;
        }

        /// <summary>
        /// Loads icons from the given stitchable, stitches them to the atlas texture and creates the necessary mappings.
        /// </summary>
        /// <param name="stitchable"></param>
        public void LoadIconDefinitions (Stitchable stitchable) {

            if (_hardened) {
                throw new SystemException ("Attempted to load additional icon definitions into an already hardened SpriteAtlas.");
            }

            GameAccess.Interface.GameConsole.Debug ("Loading {0} and stitching it into the SpriteAtlas.", stitchable.Image.Ident);
            Texture sheet = _manager.LoadTexture (stitchable.Image);
            Bitmap bitmap = sheet.ToBitmap ();

            if (stitchable.Description == null) {

                uint[] processing = ValidateIndexArray (ModelPart.Sprite, CreateKeyForStitchable (stitchable), 1);

                // If an image description is not available, we assume a big single texture.
                IconMapping mapping = new SimpleMapping (sheet, new Vect2f (), new Rect2i (0, 0, sheet.Size.X, sheet.Size.Y));
                processing [0] = _manager.GetNextIconIndex ();
                StitchAndStow (mapping, sheet, bitmap, processing [0]);

            } else {

                GameAccess.Interface.GameConsole.Debug ("Using icon definitions from {0} for stitching.", stitchable.Description.Ident);

                // Read the provided image description.
                JsonObject result = null;
                using (StreamReader reader = new StreamReader (stitchable.Description.OpenRead ())) {
                    result = JsonParser.JsonDecode (reader.ReadToEnd ()).GetValue<JsonObject> ();
                }

                int pixels = result.ContainsKey ("scale") ? (int)result ["scale"].GetValue<double> () : 1;
                if (sheet.Size.X % pixels != 0
                    || sheet.Size.Y % pixels != 0) {
                    throw new SystemException (string.Format ("Could not reconcile texture size {0} and pixel scale {1} for the sheet {2}!", sheet.Size, pixels, stitchable.Description.Ident));
                }

                JsonArray icons = result ["icons"].GetValue<JsonArray> ();
                foreach (JsonNode node in icons) {
                    JsonObject iconDef = node.GetValue<JsonObject> ();

                    foreach (ModelPart part in Enum.GetValues(typeof(ModelPart))) {

                        string partident = part.ToString ().ToLowerInvariant ();
                        if (!iconDef.ContainsKey (partident)) {
                            continue;
                        }

                        //Array definitions = ((ArrayList)iconDef [partident]).ToArray ();
                        JsonArray definitions = iconDef [partident].GetValue<JsonArray> ();
                        string key = iconDef.ContainsKey ("key") ? iconDef ["key"].GetValue<string> () : icons.Count == 1 ? CreateKeyForStitchable (stitchable) : string.Empty;
                        if (string.IsNullOrWhiteSpace (key)) {
                            throw new SystemException ("No key set for an icon definition file: " + stitchable.Description.Ident);
                        }

                        uint[] processing = ValidateIndexArray (part, key, definitions.Count);

                        for (int i = 0; i < definitions.Count; i++) {

                            processing [i] = _manager.GetNextIconIndex ();
                            IconMapping mapping;
                            if (definitions [i].Type == JsonValueType.Object) {
                                mapping = ParseComplexMapping (sheet, pixels, definitions [i].GetValue<JsonObject> ());
                            } else if (definitions [i].Type == JsonValueType.Array) {
                                mapping = ParseExplicitMapping (sheet, definitions [i].GetValue<JsonArray> ());
                            } else {
                                mapping = ParseSimpleMapping (sheet, pixels, (int)definitions [i].GetValue<double> ());
                            }

                            StitchAndStow (mapping, sheet, bitmap, processing [i]);
                        }
                    }

                }
            }
        }

        string CreateKeyForStitchable (Stitchable stitchable) {
            string key = FileUtils.RemoveFileExtension (stitchable.Image.Ident);

            int lastIndex = key.LastIndexOf ('.');
            return StringUtils.Uncapitalize (key.Substring (lastIndex + 1, key.Length - lastIndex - 1));
        }

        uint[] ValidateIndexArray (ModelPart part, string key, int length) {
            // Create the sub dictionary if needed.
            if (!_indices.ContainsKey (part)) {
                _indices [part] = new Dictionary<string, uint[]> ();
            }
            Dictionary<string, uint[]> indices = _indices [part];

            if (indices.ContainsKey (key)) {
                throw new SystemException ("An icon definition already exists for the key: " + key);
            }
            indices [key] = new uint[length];
            return indices [key];
        }

        void StitchAndStow (IconMapping mapping, Texture sheet, Bitmap bitmap, uint index) {
            _stitcher.StitchMapping (sheet, bitmap, mapping);
            _mappings [index] = mapping;
        }

        IconMapping ParseSimpleMapping (Texture sheet, int pixels, int index) {
            return CreateFlatMapping (index, sheet, pixels, new Vect2f (), new Vect2f (), new Vect2f (1, 1));
        }

        IconMapping ParseExplicitMapping (Texture sheet, JsonArray list) {
            if (list.Count != 4) {
                throw new ArgumentException ("Attempted to parse an explicit icon mapping with an argument length other than 4.");
            }

            return new SimpleMapping (sheet, new Vect2f (), new Rect2i ((int)list [0].GetValue<double> (), (int)list [1].GetValue<double> (), (int)list [2].GetValue<double> (), (int)list [3].GetValue<double> ()));
        }

        IconMapping ParseComplexMapping (Texture sheet, int pixels, JsonObject table) {

            // Sprite offsets
            float offsetX = 0;
            float offsetY = 0;
            if (table.ContainsKey ("offsets")) {
                JsonArray offsets = table ["offsets"].GetValue<JsonArray> ();
                offsetX = (float)offsets [0].GetValue<double> ();
                offsetY = (float)offsets [1].GetValue<double> ();
            }

            float translateX = 0;
            float translateY = 0;
            if (table.ContainsKey ("translate")) {
                JsonArray translates = table ["translate"].GetValue<JsonArray> ();
                translateX = (float)translates [0].GetValue<double> ();
                translateY = (float)translates [1].GetValue<double> ();
            }

            // Sprite dimensions
            float xDimension = 1;
            float yDimension = 1;
            if (table.ContainsKey ("dimensions")) {
                JsonArray dimensions = table ["dimensions"].GetValue<JsonArray> ();
                xDimension = (float)dimensions [0].GetValue<double> ();
                yDimension = (float)dimensions [1].GetValue<double> ();
            }

            JsonArray iconIndices = table ["indices"].GetValue<JsonArray> ();
            if (iconIndices.Count > 1) {
                return CreateAnimationMapping (iconIndices, table.ContainsKey ("animation") ? table ["animation"].GetValue<JsonArray> () : null, sheet, pixels, new Vect2f (translateX, translateY), new Vect2f (offsetX, offsetY), new Vect2f (xDimension, yDimension));
            } else if (iconIndices.Count == 1) {
                return CreateFlatMapping ((int)iconIndices [0].GetValue<double> (), sheet, pixels, new Vect2f (translateX, translateY), new Vect2f (offsetX, offsetY), new Vect2f (xDimension, yDimension));
            } else {
                throw new SystemException ("Invalid icon definition!");
            }
        }

        IconMapping CreateFlatMapping (int index, Texture sheet, int pixels, Vect2f translate, Vect2f offset, Vect2f dimensions) {
            return new SimpleMapping (sheet, translate * new Vect2i (SpriteManager.TILE_DIMENSION, SpriteManager.TILE_DIMENSION), CreateTextureRect (index, sheet.Size, pixels, offset, dimensions));
        }

        IconMapping CreateAnimationMapping (JsonArray iconIndices, JsonArray animation, Texture sheet, int pixels, Vect2f translate, Vect2f offset, Vect2f dimensions) {
            Rect2i[] indices = new Rect2i[iconIndices.Count];
            for (int i = 0; i < iconIndices.Count; i++) {
                int index = (int)iconIndices [i].GetValue<double> ();
                indices [i] = CreateTextureRect (index, sheet.Size, pixels, offset, dimensions);
            }

            AnimationMapping mapping = new AnimationMapping (sheet, translate * new Vect2i (SpriteManager.TILE_DIMENSION, SpriteManager.TILE_DIMENSION), indices);

            if (animation != null) {
                int[][] anim = new int[animation.Count][];
                for (int i = 0; i < animation.Count; i++) {
                    JsonArray item = animation [i].GetValue<JsonArray> ();

                    anim [i] = new int[2];
                    anim [i] [0] = (int)item [0].GetValue<double> ();
                    anim [i] [1] = (int)item [1].GetValue<double> ();
                }
                mapping.SetAnimation (anim);
            }

            return mapping;
        }

        Rect2i CreateTextureRect (int iconIndex, Vect2i sheetSize, int pixels, Vect2f offset, Vect2f dimensions) {
            int sheetStepsX = sheetSize.X / pixels;

            int column = iconIndex % sheetStepsX;
            int row = (iconIndex - column) / sheetStepsX;

            return new Rect2i ((column * pixels) + (pixels * offset.X), (row * pixels) + (pixels * offset.Y), pixels * dimensions.X, pixels * dimensions.Y);
        }

        #region IDisposable

        public void Dispose () {
            _stitcher.Dispose ();
        }

        #endregion
    }
}
