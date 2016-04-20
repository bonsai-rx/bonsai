using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class DisableState : StateConfiguration
    {
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
