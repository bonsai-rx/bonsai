using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that merges connected component collections in the
    /// sequence into a single collection.
    /// </summary>
    [Description("Merges connected component collections in the sequence into a single collection.")]
    public class MergeBinaryRegions : Transform<Tuple<ConnectedComponentCollection, ConnectedComponentCollection>, ConnectedComponentCollection>
    {
        /// <summary>
        /// Merges connected component collections in an observable sequence into
        /// a single collection.
        /// </summary>
        /// <param name="source">
        /// A sequence of the connected component collection pairs to merge.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ConnectedComponentCollection"/> objects
        /// representing the merged collections.
        /// </returns>
        public override IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<ConnectedComponentCollection, ConnectedComponentCollection>> source)
        {
            return source.Select(input =>
            {
                var first = input.Item1;
                var second = input.Item2;
                var output = new ConnectedComponentCollection(first.Concat(second).ToList(), first.ImageSize);
                return output;
            });
        }
    }
}
