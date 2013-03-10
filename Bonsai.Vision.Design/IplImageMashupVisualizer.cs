using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(IplImageMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, IplImageVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class IplImageMashupVisualizer : DialogTypeVisualizer
    {
        IplImage color;
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            var image = (IplImage)value;
            var visualizerImage = visualizer.VisualizerImage;
            if (visualizerImage != null && image != null)
            {
                // Treat image as mask and overlay it
                if (image.NumChannels == 1)
                {
                    var overlay = image;
                    // If target is a color image, convert before overlay
                    if (visualizerImage.NumChannels == 3)
                    {
                        color = IplImageHelper.EnsureImageFormat(color, visualizerImage.Size, visualizerImage.Depth, visualizerImage.NumChannels);
                        ImgProc.cvCvtColor(image, color, ColorConversion.GRAY2BGR);
                        overlay = color;
                    }

                    Core.cvCopy(overlay, visualizerImage, image);
                }
            }
        }

        public override void Load(IServiceProvider provider)
        {
            visualizer = (IplImageVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
        }

        public override void Unload()
        {
        }
    }
}
