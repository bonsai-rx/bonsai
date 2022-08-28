using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that computes the average binary region from
    /// each collection of connected components in the sequence.
    /// </summary>
    [Description("Computes the average binary region from each collection of connected components in the sequence.")]
    public class AverageBinaryRegion : Transform<ConnectedComponentCollection, ConnectedComponent>
    {
        /// <summary>
        /// Computes the average binary region from each collection of connected
        /// components in an observable sequence.
        /// </summary>
        /// <param name="source">A sequence of <see cref="ConnectedComponentCollection"/> objects.</param>
        /// <returns>
        /// A <see cref="ConnectedComponent"/> representing the average binary region
        /// from each collection of connected components in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public override IObservable<ConnectedComponent> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var result = new ConnectedComponent();

                double angle = 0;
                double area = 0;
                Point2f centroid = new Point2f();
                for (int i = 0; i < input.Count; i++)
                {
                    var component = input[i];
                    centroid.X += component.Centroid.X;
                    centroid.Y += component.Centroid.Y;
                    angle += component.Orientation;
                    area += component.Area;
                }

                if (input.Count > 0)
                {
                    centroid.X /= input.Count;
                    centroid.Y /= input.Count;
                    result.Centroid = centroid;
                    result.Orientation = angle / input.Count;
                    result.Area = area;
                    result.Contour = Contour.FromSeq(null);
                }
                else
                {
                    result.Centroid = new Point2f(float.NaN, float.NaN);
                    result.Orientation = double.NaN;
                }

                return result;
            });
        }
    }
}
