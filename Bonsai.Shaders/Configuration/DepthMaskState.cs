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
    public class DepthMaskState : StateConfiguration
    {
        public DepthMaskState()
        {
            Enabled = true;
        }

        [Description("Specifies whether the depth buffer is enabled for writing.")]
        public bool Enabled { get; set; }

        public override void Execute(ShaderWindow window)
        {
            GL.DepthMask(Enabled);
        }

        public override string ToString()
        {
            return string.Format("DepthMask({0})", Enabled);
        }
    }
}
