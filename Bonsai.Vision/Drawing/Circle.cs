using OpenCV.Net;
using System;
using System.ComponentModel;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies drawing a circle with the specified
    /// center and radius.
    /// </summary>
    [Description("Draws a circle with the specified center and radius.")]
    public class Circle : CanvasElement
    {
        /// <summary>
        /// Gets or sets the center of the circle.
        /// </summary>
        [Description("The center of the circle.")]
        public Point Center { get; set; }

        /// <summary>
        /// Gets or sets the radius of the circle.
        /// </summary>
        [Description("The radius of the circle.")]
        public int Radius { get; set; }

        /// <summary>
        /// Gets or sets the color of the circle.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the circle.")]
        public Scalar Color { get; set; } = Scalar.All(255);

        /// <summary>
        /// Gets or sets the thickness of the circle boundary, if positive.
        /// Otherwise, indicates that the circle should be filled.
        /// </summary>
        [Description("The thickness of the circle boundary, if positive. Otherwise, indicates that the circle should be filled.")]
        public int Thickness { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the line drawing algorithm used to
        /// draw the circle boundary.
        /// </summary>
        [Description("Specifies the line drawing algorithm used to draw the circle boundary.")]
        public LineFlags LineType { get; set; } = LineFlags.Connected8;

        /// <summary>
        /// Gets or sets the number of fractional bits in the center coordinates and radius value.
        /// </summary>
        [Description("The number of fractional bits in the center coordinates and radius value.")]
        public int Shift { get; set; }

        /// <summary>
        /// Returns the circle drawing operation.
        /// </summary>
        /// <inheritdoc/>
        protected override Action<IplImage> GetRenderer()
        {
            var center = Center;
            var radius = Radius;
            var color = Color;
            var thickness = Thickness;
            var lineType = LineType;
            var shift = Shift;
            return image =>
            {
                CV.Circle(image, center, radius, color, thickness, lineType, shift);
            };
        }
    }
}
