using OpenCV.Net;
using System;
using System.ComponentModel;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies drawing a line segment connecting
    /// two points.
    /// </summary>
    [Description("Draws a line segment connecting two points.")]
    public class Line : CanvasElement
    {
        /// <summary>
        /// Gets or sets the first point of the line segment.
        /// </summary>
        [Description("The first point of the line segment.")]
        public Point Start { get; set; }

        /// <summary>
        /// Gets or sets the second point of the line segment.
        /// </summary>
        [Description("The second point of the line segment.")]
        public Point End { get; set; }

        /// <summary>
        /// Gets or sets the color of the line.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the line.")]
        public Scalar Color { get; set; } = Scalar.All(255);

        /// <summary>
        /// Gets or sets the thickness of the line.
        /// </summary>
        [Description("The thickness of the line.")]
        public int Thickness { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the line drawing algorithm used to
        /// draw the line.
        /// </summary>
        [Description("Specifies the line drawing algorithm used to draw the line.")]
        public LineFlags LineType { get; set; } = LineFlags.Connected8;

        /// <summary>
        /// Gets or sets the number of fractional bits in the point coordinates.
        /// </summary>
        [Description("The number of fractional bits in the point coordinates.")]
        public int Shift { get; set; }

        /// <summary>
        /// Returns the line drawing operation.
        /// </summary>
        /// <inheritdoc/>
        protected override Action<IplImage> GetRenderer()
        {
            var start = Start;
            var end = End;
            var color = Color;
            var thickness = Thickness;
            var lineType = LineType;
            var shift = Shift;
            return image =>
            {
                CV.Line(image, start, end, color, thickness, lineType, shift);
            };
        }
    }
}
