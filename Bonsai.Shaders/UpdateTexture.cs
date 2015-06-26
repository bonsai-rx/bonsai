using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Updates the pixel store of a texture bound to the specified shader.")]
    public class UpdateTexture : Sink<IplImage>
    {
        [Description("The name of the shader program.")]
        [Editor("Bonsai.Shaders.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.CombineEither(
                ShaderManager.ReserveShader(ShaderName),
                (input, shader) =>
                {
                    shader.Update(() =>
                    {
                        TextureHelper.UpdateTexture(shader.Texture, input);
                    });
                    return input;
                });
        }
    }
}
