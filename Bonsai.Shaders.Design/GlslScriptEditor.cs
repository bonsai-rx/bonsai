using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL4;

namespace Bonsai.Shaders.Design
{
    public class GlslScriptEditor : UITypeEditor
    {
        protected virtual ShaderType? GetShaderType()
        {
            return null;
        }

        protected virtual GlslScriptExample[] GetShaderExamples()
        {
            return null;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                var fileName = value as string;
                var editorDialog = new GlslScriptEditorDialog();
                editorDialog.FileName = fileName;
                editorDialog.ScriptType = GetShaderType();
                var examples = GetShaderExamples();
                if (examples != null)
                {
                    foreach (var example in examples)
                    {
                        editorDialog.ScriptExamples.Add(example);
                    }
                }

                if (editorService.ShowDialog(editorDialog) == DialogResult.OK)
                {
                    return editorDialog.FileName;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }

    class VertScriptEditor : GlslScriptEditor
    {
        protected override ShaderType? GetShaderType()
        {
            return ShaderType.VertexShader;
        }

        protected override GlslScriptExample[] GetShaderExamples()
        {
            return new[]
            {
                new GlslScriptExample
                {
                    Name = "Clip-space Textured",
                    Source = @"#version 400
uniform vec2 scale = vec2(1, 1);
uniform vec2 shift;
in vec2 vp;
in vec2 vt;
out vec2 tex_coord;

void main()
{
  gl_Position = vec4(vp * scale + shift, 0.0, 1.0);
  tex_coord = vt;
}
"
                },

                new GlslScriptExample
                {
                    Name = "Textured Model",
                    Source = @"#version 400
uniform mat4 modelview;
uniform mat4 projection;
in vec3 vp;
in vec2 vt;
in vec3 vn;
out vec3 position;
out vec2 tex_coord;
out vec3 normal;

void main()
{
  mat4 normalmat = transpose(inverse(modelview));
  vec4 v = modelview * vec4(vp, 1.0);
  gl_Position = projection * v;
  position = vec3(v);
  tex_coord = vt;
  normal = normalize(vec3(normalmat * vec4(vn, 0.0)));
}
"
                }
            };
        }
    }

    class FragScriptEditor : GlslScriptEditor
    {
        protected override ShaderType? GetShaderType()
        {
            return ShaderType.FragmentShader;
        }

        protected override GlslScriptExample[] GetShaderExamples()
        {
            return new[]
            {
                new GlslScriptExample
                {
                    Name = "Diffuse Texture",
                    Source = @"#version 400
uniform sampler2D tex;
in vec2 tex_coord;
out vec4 frag_colour;

void main()
{
  vec4 texel = texture(tex, tex_coord);
  frag_colour = texel;
}
"
                },

                new GlslScriptExample
                {
                    Name = "Phong Shading",
                    Source = @"#version 400
uniform vec3 Ka;
uniform vec3 Kd;
uniform vec3 Ks;
uniform float Ns = 1.0;
uniform sampler2D map_Kd;
uniform vec3 light;
in vec3 position;
in vec2 tex_coord;
in vec3 normal;
out vec4 frag_colour;

void main()
{
  vec3 L = normalize(light - position);
  vec3 R = normalize(-reflect(L,normal));
  vec3 V = normalize(-position);

  vec3 Iamb = Ka;
  vec3 Idiff = Kd * texture(map_Kd, tex_coord).rgb * max(dot(normal,L), 0.0);
  vec3 Ispec = Ks * pow(max(dot(R,V),0.0),Ns);

  frag_colour = vec4(Iamb + Idiff + Ispec,1.0);
}
"
                }
            };
        }
    }

    class GeomScriptEditor : GlslScriptEditor
    {
        protected override ShaderType? GetShaderType()
        {
            return ShaderType.GeometryShader;
        }
    }

    class CompScriptEditor : GlslScriptEditor
    {
        protected override ShaderType? GetShaderType()
        {
            return ShaderType.ComputeShader;
        }
    }
}
