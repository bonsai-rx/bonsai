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
        /// Initializes a new instance of the <see cref="Circle"/> structure with
        /// the specified parameters.
        /// </summary>
        /// <param name="center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        public Circle(Point2f center, float radius)
        {
            Center = center;
            Radius = radius;
        }

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
