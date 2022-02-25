using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that computes image moments from polygonal contours
    /// or individual frames in the sequence to extract binary region properties.
    /// </summary>
    /// <remarks>
    /// If the area of any of the extracted connected components is zero, the
    /// centroid and orientation angle for that connected component will be set to
    /// <see cref="float.NaN"/>.
    /// </remarks>
    [Description("Computes image moments from polygonal contours or individual frames in the sequence to extract binary region properties.")]
    public class BinaryRegionAnalysis : Transform<Contours, ConnectedComponentCollection>
    {
        /// <summary>
        /// Computes image moments from individual frames in an observable sequence
        /// to extract binary region properties.
        /// </summary>
        /// <param name="source">
        /// A sequence of rasterized shapes where every non-zero pixel is treated
        /// as having a weight of one.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ConnectedComponent"/> objects containing the
        /// binary region measurements for all the pixels in the image considered
        /// as a single object.
        /// </returns>
        public IObservable<ConnectedComponent> Process(IObservable<IplImage> source)
        {
            return source.Select(input => ConnectedComponent.FromImage(input, binary: true));
        }

        /// <summary>
        /// Computes image moments from individual polygons in an observable sequence
        /// to extract binary region properties.
        /// </summary>
        /// <param name="source">
        /// A sequence of polygons for which to compute binary region properties.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ConnectedComponent"/> objects containing the
        /// binary region measurements for all the polygons in the sequence.
        /// </returns>
        public IObservable<ConnectedComponent> Process(IObservable<Seq> source)
        {
            return source.Select(input => ConnectedComponent.FromContour(input));
        }

        /// <summary>
        /// Computes image moments from polygonal contours in an observable sequence
        /// to extract binary region properties.
        /// </summary>
        /// <param name="source">
        /// A sequence of hierarchical polygonal contours for which to compute binary
        /// region properties.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ConnectedComponentCollection"/> objects containing the
        /// binary region measurements for all the polygons in the hierarchy of polygonal
        /// contours.
        /// </returns>
        public override IObservable<ConnectedComponentCollection> Process(IObservable<Contours> source)
        {
            return source.Select(input =>
            {
                var currentContour = input.FirstContour;
                var output = new ConnectedComponentCollection(input.ImageSize);

                while (currentContour != null)
                {
                    var component = ConnectedComponent.FromContour(currentContour);
                    currentContour = currentContour.HNext;
                    output.Add(component);
                }

                return output;
            });
        }

        /// <summary>
        /// Computes image moments from polygonal contours in an observable sequence
        /// to extract binary region properties, where each binary region will be
        /// associated with a corresponding patch of pixels.
        /// </summary>
        /// <param name="source">
        /// A sequence of hierarchical polygonal contours for which to compute binary
        /// region properties, paired with the image from which they were extracted.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ConnectedComponentCollection"/> objects containing the
        /// binary region measurements for all the polygons in the hierarchy of polygonal
        /// contours. Each binary region will be associated with the subregion of the
        /// paired image corresponding to the bounding box of the binary region shape.
        /// </returns>
        public IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<Contours, IplImage>> source)
        {
            return source.Select(input =>
            {
                var image = input.Item2;
                var contours = input.Item1;
                var currentContour = contours.FirstContour;
                var output = new ConnectedComponentCollection(image.Size);

                while (currentContour != null)
                {
                    var component = ConnectedComponent.FromContour(currentContour);
                    component.Patch = image.GetSubRect(component.Contour.Rect);
                    currentContour = currentContour.HNext;
                    output.Add(component);
                }

                return output;
            });
        }
    }
}
