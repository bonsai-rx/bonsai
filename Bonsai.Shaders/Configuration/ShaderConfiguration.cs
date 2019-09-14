using Bonsai.Resources;
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
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class ShaderConfiguration : ResourceConfiguration<Shader>
    {
        readonly FramebufferConfiguration framebuffer = new FramebufferConfiguration();
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();
        readonly UniformConfigurationCollection shaderUniforms = new UniformConfigurationCollection();
        readonly BufferBindingConfigurationCollection bufferBindings = new BufferBindingConfigurationCollection();

        [Category("State")]
        [Description("Specifies any render states that are required to run the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [Category("State")]
        [Description("Specifies any shader uniform values that are required to run the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.UniformConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public UniformConfigurationCollection ShaderUniforms
        {
            get { return shaderUniforms; }
        }

        [Category("State")]
        [Description("Specifies any buffer bindings that are required to run the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.BufferBindingConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public BufferBindingConfigurationCollection BufferBindings
        {
            get { return bufferBindings; }
        }

        [Category("State")]
        [Description("Specifies any framebuffer attachments that are required to run the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.FramebufferAttachmentConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
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
            using (var stream = OpenResource(path))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
