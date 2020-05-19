using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class HintState : StateConfiguration
    {
        public HintState()
        {
            Target = HintTarget.PointSmoothHint;
            Mode = HintMode.Nicest;
        }

        [Description("Specifies the implementation specific behavior to be controlled.")]
        public HintTarget Target { get; set; }

        [Description("Specifies the desired behavior.")]
        public HintMode Mode { get; set; }

        public override void Execute(ShaderWindow window)
        {
            GL.Hint(Target, Mode);
        }

        public override string ToString()
        {
            return string.Format("Hint({0}, {1})", Target, Mode);
        }
    }
}
