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
    internal class MaterialState
    {
        readonly Material material;
        readonly List<StateConfiguration> renderState;
        readonly List<UniformBinding> uniformBindings;
        readonly List<TextureBinding> textureBindings;
        readonly FramebufferState framebuffer;

        public MaterialState(
            Material materialTarget,
            IEnumerable<StateConfiguration> renderStateConfiguration,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<TextureBindingConfiguration> textureBindingConfiguration,
            FramebufferConfiguration framebufferConfiguration)
        {
            if (materialTarget == null)
            {
                throw new ArgumentNullException("materialTarget");
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

            material = materialTarget;
            renderState = renderStateConfiguration.ToList();
            uniformBindings = shaderUniforms.Select(configuration => new UniformBinding(configuration)).ToList();
            textureBindings = textureBindingConfiguration.Select(configuration => new TextureBinding(configuration)).ToList();
            framebuffer = new FramebufferState(framebufferConfiguration);
        }
        
        public void Load()
        {
            foreach (var binding in uniformBindings)
            {
                binding.Load(material);
            }

            foreach (var binding in textureBindings)
            {
                binding.Load(material);
            }

            framebuffer.Load(material.Window);
        }

        public void Bind()
        {
            foreach (var state in renderState)
            {
                state.Execute(material.Window);
            }

            foreach (var binding in uniformBindings)
            {
                binding.Bind(material);
            }

            foreach (var binding in textureBindings)
            {
                binding.Bind(material);
            }

            framebuffer.Bind(material.Window);
        }

        public void Unbind()
        {
            framebuffer.Unbind(material.Window);
            foreach (var binding in textureBindings)
            {
                binding.Unbind(material);
            }
        }

        public void Unload()
        {
            framebuffer.Unload(material.Window);
            foreach (var binding in textureBindings)
            {
                binding.Unload(material);
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

            public void Load(Material material)
            {
                location = GL.GetUniformLocation(material.Program, configuration.Name);
                if (location < 0)
                {
                    throw new InvalidOperationException(string.Format(
                        "The uniform variable \"{0}\" was not found in material \"{1}\".",
                        configuration.Name,
                        material.Name));
                }
            }

            public void Bind(Material material)
            {
                configuration.SetUniform(location);
            }
        }

        class TextureBinding
        {
            Texture texture;
            readonly string name;
            readonly string textureName;
            readonly TextureUnit textureSlot;

            public TextureBinding(TextureBindingConfiguration bindingConfiguration)
            {
                name = bindingConfiguration.Name;
                textureName = bindingConfiguration.TextureName;
                textureSlot = bindingConfiguration.TextureSlot;
            }

            public void Load(Material material)
            {
                material.SetTextureSlot(name, textureSlot);
                if (!material.Window.Textures.TryGetValue(textureName, out texture))
                {
                    throw new InvalidOperationException(string.Format(
                        "The texture reference \"{0}\" was not found.",
                        textureName));
                }
            }

            public void Bind(Material material)
            {
                GL.ActiveTexture(textureSlot);
                GL.BindTexture(TextureTarget.Texture2D, texture.Id);
            }

            public void Unbind(Material material)
            {
                GL.ActiveTexture(textureSlot);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            public void Unload(Material material)
            {
                texture = null;
            }
        }
    }
}
