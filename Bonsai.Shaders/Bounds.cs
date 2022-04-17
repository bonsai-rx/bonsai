using OpenTK;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an axis-aligned bounding box for mesh vertices.
    /// </summary>
    public class Bounds
    {
        /// <summary>
        /// Represents a <see cref="Bounds"/> instance with empty extents.
        /// </summary>
        public static readonly Bounds Empty = new Bounds(Vector3.Zero, Vector3.Zero);
        Vector3 center;
        Vector3 extents;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bounds"/> class using
        /// the specified bounding-box center and dimensions.
        /// </summary>
        /// <param name="center">The center of the axis-aligned bounding box.</param>
        /// <param name="extents">
        /// The dimensions of the axis-aligned bounding box, measured from
        /// the center to the extremities.
        /// </param>
        public Bounds(Vector3 center, Vector3 extents)
        {
            this.center = center;
            this.extents = extents;
        }

        /// <summary>
        /// Gets the center of the axis-aligned bounding box.
        /// </summary>
        public Vector3 Center
        {
            get { return center; }
        }

        /// <summary>
        /// Gets the dimensions of the axis-aligned bounding box, measured from
        /// the center to the extremities.
        /// </summary>
        public Vector3 Extents
        {
            get { return extents; }
        }

        /// <summary>
        /// Gets the minimum point of the axis-aligned bounding box.
        /// </summary>
        public Vector3 Minimum
        {
            get { return center - extents; }
        }

        /// <summary>
        /// Gets the maximum point of the axis-aligned bounding box.
        /// </summary>
        public Vector3 Maximum
        {
            get { return center + extents; }
        }

        /// <summary>
        /// Gets the full size of the axis-aligned bounding box across all
        /// three dimensions.
        /// </summary>
        public Vector3 Size
        {
            get { return 2 * extents; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"(Center:{center}, Extents:{extents})";
        }
    }
}
