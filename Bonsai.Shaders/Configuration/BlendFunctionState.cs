using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class BlendFunctionState : StateConfiguration
    {
        public BlendFunctionState()
        {
            SourceFactor = BlendingFactorSrc.SrcAlpha;
            DestinationFactor = BlendingFactorDest.OneMinusSrcAlpha;
        }

        public BlendingFactorSrc SourceFactor { get; set; }

        public BlendingFactorDest DestinationFactor { get; set; }

        public override void Execute(Shader shader)
        {
            GL.BlendFunc(SourceFactor, DestinationFactor);
        }

        public override string ToString()
        {
            return string.Format("BlendFunc({0}, {1})", SourceFactor, DestinationFactor);
        }
    }
}
