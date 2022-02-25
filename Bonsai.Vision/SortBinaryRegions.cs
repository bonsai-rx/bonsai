using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that sorts each collection of binary regions in the
    /// sequence by descending order of area.
    /// </summary>
    [Description("Sorts each collection of binary regions in the sequence by descending order of area.")]
    public class SortBinaryRegions : Transform<ConnectedComponentCollection, ConnectedComponentCollection>
    {
        /// <summary>
        /// Sorts each collection of binary regions in an observable sequence by
        /// descending order of area.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="ConnectedComponentCollection"/> objects to sort.
        /// </param>
        /// <returns>
        /// A sequence containing the sorted <see cref="ConnectedComponentCollection"/>
        /// objects, where the sequence of connected components is ordered by descending
        /// order of the area of each component.
        /// </returns>
        public override IObservable<ConnectedComponentCollection> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var components = input.OrderByDescending(xs => xs.Area).ToList();
                return new ConnectedComponentCollection(components, input.ImageSize);
            });
        }
    }
}
