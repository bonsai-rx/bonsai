using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ScissorState : StateConfiguration
    {
        public ScissorState()
        {
            Width = 1;
            Height = 1;
        }

        [Description("The x-coordinate of the lower left corner of the scissor box.")]
        public float X { get; set; }

        [Description("The y-coordinate of the lower left corner of the scissor box.")]
        public float Y { get; set; }

        [Description("The width of the scissor box.")]
        public float Width { get; set; }

        [Description("The height of the scissor box.")]
        public float Height { get; set; }

        public override void Execute(ShaderWindow window)
        {
            window.Scissor = new RectangleF(X, Y, Width, Height);
        }

        public override string ToString()
        {
            return string.Format("Scissor({0}, {1}, {2}, {3})", X, Y, Width, Height);
        }
    }
}
