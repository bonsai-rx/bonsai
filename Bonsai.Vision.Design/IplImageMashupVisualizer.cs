using System;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(IplImageMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, IplImageVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class IplImageMashupVisualizer : MashupTypeVisualizer
    {
        IplImage color;
        ImageMashupVisualizer visualizer;

        public override void Show(object value)
        {
            var image = (IplImage)value;
            var visualizerImage = visualizer.VisualizerImage;
            if (visualizerImage != null && image != null)
            {
                // Treat image as mask and overlay it
                if (image.Channels == 1)
                {
                    var overlay = image;
                    // If target is a color image, convert before overlay
                    if (visualizerImage.Channels == 3)
                    {
                        color = IplImageHelper.EnsureImageFormat(color, visualizerImage.Size, visualizerImage.Depth, visualizerImage.Channels);
                        CV.CvtColor(image, color, ColorConversion.Gray2Bgr);
                        overlay = color;
                    }

                    CV.Copy(overlay, visualizerImage, image);
                }
            }
        }

        public override void Load(IServiceProvider provider)
        {
            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
        }

        public override void Unload()
        {
        }
    }
}
