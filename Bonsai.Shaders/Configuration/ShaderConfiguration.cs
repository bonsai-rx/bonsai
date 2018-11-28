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
            const char AssemblySeparator = ':';
            if (string.IsNullOrEmpty(path)) return null;

            var separatorIndex = path.IndexOf(AssemblySeparator);
            if (separatorIndex >= 0)
            {
                var nameElements = path.Split(AssemblySeparator);
                var assembly = System.Reflection.Assembly.Load(nameElements[0]);
                var resourceName = string.Join(ExpressionHelper.MemberSeparator, nameElements);
                var resourceStream = assembly.GetManifestResourceStream(resourceName);
                using (var reader = new StreamReader(resourceStream))
                {
                    return reader.ReadToEnd();
                }
            }
            else
            {
                if (!File.Exists(path))
                {
                    throw new ArgumentNullException("path", "The specified path \"" + path + "\" was not found while loading " + Name + " shader.");
                }

                return File.ReadAllText(path);
            }
        }

        public override string ToString()
        {
            var name = Name;
            return string.IsNullOrEmpty(name) ? GetType().Name : name;
        }
    }
}
