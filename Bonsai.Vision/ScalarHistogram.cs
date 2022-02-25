using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents the per-channel histograms for all channels in a multi-channel array.
    /// </summary>
    public class ScalarHistogram
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarHistogram"/> class
        /// using the specified per-channel histograms.
        /// </summary>
        /// <param name="val0">The histogram for the first channel in the array.</param>
        /// <param name="val1">The histogram for the second channel in the array.</param>
        /// <param name="val2">The histogram for the third channel in the array.</param>
        /// <param name="val3">The histogram for the fourth channel in the array.</param>
        public ScalarHistogram(Histogram val0, Histogram val1, Histogram val2, Histogram val3)
        {
            Val0 = val0;
            Val1 = val1;
            Val2 = val2;
            Val3 = val3;
        }

        /// <summary>
        /// Gets the histogram for the first channel in the array.
        /// </summary>
        public Histogram Val0 { get; private set; }

        /// <summary>
        /// Gets the histogram for the second channel in the array.
        /// </summary>
        public Histogram Val1 { get; private set; }

        /// <summary>
        /// Gets the histogram for the third channel in the array.
        /// </summary>
        public Histogram Val2 { get; private set; }

        /// <summary>
        /// Gets the histogram for the fourth channel in the array.
        /// </summary>
        public Histogram Val3 { get; private set; }
    }
}
