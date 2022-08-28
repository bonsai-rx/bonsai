using OpenCV.Net;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents the parameters of a circle.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Circle
    {
        /// <summary>
        /// The center of the circle.
        /// </summary>
        public Point2f Center;

        /// <summary>
        /// The radius of the circle.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Creates a <see cref="string"/> representation of this
        /// <see cref="Circle"/> structure.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing all the parameter values of this
        /// <see cref="Circle"/> structure.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Circle(Center={0}, Radius={1})", Center, Radius);
        }
    }
}
