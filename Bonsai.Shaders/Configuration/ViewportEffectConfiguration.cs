using Bonsai.Resources;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for viewport effect
    /// shader programs.
    /// </summary>
    [DisplayName(XmlTypeName)]
    [XmlType(TypeName = XmlTypeName, Namespace = Constants.XmlNamespace)]
    public class ViewportEffectConfiguration : ShaderConfiguration
    {
        const string XmlTypeName = "ViewportEffect";
        readonly TexturedQuad texturedQuad = new TexturedQuad();

        /// <summary>
        /// Gets or sets the path to the fragment shader file.
        /// </summary>
        [Category("Shaders")]
        [Description("Specifies the path to the fragment shader file.")]
        [FileNameFilter("Fragment Shader Files (*.frag)|*.frag|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FragmentShader { get; set; }

        /// <summary>
        /// Gets or sets a value specifying quad geometry transformation effects.
        /// </summary>
        [Category("State")]
        [Description("Specifies quad geometry transformation effects.")]
        public QuadEffects QuadEffects
        {
            get { return texturedQuad.QuadEffects; }
            set { texturedQuad.QuadEffects = value; }
        }

        /// <summary>
        /// Creates a new viewport effect shader resource.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Shader"/> class representing the
        /// compiled viewport effect shader.
        /// </returns>
        /// <inheritdoc/>
        public override Shader CreateResource(ResourceManager resourceManager)
        {
            var windowManager = resourceManager.Load<WindowManager>(string.Empty);
            var fragmentSource = ReadShaderSource(FragmentShader);
            var effect = new ViewportEffect(
                Name, windowManager.Window,
                fragmentSource,
                RenderState,
                ShaderUniforms,
                BufferBindings,
                Framebuffer,
                texturedQuad.CreateResource(resourceManager));
            windowManager.Window.AddShader(effect);
            return effect;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var name = Name;
            if (string.IsNullOrEmpty(name)) return XmlTypeName;
            else return $"{name} [{XmlTypeName}]";
        }
    }
}
