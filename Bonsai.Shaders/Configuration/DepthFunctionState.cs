using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class DepthFunctionState : StateConfiguration
    {
        public DepthFunctionState()
        {
            Function = DepthFunction.Less;
        }

        [Description("Specifies the function used for depth buffer comparisons.")]
        public DepthFunction Function { get; set; }

        public override void Execute(ShaderWindow window)
        {
            GL.DepthFunc(Function);
        }

        public override string ToString()
        {
            return string.Format("DepthFunc({0})", Function);
        }
    }
}
