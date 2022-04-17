using Bonsai.Resources;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for material shader programs.
    /// </summary>
    [DisplayName(XmlTypeName)]
    [XmlType(TypeName = XmlTypeName, Namespace = Constants.XmlNamespace)]
    public class MaterialConfiguration : ShaderConfiguration
    {
        const string XmlTypeName = "Material";

        /// <summary>
        /// Gets or sets the path to the vertex shader file.
        /// </summary>
        [Category("Shaders")]
        [Description("Specifies the path to the vertex shader program.")]
        [FileNameFilter("Vertex Shader Files (*.vert)|*.vert|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string VertexShader { get; set; }

        /// <summary>
        /// Gets or sets the path to the geometry shader file.
        /// </summary>
        [Category("Shaders")]
        [Description("Specifies the path to the geometry shader file.")]
        [FileNameFilter("Geometry Shader Files (*.geom)|*.geom|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GeometryShader { get; set; }

        /// <summary>
        /// Gets or sets the path to the fragment shader file.
        /// </summary>
        [Category("Shaders")]
        [Description("Specifies the path to the fragment shader file.")]
        [FileNameFilter("Fragment Shader Files (*.frag)|*.frag|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FragmentShader { get; set; }

        /// <summary>
        /// Gets or sets the name of the mesh geometry to draw.
        /// </summary>
        [Category("State")]
        [Description("The name of the mesh geometry to draw.")]
        [TypeConverter(typeof(MeshNameConverter))]
        public string MeshName { get; set; }

        /// <summary>
        /// Creates a new material shader resource.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Shader"/> class representing the
        /// compiled material shader.
        /// </returns>
        /// <inheritdoc/>
        public override Shader CreateResource(ResourceManager resourceManager)
        {
            var windowManager = resourceManager.Load<WindowManager>(string.Empty);
            var vertexSource = ReadShaderSource(VertexShader);
            var geometrySource = ReadShaderSource(GeometryShader);
            var fragmentSource = ReadShaderSource(FragmentShader);

            var material = new Material(
                Name, windowManager.Window,
                vertexSource,
                geometrySource,
                fragmentSource,
                RenderState,
                ShaderUniforms,
                BufferBindings,
                Framebuffer);
            if (!string.IsNullOrEmpty(MeshName))
            {
                material.Mesh = resourceManager.Load<Mesh>(MeshName);
            }
            windowManager.Window.AddShader(material);
            return material;
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
