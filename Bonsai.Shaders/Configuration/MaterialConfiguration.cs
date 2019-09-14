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
    [DisplayName(XmlTypeName)]
    [XmlType(TypeName = XmlTypeName, Namespace = Constants.XmlNamespace)]
    public class MaterialConfiguration : ShaderConfiguration
    {
        const string XmlTypeName = "Material";

        [Category("Shaders")]
        [Description("Specifies the path to the vertex shader.")]
        [FileNameFilter("Vertex Shader Files (*.vert)|*.vert|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string VertexShader { get; set; }

        [Category("Shaders")]
        [Description("Specifies the path to the geometry shader.")]
        [FileNameFilter("Geometry Shader Files (*.geom)|*.geom|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string GeometryShader { get; set; }

        [Category("Shaders")]
        [Description("Specifies the path to the fragment shader.")]
        [FileNameFilter("Fragment Shader Files (*.frag)|*.frag|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FragmentShader { get; set; }

        [Category("State")]
        [Description("The name of the mesh geometry to draw.")]
        [TypeConverter(typeof(MeshNameConverter))]
        public string MeshName { get; set; }

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

        public override string ToString()
        {
            var name = Name;
            if (string.IsNullOrEmpty(name)) return XmlTypeName;
            else return string.Format("{0} [{1}]", name, XmlTypeName);
        }
    }
}
