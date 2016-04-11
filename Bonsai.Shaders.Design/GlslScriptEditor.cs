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
                editorDialog.ShaderType = GetShaderType();
                var examples = GetShaderExamples();
                if (examples != null)
                {
                    foreach (var example in examples)
                    {
                        editorDialog.ShaderExamples.Add(example);
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
}
