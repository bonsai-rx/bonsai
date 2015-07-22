using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [XmlInclude(typeof(PointSprite))]
    [XmlInclude(typeof(TexturedQuad))]
    [XmlInclude(typeof(TexturedModel))]
    public class ShaderConfiguration
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();
        readonly TextureConfigurationCollection textureUnits = new TextureConfigurationCollection();

        public ShaderConfiguration()
        {
            Enabled = true;
            VertexShader = DefaultVertexShader;
            FragmentShader = DefaultFragmentShader;
        }

        public string Name { get; set; }

        [Category("State")]
        public bool Enabled { get; set; }

        [Category("Shaders")]
        [Editor("Bonsai.Shaders.Design.GlslScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string VertexShader { get; set; }

        [Category("Shaders")]
        [Editor("Bonsai.Shaders.Design.GlslScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string GeometryShader { get; set; }

        [Category("Shaders")]
        [Editor("Bonsai.Shaders.Design.GlslScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FragmentShader { get; set; }

        [Category("State")]
        [Editor("Bonsai.Shaders.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [Category("State")]
        [Editor("Bonsai.Shaders.Design.TextureConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public TextureConfigurationCollection TextureUnits
        {
            get { return textureUnits; }
        }

        internal virtual void Configure(Shader shader)
        {
            shader.Enabled = Enabled;
        }

        const string DefaultVertexShader = @"
#version 400
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
";

        const string DefaultFragmentShader = @"
#version 400
in vec2 tex_coord;
out vec4 frag_colour;

void main()
{
  frag_colour = vec4(1.0, 0.0, 0.0, 1.0);
}
";
    }
}
