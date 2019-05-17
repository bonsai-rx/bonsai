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
    [Description("Reads a block of pixels from the frame buffer.")]
    public class ReadPixels : Source<IplImage>
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The optional pixel region of interest to read from the window, in upper left coordinates.")]
        public Rect? RegionOfInterest { get; set; }

        public override IObservable<IplImage> Generate()
        {
            return Generate(updateFrame.Generate().Take(1));
        }

        public IObservable<IplImage> Generate<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var flipBuffer = default(IplImage);
                return source.CombineEither(
                    ShaderManager.WindowSource,
                    (input, window) =>
                    {
                        IplImage temp;
                        PixelType pixelType;
                        PixelFormat pixelFormat;
                        var rect = RegionOfInterest.GetValueOrDefault(new Rect(0, 0, window.Width, window.Height));
                        var result = new IplImage(new Size(rect.Width, rect.Height), IplDepth.U8, 3);
                        rect.Y = window.Height - (rect.Y + rect.Height);
                        TextureHelper.PackPixelStore(result, out pixelFormat, out pixelType);
                        GL.ReadPixels(rect.X, rect.Y, rect.Width, rect.Height, pixelFormat, pixelType, result.ImageData);
                        if (flipBuffer == null || flipBuffer.Size != result.Size)
                        {
                            flipBuffer = new IplImage(result.Size, result.Depth, result.Channels);
                        }
                        CV.Flip(result, flipBuffer, FlipMode.Vertical);
                        temp = result;
                        result = flipBuffer;
                        flipBuffer = temp;
                        return result;
                    });
            });
        }
    }
}
