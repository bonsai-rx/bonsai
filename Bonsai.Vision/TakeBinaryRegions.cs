using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that takes the specified number of binary regions from
    /// each collection in the sequence.
    /// </summary>
    [Description("Takes the specified number of binary regions from each collection in the sequence.")]
    public class TakeBinaryRegions : Transform<ConnectedComponentCollection, ConnectedComponentCollection>
    {
        /// <summary>
        /// Gets or sets the number of binary regions to take.
        /// </summary>
        [Description("The number of binary regions to take.")]
        public int Count { get; set; } = 1;

        static List<ConnectedComponent> Process(IEnumerable<ConnectedComponent> source, int count)
        {
            var output = new List<ConnectedComponent>(count);
            output.AddRange(source.Take(count));
            while (output.Count < count)
            {
                output.Add(new ConnectedComponent
                {
                    Centroid = new Point2f(float.NaN, float.NaN),
                    Orientation = double.NaN
                });
            }

            return output;
        }

        /// <summary>
        /// Takes the specified number of binary regions from each collection in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="ConnectedComponentCollection"/> objects from which
        /// to take the specified number of binary regions.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ConnectedComponentCollection"/> objects, where
        /// each collection always contains the specified number of binary regions.
        /// If the corresponding collection in the original sequence does not have
        /// enough regions, the missing elements are set to the empty region.
        /// </returns>
        public override IObservable<ConnectedComponentCollection> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var output = Process(input, Count);
                return new ConnectedComponentCollection(output, input.ImageSize);
            });
        }

        /// <summary>
        /// Takes the specified number of binary regions from each collection in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="ConnectedComponent"/> collection objects from which
        /// to take the specified number of binary regions.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ConnectedComponentCollection"/> objects, where
        /// each collection always contains the specified number of binary regions.
        /// If the corresponding collection in the original sequence does not have
        /// enough regions, the missing elements are set to the empty region.
        /// </returns>
        public IObservable<ConnectedComponentCollection> Process(IObservable<IEnumerable<ConnectedComponent>> source)
        {
            return source.Select(input =>
            {
                var output = Process(input, Count);
                return new ConnectedComponentCollection(output, Size.Zero);
            });
        }
    }
}
