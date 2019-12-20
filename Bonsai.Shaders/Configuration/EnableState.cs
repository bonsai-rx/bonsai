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
    public class EnableState : StateConfiguration
    {
        public EnableState()
        {
            Capability = EnableCap.Blend;
        }

        [Description("The render state capability to enable.")]
        public EnableCap Capability { get; set; }

        public override void Execute(ShaderWindow window)
        {
            GL.Enable(Capability);
        }

        public override string ToString()
        {
            return string.Format("Enable({0})", Capability);
        }
    }
}
