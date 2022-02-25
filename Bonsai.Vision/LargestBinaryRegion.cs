using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that finds the largest binary region from each
    /// collection of connected components in the sequence.
    /// </summary>
    [Description("Finds the largest binary region from each collection of connected components in the sequence.")]
    public class LargestBinaryRegion : Transform<ConnectedComponentCollection, ConnectedComponent>
    {
        /// <summary>
        /// Finds the largest binary region from each collection of connected
        /// components in an observable sequence.
        /// </summary>
        /// <param name="source">A sequence of <see cref="ConnectedComponentCollection"/> objects.</param>
        /// <returns>
        /// A <see cref="ConnectedComponent"/> representing the largest binary region
        /// from each collection of connected components in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public override IObservable<ConnectedComponent> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var largest = new ConnectedComponent();
                if (input.Count > 0)
                {
                    for (int i = 0; i < input.Count; i++)
                    {
                        var component = input[i];
                        if (component.Area > largest.Area)
                        {
                            largest = component;
                        }
                    }
                }
                else
                {
                    largest.Centroid = new Point2f(float.NaN, float.NaN);
                    largest.Orientation = double.NaN;
                }

                return largest;
            });
        }
    }
}
