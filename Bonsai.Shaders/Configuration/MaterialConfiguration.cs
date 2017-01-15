﻿using System;
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
    [XmlType(TypeName = XmlTypeName)]
    public class MaterialConfiguration : ShaderConfiguration
    {
        const string XmlTypeName = "Material";

        [Category("Shaders")]
        [Description("Specifies the path to the vertex shader.")]
        [FileNameFilter("Vertex Shader Files (*.vert)|*.vert|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string VertexShader { get; set; }

        [Category("Shaders")]
        [Description("Specifies the path to the geometry shader.")]
        [FileNameFilter("Geometry Shader Files (*.geom)|*.geom|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string GeometryShader { get; set; }

        [Category("Shaders")]
        [Description("Specifies the path to the fragment shader.")]
        [FileNameFilter("Fragment Shader Files (*.frag)|*.frag|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FragmentShader { get; set; }

        [Category("State")]
        [Description("The name of the mesh geometry to draw.")]
        [TypeConverter(typeof(MeshNameConverter))]
        public string MeshName { get; set; }

        public override Shader CreateShader(ShaderWindow window)
        {
            var vertexSource = File.ReadAllText(VertexShader);
            var geometrySource = !string.IsNullOrEmpty(GeometryShader) ? File.ReadAllText(GeometryShader) : null;
            var fragmentSource = File.ReadAllText(FragmentShader);

            var material = new Material(
                Name, window,
                vertexSource,
                geometrySource,
                fragmentSource,
                RenderState,
                ShaderUniforms,
                BufferBindings,
                Framebuffer);
            material.Enabled = Enabled;
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
