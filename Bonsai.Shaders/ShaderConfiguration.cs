using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [XmlInclude(typeof(TexturedQuad))]
    public class ShaderConfiguration
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

        public ShaderConfiguration()
        {
            Enabled = true;
            VertexShader = ShaderPrograms.UniformScaleShiftTexCoord;
            FragmentShader = ShaderPrograms.UniformSampler;
        }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        [Category("Shaders")]
        [Editor("Bonsai.Shaders.Design.GlslScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string VertexShader { get; set; }

        [Category("Shaders")]
        [Editor("Bonsai.Shaders.Design.GlslScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FragmentShader { get; set; }

        [Editor("Bonsai.Shaders.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        internal virtual void Configure(Shader shader)
        {
            shader.Enabled = Enabled;
        }
    }
}
