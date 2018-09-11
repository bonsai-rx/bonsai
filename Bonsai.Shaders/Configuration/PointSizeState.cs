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
    public class PointSizeState : StateConfiguration
    {
        [Description("Specifies the diameter of rasterized points.")]
        public float Size { get; set; }

        public override void Execute(ShaderWindow window)
        {
            GL.PointSize(Size);
        }

        public override string ToString()
        {
            return string.Format("PointSize({0})", Size);
        }
    }
}
