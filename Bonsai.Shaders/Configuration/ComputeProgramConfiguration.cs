using Bonsai.Resources;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for compute shader programs.
    /// </summary>
    [DisplayName(XmlTypeName)]
    [XmlType(TypeName = XmlTypeName, Namespace = Constants.XmlNamespace)]
    public class ComputeProgramConfiguration : ShaderConfiguration
    {
        const string XmlTypeName = "ComputeProgram";

        /// <summary>
        /// Gets or sets the path to the compute shader file.
        /// </summary>
        [Category("Shaders")]
        [Description("Specifies the path to the compute shader program.")]
        [FileNameFilter("Compute Shader Files (*.comp)|*.comp|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ComputeShader { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the number of workgroups to be
        /// launched when dispatching the compute shader.
        /// </summary>
        [Category("State")]
        [Description("Specifies the number of workgroups to be launched when dispatching the compute shader.")]
        public DispatchParameters WorkGroups { get; set; }

        /// <summary>
        /// Creates a new compute shader program resource.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Shader"/> class representing the
        /// compiled compute shader.
        /// </returns>
        /// <inheritdoc/>
        public override Shader CreateResource(ResourceManager resourceManager)
        {
            var windowManager = resourceManager.Load<WindowManager>(string.Empty);
            var computeSource = ReadShaderSource(ComputeShader);
            var computation = new ComputeProgram(
                Name, windowManager.Window,
                computeSource,
                RenderState,
                ShaderUniforms,
                BufferBindings,
                Framebuffer);
            computation.WorkGroups = WorkGroups;
            windowManager.Window.AddShader(computation);
            return computation;
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
