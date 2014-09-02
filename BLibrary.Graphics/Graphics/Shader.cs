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

﻿using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using BLibrary.Util;

namespace BLibrary.Graphics {

    public sealed class Shader {
        TextureUnit[] TU_VALUES = (TextureUnit[])Enum.GetValues (typeof(TextureUnit));
        const string U_PROJECTION_MATRIX = "matrixProjection";
        const string U_MODELVIEW_MATRIX = "matrixModelview";

        /// <summary>
        /// Simple struct to save texture bindings in a shader.
        /// </summary>
        struct TextureBinding {
            public int UniformLocation;
            public TextureUnit TextureUnit;
            public int TextureId;

            public TextureBinding (int uniformLocation, TextureUnit unit, int texId) {
                UniformLocation = uniformLocation;
                TextureUnit = unit;
                TextureId = texId;
            }

            public override string ToString () {
                return string.Format ("[TextureBinding: UniformLocation={0}, TextureUnit={1}, TextureId={2}]", UniformLocation, TextureUnit.ToString (), TextureId);
            }
        }

        /// <summary>
        /// Indicates whether the shader is available and useable.
        /// </summary>
        /// <value><c>true</c> if this instance is available; otherwise, <c>false</c>.</value>
        public bool IsAvailable {
            get { return _programId > 0; }
        }

        int _programId = -1;

        public int ProgramId {
            get { return _programId; }
        }

        public bool Debug {
            get;
            set;
        }

        Dictionary<string, int> _uniformLocations = new Dictionary<string, int> ();
        Dictionary<string, int> _attributeLocations = new Dictionary<string, int> ();
        Dictionary<string, int> _textureUnits = new Dictionary<string, int> ();
        TextureBinding[] _textures = new TextureBinding[4];

        public bool RequiresMatrices {
            get;
            private set;
        }

        public Shader (ShaderType type, Stream stream) {
            CreateShader (type, stream);
        }

        public Shader (Stream vertex, Stream fragment) {
            CreateShader (vertex, fragment);
        }

        void CreateShader (ShaderType type, Stream stream) {
            if (type == ShaderType.VertexShader) {
                CreateShader (stream, null);
            } else if (type == ShaderType.FragmentShader) {
                CreateShader (null, stream);
            } else {
                Console.Out.WriteLine ("Ignored a shader of unhandled type: " + type.ToString ());
            }
        }

        void CreateShader (Stream vertex, Stream fragment) {
            // Create the program.
            CreateProgram ();

            // Load the shaders
            if (vertex != null) {
                LoadShader (ShaderType.VertexShader, vertex);
                RequiresMatrices = true;
            }
            if (fragment != null) {
                LoadShader (ShaderType.FragmentShader, fragment);
            }

            // If we failed, abort.
            if (_programId <= 0)
                return;
            // Link the program.
            Link ();
            // Flush
            GL.Flush ();
        }

        void LoadShader (ShaderType type, Stream stream) {
            // Load shader source from stream.
            string source = string.Empty;
            using (StreamReader reader = new StreamReader (stream)) {
                source = reader.ReadToEnd ();
            }
            if (string.IsNullOrWhiteSpace (source)) {
                Delete ();
                return;
            }

            // Compile the shader.
            CompileAndAttach (source, type);
        }

        void CompileAndAttach (string source, ShaderType type) {
            int shaderId = GL.CreateShader (type);

            GL.ShaderSource (shaderId, source);
            GL.CompileShader (shaderId);

            int compileResult;
            GL.GetShader (shaderId, ShaderParameter.CompileStatus, out compileResult);
            if (compileResult != 1) {
                Console.Out.WriteLine ("Shader failed to compile and will be unuseable: " + GL.GetShaderInfoLog (shaderId));
                Console.Out.WriteLine (source);
                Delete ();
                return;
            }

            // Attach the shader to the program.
            GL.AttachShader (_programId, shaderId);

            // Delete the shader again, it is not needed anymore.
            GL.DeleteShader (shaderId);
        }

        void CreateProgram () {
            if (_programId > 0)
                Delete ();

            _programId = GL.CreateProgram ();
        }

        void Link () {
            GL.LinkProgram (_programId);

            int linkResult;
            GL.GetProgram (_programId, GetProgramParameterName.LinkStatus, out linkResult);
            if (linkResult != 1) {
                Console.Out.WriteLine ("Shader program failed to link and will be unuseable: " + GL.GetProgramInfoLog (_programId));
                Delete ();
                return;
            }
        }

        void Delete () {
            if (_programId > 0) {
                GL.DeleteProgram (_programId);
                _programId = -1;
            }
        }

