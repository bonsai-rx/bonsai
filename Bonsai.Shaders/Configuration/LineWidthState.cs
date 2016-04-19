﻿using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class LineWidthState : StateConfiguration
    {
        [Description("Specifies the width of rasterized lines.")]
        public float Width { get; set; }

        public override void Execute(Shader shader)
        {
            GL.LineWidth(Width);
        }

        public override string ToString()
        {
            return string.Format("LineWidth({0})", Width);
        }
    }
}
