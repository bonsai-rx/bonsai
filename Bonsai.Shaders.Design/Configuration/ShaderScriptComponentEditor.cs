﻿using Bonsai.Design;
using Bonsai.Shaders.Design;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Bonsai.Shaders.Configuration.Design
{
    /// <summary>
    /// Provides a custom user interface for editing and validating GLSL shader scripts.
    /// </summary>
    public class ShaderScriptComponentEditor : WorkflowComponentEditor
    {
        static GlslScriptEditorDialog editorDialog;

        /// <inheritdoc/>
        public override bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner)
        {
            if (editorDialog == null)
            {
                editorDialog = new GlslScriptEditorDialog();
                editorDialog.InitialDirectory = Environment.CurrentDirectory;
                foreach (var example in GetShaderExamples())
                {
                    editorDialog.ScriptExamples.Add(example);
                }

                editorDialog.FormClosed += (sender, e) => editorDialog = null;
                editorDialog.Load += (sender, e) =>
                {
                    if (editorDialog.Owner != null)
                    {
                        editorDialog.Icon = editorDialog.Owner.Icon;
                        editorDialog.ShowIcon = true;
                    }
                };

                editorDialog.Show(owner);
            }
            else
            {
                if (editorDialog.WindowState == FormWindowState.Minimized)
                {
                    editorDialog.WindowState = FormWindowState.Normal;
                }
                editorDialog.Activate();
            }
            return false;
        }

        /// <summary>
        /// Gets the GLSL example scripts that can be created from this editor.
        /// </summary>
        /// <returns>
        /// An array of GLSL examples that this editor can create.
        /// </returns>
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
layout(location = 0) in vec2 vp;
layout(location = 1) in vec2 vt;
out vec2 texCoord;

void main()
{
  gl_Position = vec4(vp * scale + shift, 0.0, 1.0);
  texCoord = vt;
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
uniform mat4 normalMatrix;
layout(location = 0) in vec3 vp;
layout(location = 1) in vec2 vt;
layout(location = 2) in vec3 vn;
out vec3 position;
out vec2 texCoord;
out vec3 normal;

void main()
{
  vec4 v = modelview * vec4(vp, 1.0);
  gl_Position = projection * v;
  position = vec3(v);
  texCoord = vt;
  normal = normalize(vec3(normalMatrix * vec4(vn, 0.0)));
}
"
                },

                new GlslScriptExample
                {
                    Name = "Skybox",
                    Type = ShaderType.VertexShader,
                    Source = @"#version 400
uniform mat4 modelview;
uniform mat4 projection;
layout(location = 0) in vec3 vp;
out vec3 texCoord;

void main()
{
  vec4 position = projection * modelview * vec4(vp, 1.0);
  gl_Position = position.xyww;
  texCoord = vp;
}
"
                },

                new GlslScriptExample
                {
                    Name = "Viewport Effect",
                    Type = ShaderType.FragmentShader,
                    Source = @"#version 400
in vec2 texCoord;
out vec4 fragColor;

void main()
{
  fragColor = vec4(texCoord, 0.0, 1.0);
}
"
                },

                new GlslScriptExample
                {
                    Name = "Diffuse Texture",
                    Type = ShaderType.FragmentShader,
                    Source = @"#version 400
uniform sampler2D tex;
in vec2 texCoord;
out vec4 fragColor;

void main()
{
  vec4 texel = texture(tex, texCoord);
  fragColor = texel;
}
"
                },

                new GlslScriptExample
                {
                    Name = "Cubemap Texture",
                    Type = ShaderType.FragmentShader,
                    Source = @"#version 400
uniform samplerCube tex;
in vec3 texCoord;
out vec4 fragColor;

void main()
{
  vec4 texel = texture(tex, texCoord);
  fragColor = texel;
}
"
                },

                new GlslScriptExample
                {
                    Name = "Phong Shading",
                    Type = ShaderType.FragmentShader,
                    Source = @"#version 400
uniform vec3 colorAmbient;
uniform vec3 colorDiffuse;
uniform vec3 colorSpecular;
uniform float shininess = 1.0;
uniform sampler2D tex;
uniform vec3 light;
in vec3 position;
in vec2 texCoord;
in vec3 normal;
out vec4 fragColor;

void main()
{
  vec3 L = normalize(light - position);
  vec3 R = normalize(-reflect(L, normal));
  vec3 V = normalize(-position);
  vec4 texel = texture(tex, texCoord);

  vec3 Iamb = colorAmbient * texel.rgb;
  vec3 Idiff = colorDiffuse * texel.rgb * max(dot(normal, L), 0.0);
  vec3 Ispec = colorSpecular * pow(max(dot(R, V), 0.0), shininess);

  fragColor = vec4(Iamb + Idiff + Ispec, texel.a);
}
"
                }
            };
        }
    }
}
