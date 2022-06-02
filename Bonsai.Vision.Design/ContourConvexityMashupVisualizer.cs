using System;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(ContourConvexityMashupVisualizer), Target = typeof(MashupSource<ImageMashupVisualizer, ContourConvexityVisualizer>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that overlays the results of contour convexity
    /// analysis over an existing image visualizer.
    /// </summary>
    public class ContourConvexityMashupVisualizer : DialogTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var image = visualizer.VisualizerImage;
            var contourConvexity = (ContourConvexity)value;
            var contour = contourConvexity.Contour;

            if (image != null && contour != null)
            {
                CV.DrawContours(image, contourConvexity.ConvexHull, Scalar.Rgb(0, 255, 0), Scalar.All(0), 0);
                DrawingHelper.DrawConvexityDefects(image, contourConvexity.ConvexityDefects, Scalar.Rgb(204, 0, 204));
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
