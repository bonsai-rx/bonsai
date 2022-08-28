using OpenCV.Net;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents the minimum and maximum values of a 2D array, and their corresponding locations.
    /// </summary>
    public struct ArrayExtrema
    {
        /// <summary>
        /// The minimum value of the 2D array.
        /// </summary>
        public double MinValue;

        /// <summary>
        /// The maximum value of the 2D array.
        /// </summary>
        public double MaxValue;

        /// <summary>
        /// The zero-based index of the minimum value in the 2D array.
        /// </summary>
        public Point MinLocation;

        /// <summary>
        /// The zero-based index of the maximum value in the 2D array.
        /// </summary>
        public Point MaxLocation;

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{{MinValue={0}, MaxValue={1}, MinLocation={2}, MaxLocation={3}}}",
                                 MinValue, MaxValue,
                                 MinLocation, MaxLocation);
        }
    }
}
