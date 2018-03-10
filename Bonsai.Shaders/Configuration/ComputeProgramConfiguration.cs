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
    [XmlType(TypeName = XmlTypeName)]
    public class ComputeProgramConfiguration : ShaderConfiguration
    {
        const string XmlTypeName = "ComputeProgram";

        [Category("Shaders")]
        [Description("Specifies the path to the compute shader.")]
        [FileNameFilter("Compute Shader Files (*.comp)|*.comp|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string ComputeShader { get; set; }

        [Category("State")]
        [Description("Specifies the number of workgroups to be launched when dispatching the compute shader.")]
        public DispatchParameters WorkGroups { get; set; }

        public override Shader CreateShader(ShaderWindow window)
        {
            var computeSource = ReadShaderSource(ComputeShader);
            var computation = new ComputeProgram(
                Name, window,
                computeSource,
                RenderState,
                ShaderUniforms,
                BufferBindings,
                Framebuffer);
            computation.Enabled = Enabled;
            computation.WorkGroups = WorkGroups;
            return computation;
        }

        public override string ToString()
        {
            var name = Name;
            if (string.IsNullOrEmpty(name)) return XmlTypeName;
            else return string.Format("{0} [{1}]", name, XmlTypeName);
        }
    }
}
