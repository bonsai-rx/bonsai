using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
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
        public UpdateTexture()
        {
            TextureName = "tex";
        }

        [Description("The name of the shader program.")]
        [Editor("Bonsai.Shaders.Design.ShaderConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string ShaderName { get; set; }

        public string TextureName { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var texture = 0;
                var name = TextureName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A texture sampler name must be specified.");
                }

                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName).Do(shader =>
                    {
                        shader.Update(() =>
                        {
                            var textureUnit = shader.TextureUnits.FirstOrDefault(t => t.Name == name);
                            if (textureUnit == null)
                            {
                                throw new InvalidOperationException(string.Format(
                                    "The texture unit \"{0}\" was not found in shader program \"{1}\".",
                                    name,
                                    ShaderName));
                            }

                            texture = textureUnit.GetTexture();
                        });
                    }),
                    (input, shader) =>
                    {
                        shader.Update(() =>
                        {
                            TextureHelper.UpdateTexture(texture, input);
                        });
                        return input;
                    });
            });
        }
    }
}
