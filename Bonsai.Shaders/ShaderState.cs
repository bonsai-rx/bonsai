using Bonsai.Resources;
using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    internal class ShaderState : IDisposable
    {
        readonly Shader shader;
        readonly List<StateConfiguration> renderState;
        readonly List<UniformBinding> uniformBindings;
        readonly List<BufferBinding> bufferBindings;
        readonly FramebufferState framebuffer;

        public ShaderState(
            Shader shaderTarget,
            ResourceManager resourceManager,
            IEnumerable<StateConfiguration> renderStateConfiguration,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<BufferBindingConfiguration> bufferBindingConfiguration,
            FramebufferConfiguration framebufferConfiguration)
        {
            if (shaderTarget == null)
            {
                throw new ArgumentNullException("shaderTarget");
            }

            if (resourceManager == null)
            {
                throw new ArgumentNullException("resourceManager");
            }

            if (renderStateConfiguration == null)
            {
                throw new ArgumentNullException("renderStateConfiguration");
            }

            if (shaderUniforms == null)
            {
                throw new ArgumentNullException("shaderUniforms");
            }

            if (bufferBindingConfiguration == null)
            {
                throw new ArgumentNullException("bufferBindingConfiguration");
            }

            if (framebufferConfiguration == null)
            {
                throw new ArgumentNullException("framebufferConfiguration");
            }

            shader = shaderTarget;
            renderState = renderStateConfiguration.ToList();
            uniformBindings = shaderUniforms.Select(configuration => new UniformBinding(shader, configuration)).ToList();
            bufferBindings = bufferBindingConfiguration.Select(configuration => configuration.CreateBufferBinding(shader, resourceManager)).ToList();
            framebuffer = new FramebufferState(shader.Window, framebufferConfiguration);
        }

        public void Execute()
        {
            foreach (var state in renderState)
            {
                state.Execute(shader.Window);
            }
        }

        public void Bind()
        {
            foreach (var binding in uniformBindings)
            {
                binding.Bind();
            }

            foreach (var binding in bufferBindings)
            {
                binding.Bind();
            }

            framebuffer.Bind();
        }

        public void Unbind()
        {
            framebuffer.Unbind();
            foreach (var binding in bufferBindings)
            {
                binding.Unbind();
            }
        }

        class UniformBinding
        {
            int location;
            UniformConfiguration configuration;

            public UniformBinding(Shader shader, UniformConfiguration uniformConfiguration)
            {
                if (uniformConfiguration == null)
                {
                    throw new ArgumentNullException("uniformConfiguration");
                }

                if (string.IsNullOrEmpty(uniformConfiguration.Name))
                {
                    throw new InvalidOperationException(string.Format(
                        "Missing variable name for uniform assignment in shader \"{0}\".",
                        shader.Name));
                }

                configuration = uniformConfiguration;
                location = GL.GetUniformLocation(shader.Program, configuration.Name);
                if (location < 0)
                {
                    throw new InvalidOperationException(string.Format(
                        "The uniform variable \"{0}\" was not found in shader \"{1}\".",
                        configuration.Name,
                        shader.Name));
                }
            }

            public void Bind()
            {
                configuration.SetUniform(location);
            }
        }

        public void Dispose()
        {
            framebuffer.Dispose();
        }
    }
}
