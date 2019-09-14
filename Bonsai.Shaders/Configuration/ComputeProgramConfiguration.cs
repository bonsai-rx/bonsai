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
    public class ComputeProgramConfiguration : ShaderConfiguration
    {
        const string XmlTypeName = "ComputeProgram";

        [Category("Shaders")]
        [Description("Specifies the path to the compute shader.")]
        [FileNameFilter("Compute Shader Files (*.comp)|*.comp|All Files (*.*)|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ComputeShader { get; set; }

        [Category("State")]
        [Description("Specifies the number of workgroups to be launched when dispatching the compute shader.")]
        public DispatchParameters WorkGroups { get; set; }

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

        public override string ToString()
        {
            var name = Name;
            if (string.IsNullOrEmpty(name)) return XmlTypeName;
            else return string.Format("{0} [{1}]", name, XmlTypeName);
        }
    }
}
