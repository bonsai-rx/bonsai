using OpenCV.Net;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents the two endpoints of a line segment in pixel-accurate coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LineSegment
    {
        /// <summary>
        /// The first endpoint of the line segment.
        /// </summary>
        public Point Start;

        /// <summary>
        /// The second endpoint of the line segment.
        /// </summary>
        public Point End;

        /// <summary>
        /// Creates a <see cref="string"/> representation of this
        /// <see cref="LineSegment"/> structure.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing all the parameter values of this
        /// <see cref="LineSegment"/> structure.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Line2f(Start={0}, End={1})", Start, End);
        }
    }
}
