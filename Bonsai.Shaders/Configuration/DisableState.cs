using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class DisableState : StateConfiguration
    {
        public DisableState()
        {
            Capability = EnableCap.Blend;
        }

        [Description("The render state capability to disable.")]
        public EnableCap Capability { get; set; }

        public override void Execute(ShaderWindow window)
        {
            GL.Disable(Capability);
        }

        public override string ToString()
        {
            return string.Format("Disable({0})", Capability);
        }
    }
}
