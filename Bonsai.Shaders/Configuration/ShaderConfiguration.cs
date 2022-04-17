using Bonsai.Resources;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides the abstract base class for configuring and loading shader resources.
    /// </summary>
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

        /// <summary>
        /// Gets the collection of configuration objects specifying the render states
        /// which are required to run the shader.
        /// </summary>
        [Category("State")]
        [Description("Specifies the render states which are required to run the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        /// <summary>
        /// Gets the collection of configuration objects specifying the default values
        /// of uniform variables in the shader program.
        /// </summary>
        [Category("State")]
        [Description("Specifies the default values of uniform variables in the shader program.")]
        [Editor("Bonsai.Shaders.Configuration.Design.UniformConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public UniformConfigurationCollection ShaderUniforms
        {
            get { return shaderUniforms; }
        }

        /// <summary>
        /// Gets the collection of configuration objects specifying the buffer bindings
        /// to set before running the shader.
        /// </summary>
        [Category("State")]
        [Description("Specifies the buffer bindings to set before running the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.BufferBindingConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public BufferBindingConfigurationCollection BufferBindings
        {
            get { return bufferBindings; }
        }

        /// <summary>
        /// Gets the collection of configuration objects specifying any framebuffer
        /// attachments to use when running the shader.
        /// </summary>
        [Category("State")]
        [Description("Specifies any framebuffer attachments to use when running the shader.")]
        [Editor("Bonsai.Shaders.Configuration.Design.FramebufferAttachmentConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebuffer.FramebufferAttachments; }
        }

        /// <summary>
        /// Gets the configuration state of the framebuffer object used for render
        /// to texture passes.
        /// </summary>
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