        public void BindTextures () {
            for (int i = 0; i < _textures.Length; i++) {
                TextureBinding binding = _textures [i];
                if (binding.TextureId > 0)
                    BindTexture (binding.TextureId, binding.TextureUnit, binding.UniformLocation);
            }
        }

        void BindTexture (int textureId, TextureUnit textureUnit, int uniformLocation) {
            GL.ActiveTexture (textureUnit);
            GL.BindTexture (TextureTarget.Texture2D, textureId);
            GL.Uniform1 (uniformLocation, textureUnit - TextureUnit.Texture0);
        }

        #region Uniforms

        #region Generic

        public void SetUniform (string name, float val) {
            if (!IsAvailable)
                return;
            GL.UseProgram (_programId);

            int location = GetUniformLocation (name);
            if (location >= 0)
                GL.Uniform1 (location, val);
            GL.UseProgram (0);
        }

        public void SetUniform (string name, float val1, float val2, float val3, float val4) {
            if (!IsAvailable)
                return;
            GL.UseProgram (_programId);

            int location = GetUniformLocation (name);
            if (location >= 0)
                GL.Uniform4 (location, val1, val2, val3, val4);
            GL.UseProgram (0);
        }

        public void SetUniform (string name, int val) {
            if (!IsAvailable)
                return;
            GL.UseProgram (_programId);

            int location = GetUniformLocation (name);
            if (location >= 0)
                GL.Uniform1 (location, val);
            GL.UseProgram (0);
        }

        public void SetUniform (string name, Colour colour) {
            SetUniform (name, (float)colour.R / 255, (float)colour.G / 255, (float)colour.B / 255, (float)colour.A / 255);
        }

        public void SetUniform (string name, Vect2f val) {
            SetUniform (name, val.X, val.Y);
        }

        public void SetUniform (string name, float val1, float val2) {
            if (!IsAvailable)
                return;
            GL.UseProgram (_programId);

            int location = GetUniformLocation (name);
            if (location >= 0)
                GL.Uniform2 (location, val1, val2);
            GL.UseProgram (0);
        }

        public void SetUniform (string name, Transform transform) {
            if (!IsAvailable)
                return;
            GL.UseProgram (_programId);

            int location = GetUniformLocation (name);
            if (location >= 0)
                GL.UniformMatrix4 (location, 1, false, transform.Matrix);
            GL.UseProgram (0);
        }

        public void SetUniform (string name, Texture texture) {
            if (!IsAvailable)
                return;
            if (!_textureUnits.ContainsKey (name))
                _textureUnits [name] = _textureUnits.Count;

            _textures [_textureUnits [name]] = new TextureBinding (GetUniformLocation (name), TU_VALUES [_textureUnits [name] + 1], texture.GlTexId);
        }

        #endregion

        public void SetProjectionMatrix (Transform transform) {
            SetUniform (U_PROJECTION_MATRIX, transform);
        }

        public void SetModelviewMatrix (Transform transform) {
            SetUniform (U_MODELVIEW_MATRIX, transform);
        }

        /// <summary>
        /// Returns the location of the uniform variable in this shader.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetUniformLocation (string name) {
            if (!_uniformLocations.ContainsKey (name)) {
                _uniformLocations [name] = GL.GetUniformLocation (_programId, name);
            }
            return _uniformLocations [name];
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Returns the location of an attribute variable in this shader.
        /// </summary>
        /// <returns>The attribute location.</returns>
        /// <param name="name">Name.</param>
        public int GetAttributeLocation (string name) {
            if (!_attributeLocations.ContainsKey (name))
                _attributeLocations [name] = GL.GetAttribLocation (_programId, name);
            return _attributeLocations [name];
        }

        #endregion

        #region Helper functions

        /// <summary>
        /// Binds the given shader.
        /// </summary>
        /// <param name="shader"></param>
        public static void Bind (Shader shader, int texture0Id, Transform projection, Transform modelview) {
            if (shader == null || !shader.IsAvailable) {
                GL.UseProgram (0);
                return;
            }

            GL.UseProgram (shader.ProgramId);

            shader.BindTextures ();
            if (shader.RequiresMatrices) {
                shader.SetProjectionMatrix (projection);
                shader.SetModelviewMatrix (modelview);
            }
                
            // Bind the current texture as Texture0
            GL.ActiveTexture (TextureUnit.Texture0);
            GL.BindTexture (TextureTarget.Texture2D, texture0Id);
            GL.Uniform1 (shader.GetUniformLocation ("texture0"), TextureUnit.Texture0 - TextureUnit.Texture0);
        }

        #endregion
    }
}
