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
    internal class ShaderState
    {
        readonly Shader shader;
        readonly List<StateConfiguration> renderState;
        readonly List<UniformBinding> uniformBindings;
        readonly List<BufferBinding> bufferBindings;
        readonly FramebufferState framebuffer;

        public ShaderState(
            Shader shaderTarget,
            IEnumerable<StateConfiguration> renderStateConfiguration,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<BufferBindingConfiguration> bufferBindingConfiguration,
            FramebufferConfiguration framebufferConfiguration)
        {
            if (shaderTarget == null)
            {
                throw new ArgumentNullException("shaderTarget");
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
            uniformBindings = shaderUniforms.Select(configuration => new UniformBinding(configuration)).ToList();
            bufferBindings = bufferBindingConfiguration.Select(configuration => configuration.CreateBufferBinding()).ToList();
            framebuffer = new FramebufferState(framebufferConfiguration);
        }
        
        public void Load()
        {
            foreach (var binding in uniformBindings)
            {
                binding.Load(shader);
            }

            foreach (var binding in bufferBindings)
            {
                binding.Load(shader);
            }

            framebuffer.Load(shader.Window);
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
                binding.Bind(shader);
            }

            foreach (var binding in bufferBindings)
            {
                binding.Bind(shader);
            }

            framebuffer.Bind(shader.Window);
        }

        public void Unbind()
        {
            framebuffer.Unbind(shader.Window);
            foreach (var binding in bufferBindings)
            {
                binding.Unbind(shader);
            }
        }

        public void Unload()
        {
            framebuffer.Unload(shader.Window);
            foreach (var binding in bufferBindings)
            {
                binding.Unload(shader);
            }
        }

        class UniformBinding
        {
            int location;
            UniformConfiguration configuration;

            public UniformBinding(UniformConfiguration uniformConfiguration)
            {
                if (uniformConfiguration == null)
                {
                    throw new ArgumentNullException("uniformConfiguration");
                }

                configuration = uniformConfiguration;
            }

            public void Load(Shader shader)
            {
                location = GL.GetUniformLocation(shader.Program, configuration.Name);
                if (location < 0)
                {
                    throw new InvalidOperationException(string.Format(
                        "The uniform variable \"{0}\" was not found in shader \"{1}\".",
                        configuration.Name,
                        shader.Name));
                }
            }

            public void Bind(Shader shader)
            {
                configuration.SetUniform(location);
            }
        }
    }
}
