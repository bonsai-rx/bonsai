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
        readonly List<TextureBinding> textureBindings;
        readonly FramebufferState framebuffer;

        public ShaderState(
            Shader shaderTarget,
            IEnumerable<StateConfiguration> renderStateConfiguration,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<TextureBindingConfiguration> textureBindingConfiguration,
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

            if (textureBindingConfiguration == null)
            {
                throw new ArgumentNullException("textureBindingConfiguration");
            }

            if (framebufferConfiguration == null)
            {
                throw new ArgumentNullException("framebufferConfiguration");
            }

            shader = shaderTarget;
            renderState = renderStateConfiguration.ToList();
            uniformBindings = shaderUniforms.Select(configuration => new UniformBinding(configuration)).ToList();
            textureBindings = textureBindingConfiguration.Select(configuration => new TextureBinding(configuration)).ToList();
            framebuffer = new FramebufferState(framebufferConfiguration);
        }
        
        public void Load()
        {
            foreach (var binding in uniformBindings)
            {
                binding.Load(shader);
            }

            foreach (var binding in textureBindings)
            {
                binding.Load(shader);
            }

            framebuffer.Load(shader.Window);
        }

        public void Bind()
        {
            foreach (var state in renderState)
            {
                state.Execute(shader.Window);
            }

            foreach (var binding in uniformBindings)
            {
                binding.Bind(shader);
            }

            foreach (var binding in textureBindings)
            {
                binding.Bind(shader);
            }

            framebuffer.Bind(shader.Window);
        }

        public void Unbind()
        {
            framebuffer.Unbind(shader.Window);
            foreach (var binding in textureBindings)
            {
                binding.Unbind(shader);
            }
        }

        public void Unload()
        {
            framebuffer.Unload(shader.Window);
            foreach (var binding in textureBindings)
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

        class TextureBinding
        {
            Texture texture;
            readonly TextureBindingConfiguration binding;

            public TextureBinding(TextureBindingConfiguration bindingConfiguration)
            {
                binding = bindingConfiguration;
            }

            public void Load(Shader shader)
            {
                shader.SetTextureSlot(binding.Name, binding.TextureSlot);
                if (!shader.Window.Textures.TryGetValue(binding.TextureName, out texture))
                {
                    throw new InvalidOperationException(string.Format(
                        "The texture reference \"{0}\" was not found.",
                        binding.TextureName));
                }
            }

            public void Bind(Shader shader)
            {
                binding.Bind(texture);
            }

            public void Unbind(Shader shader)
            {
                binding.Unbind(texture);
            }

            public void Unload(Shader shader)
            {
                texture = null;
            }
        }
    }
}
