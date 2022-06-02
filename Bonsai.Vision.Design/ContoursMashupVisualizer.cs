using System;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(ContoursMashupVisualizer), Target = typeof(MashupSource<ImageMashupVisualizer, ContoursVisualizer>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that overlays a hierarchy of polygonal contours
    /// over an existing image visualizer.
    /// </summary>
    public class ContoursMashupVisualizer : DialogTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var contours = (Contours)value;
            var image = visualizer.VisualizerImage;
            if (image != null && contours.FirstContour != null)
            {
                CV.DrawContours(image, contours.FirstContour, Scalar.All(255), Scalar.All(128), 2);
            }
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(MashupVisualizer));
        }

        /// <inheritdoc/>
        public override void Unload()
        {
        }
    }
}
