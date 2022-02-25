using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that tests whether the activation intensity is above
    /// a given threshold for each specified region in the sequence.
    /// </summary>
    [Description("Tests whether the activation intensity is above a given threshold for each specified region in the sequence.")]
    public class RoiActivityDetected : Transform<RegionActivityCollection, bool>
    {
        /// <summary>
        /// Gets or sets the index of the region of interest to test.
        /// </summary>
        /// <remarks>
        /// If no index is specified, the activity inside any specified region
        /// of interest will be considered.
        /// </remarks>
        [Description("The index of the region of interest to test.")]
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets the activity detection threshold.
        /// </summary>
        [Description("The activity detection threshold.")]
        public double Threshold { get; set; }

        /// <summary>
        /// Tests whether the activation intensity is above a given threshold for
        /// each specified region in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="RegionActivityCollection"/> containing the
        /// regions of interest for which activation intensity was extracted.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the
        /// activation intensity of the specified region of interest exceeded
        /// the activation threshold.
        /// </returns>
        public override IObservable<bool> Process(IObservable<RegionActivityCollection> source)
        {
            return source.Select(input =>
            {
                if (Index.HasValue && Index >= 0 && Index < input.Count)
                {
                    return input[Index.Value].Activity.Val0 > Threshold;
                }
                else return input.Where(region => region.Activity.Val0 > Threshold).Count() > 0;
            });
        }
    }
}
