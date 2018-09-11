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
    public class MemoryBarrierState : StateConfiguration
    {
        public MemoryBarrierState()
        {
            Barriers = MemoryBarrierFlags.AllBarrierBits;
        }

        [Description("Specifies the memory barriers to insert.")]
        public MemoryBarrierFlags Barriers { get; set; }

        public override void Execute(ShaderWindow window)
        {
            GL.MemoryBarrier(Barriers);
        }

        public override string ToString()
        {
            return string.Format("MemoryBarrier({0})", Barriers);
        }
    }
}
