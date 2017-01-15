using Bonsai.Design;
using Bonsai.Shaders.Design;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Shaders.Configuration.Design
{
    public class ShaderConfigurationComponentEditor : WorkflowComponentEditor
    {
        static ShaderConfigurationEditorDialog editorDialog;

        internal void EditConfiguration(IWin32Window owner)
        {
            if (editorDialog == null)
            {
                RefreshEventHandler editorRefreshed;
                editorDialog = new ShaderConfigurationEditorDialog();
                editorRefreshed = e => editorDialog.Close();
                TypeDescriptor.Refreshed += editorRefreshed;
                editorDialog.FormClosed += (sender, e) =>
                {
                    TypeDescriptor.Refreshed -= editorRefreshed;
                    editorDialog = null;
                };
                editorDialog.SelectedPage = ShaderConfigurationEditorPage.Window;
                foreach (var example in GetShaderExamples())
                {
                    editorDialog.ScriptExamples.Add(example);
                }
                editorDialog.Show(owner);
            }
            else editorDialog.Activate();
        }

        internal void EditConfiguration(ShaderConfigurationEditorPage selectedPage, IWin32Window owner)
        {
            EditConfiguration(owner);
            editorDialog.SelectedPage = selectedPage;
        }

        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            EditConfiguration(owner);
            return false;
        }

        protected virtual GlslScriptExample[] GetShaderExamples()
        {
            return new[]
            {
                new GlslScriptExample
                {
                    Name = "Clip-space Textured",
                    Type = ShaderType.VertexShader,
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
                    Type = ShaderType.VertexShader,
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
                },

                new GlslScriptExample
                {
                    Name = "Diffuse Texture",
                    Type = ShaderType.FragmentShader,
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
                    Type = ShaderType.FragmentShader,
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
}
