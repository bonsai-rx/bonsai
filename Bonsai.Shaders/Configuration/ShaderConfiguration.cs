using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlInclude(typeof(MaterialConfiguration))]
    [XmlInclude(typeof(ViewportEffectConfiguration))]
    [XmlInclude(typeof(ComputeProgramConfiguration))]
    public abstract class ShaderConfiguration
    {
        readonly FramebufferConfiguration framebuffer = new FramebufferConfiguration();
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();
        readonly UniformConfigurationCollection shaderUniforms = new UniformConfigurationCollection();
        readonly BufferBindingConfigurationCollection bufferBindings = new BufferBindingConfigurationCollection();

        public ShaderConfiguration()
        {
            Enabled = true;
        }

        [Description("The name of the shader program.")]
        public string Name { get; set; }

        [Category("State")]
        [Description("Specifies whether the shader is active.")]
        public bool Enabled { get; set; }

        [Category("State")]
        [Description("Specifies any render states that are required to run the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [Category("State")]
        [Description("Specifies any shader uniform values that are required to run the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.UniformConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public UniformConfigurationCollection ShaderUniforms
        {
            get { return shaderUniforms; }
        }

        [Category("State")]
        [Description("Specifies any buffer bindings that are required to run the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.BufferBindingConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public BufferBindingConfigurationCollection BufferBindings
        {
            get { return bufferBindings; }
        }

        [Category("State")]
        [Description("Specifies any framebuffer attachments that are required to run the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.FramebufferAttachmentConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebuffer.FramebufferAttachments; }
        }

        protected FramebufferConfiguration Framebuffer
        {
            get { return framebuffer; }
        }

        internal string ReadShaderSource(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (!File.Exists(path))
            {
                throw new ArgumentNullException("path", "The specified path \"" + path + "\" was not found while loading " + Name + " shader.");
            }

            return File.ReadAllText(path);
        }

        public abstract Shader CreateShader(ShaderWindow window);

        public override string ToString()
        {
            var name = Name;
            return string.IsNullOrEmpty(name) ? GetType().Name : name;
        }
    }
}
