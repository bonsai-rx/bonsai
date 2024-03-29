﻿using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Provides functionality for executing and updating the state of a compute
    /// shader program.
    /// </summary>
    public class ComputeProgram : Shader
    {
        readonly string computeSource;

        internal ComputeProgram(
            string name,
            ShaderWindow window,
            string computeShader,
            IEnumerable<StateConfiguration> renderState,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<BufferBindingConfiguration> bufferBindings,
            FramebufferConfiguration framebuffer)
            : base(name, window)
        {
            computeSource = computeShader ?? throw new ArgumentNullException(
                nameof(computeShader),
                "No compute shader was specified for compute program " + name + ".");
            CreateShaderState(renderState, shaderUniforms, bufferBindings, framebuffer);
        }

        /// <summary>
        /// Gets or sets a value specifying the number of workgroups to be
        /// launched when dispatching the compute shader.
        /// </summary>
        public DispatchParameters WorkGroups { get; set; }

        /// <summary>
        /// Compiles the compute shader and returns the program object handle.
        /// </summary>
        /// <returns>
        /// A handle to the compute shader program object.
        /// </returns>
        protected override int CreateShader()
        {
            var computeShader = GL.CreateShader(ShaderType.ComputeShader);
            try
            {
                GL.ShaderSource(computeShader, computeSource);
                GL.CompileShader(computeShader);
                GL.GetShader(computeShader, ShaderParameter.CompileStatus, out int status);
                if (status == 0)
                {
                    var message = string.Format(
                        "Failed to compile compute shader.\nShader name: {0}\n{1}",
                        Name,
                        GL.GetShaderInfoLog(computeShader));
                    throw new ShaderException(message);
                }

                var shaderProgram = GL.CreateProgram();
                GL.AttachShader(shaderProgram, computeShader);
                GL.LinkProgram(shaderProgram);
                GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out status);
                if (status == 0)
                {
                    var message = string.Format(
                        "Failed to link shader program.\nShader name: {0}\n{1}",
                        Name,
                        GL.GetProgramInfoLog(shaderProgram));
                    throw new ShaderException(message);
                }

                return shaderProgram;
            }
            finally { GL.DeleteShader(computeShader); }
        }

        /// <inheritdoc/>
        protected override Action OnDispatch()
        {
            var workGroups = WorkGroups;
            if (workGroups.NumGroupsX == 0 || workGroups.NumGroupsY == 0 || workGroups.NumGroupsZ == 0)
            {
                return null;
            }

            return () => GL.DispatchCompute(workGroups.NumGroupsX, workGroups.NumGroupsY, workGroups.NumGroupsZ);
        }
    }
}
