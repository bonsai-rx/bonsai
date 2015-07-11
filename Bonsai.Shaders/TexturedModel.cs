using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class TexturedModel : ShaderConfiguration
    {
        public TexturedModel()
        {
            VertexShader = DefaultVertexShader;
            FragmentShader = DefaultFragmentShader;
        }

        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [FileNameFilter("OBJ Files (*.obj)|*.obj")]
        [Description("The name of the model file.")]
        public string FileName { get; set; }

        internal override void Configure(Shader shader)
        {
            base.Configure(shader);
            shader.Update(() =>
            {
                ObjReader.ReadObject(shader, FileName);
            });
        }

        const string DefaultVertexShader = @"
#version 400
layout(location = 0) in vec3 vp;
layout(location = 1) in vec2 vt;
layout(location = 2) out vec3 vn;
out vec2 tex_coord;
out vec3 normal;
uniform mat4 mvp;

void main()
{
  vec4 v = vec4(vp, 1.0);
  gl_Position = mvp * v;
  tex_coord = vt;
  normal = vn;
}
";

        const string DefaultFragmentShader = @"
#version 400
uniform sampler2D tex;
in vec2 tex_coord;
out vec4 frag_colour;

void main()
{
  vec4 texel = texture(tex, tex_coord);
  frag_colour = texel;
}
";
    }
}
