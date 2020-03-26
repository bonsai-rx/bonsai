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
    public class BlendFunctionState : StateConfiguration
    {
        public BlendFunctionState()
        {
            SourceFactor = BlendingFactor.SrcAlpha;
            DestinationFactor = BlendingFactor.OneMinusSrcAlpha;
        }

        [Description("Specifies the scale factor for blending source color values.")]
        public BlendingFactor SourceFactor { get; set; }

        [Description("Specifies the scale factor for blending destination color values.")]
        public BlendingFactor DestinationFactor { get; set; }

        public override void Execute(ShaderWindow window)
        {
            GL.BlendFunc(SourceFactor, DestinationFactor);
        }

        public override string ToString()
        {
            return string.Format("BlendFunc({0}, {1})", SourceFactor, DestinationFactor);
        }
    }
}
