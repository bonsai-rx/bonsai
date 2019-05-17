using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Reads the input texture data to an image.")]
    public class RetrieveImage : Transform<Texture, IplImage>
    {
        public RetrieveImage()
        {
            Depth = IplDepth.U8;
            Channels = 3;
        }

        [Description("The bit depth of each pixel in the output image.")]
        public IplDepth Depth { get; set; }

        [Description("The number of channels in the output image.")]
        public int Channels { get; set; }

        [Description("Specifies the optional flip mode applied to the retrieved image.")]
        public FlipMode? FlipMode { get; set; }

        public override IObservable<IplImage> Process(IObservable<Texture> source)
        {
            return Observable.Defer(() =>
            {
                var flipBuffer = default(IplImage);
                return source.Select(texture =>
                {
                    int width, height;
                    PixelType pixelType;
                    PixelFormat pixelFormat;
                    GL.BindTexture(TextureTarget.Texture2D, texture.Id);
                    GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out width);
                    GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out height);
                    var result = new IplImage(new Size(width, height), Depth, Channels);
                    TextureHelper.PackPixelStore(result, out pixelFormat, out pixelType);
                    GL.GetTexImage(TextureTarget.Texture2D, 0, pixelFormat, pixelType, result.ImageData);

                    var flipMode = FlipMode;
                    if (flipMode.HasValue)
                    {
                        IplImage temp;
                        if (flipBuffer == null) flipBuffer = new IplImage(result.Size, result.Depth, result.Channels);
                        CV.Flip(result, flipBuffer, flipMode.Value);
                        temp = result;
                        result = flipBuffer;
                        flipBuffer = temp;
                    }
                    return result;
                });
            });
        }
    }
}
