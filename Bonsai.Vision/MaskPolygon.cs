using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that applies a polygonal mask to each image in
    /// the sequence.
    /// </summary>
    [Description("Applies a polygonal mask to each image in the sequence.")]
    public class MaskPolygon : CropPolygon
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaskPolygon"/> class.
        /// </summary>
        public MaskPolygon()
            : base(false)
        {
        }
    }
}
