using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class ShaderConfiguration
    {
        public ShaderConfiguration()
        {
            Visible = true;
            VertexShader = ShaderPrograms.UniformScaleShiftTexCoord;
            FragmentShader = ShaderPrograms.UniformSampler;
        }

        public string Name { get; set; }

        public bool Visible { get; set; }

        [Category("Shaders")]
        [Editor(DesignTypes.MultilineStringEditor, "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string VertexShader { get; set; }

        [Category("Shaders")]
        [Editor(DesignTypes.MultilineStringEditor, "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FragmentShader { get; set; }
    }
}
