using OpenCV.Net;
using System;
using System.ComponentModel;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies drawing a simple, thick, or filled
    /// rectangle with the specified origin and size.
    /// </summary>
    [Description("Draws a simple, thick, or filled rectangle with the specified origin and size.")]
    public class Rectangle : CanvasElement
    {
        /// <summary>
        /// Gets or sets the coordinates of the top-left corner of the rectangle.
        /// </summary>
        [Description("The coordinates of the top-left corner of the rectangle.")]
        public Point Origin { get; set; }

        /// <summary>
        /// Gets or sets the size of the rectangle.
        /// </summary>
        [Description("The size of the rectangle.")]
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the color of the rectangle.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the rectangle.")]
        public Scalar Color { get; set; } = Scalar.All(255);

        /// <summary>
        /// Gets or sets the thickness of the rectangle outline, if positive.
        /// Otherwise, indicates that the rectangle should be filled.
        /// </summary>
        [Description("The thickness of the rectangle outline, if positive. Otherwise, indicates that the rectangle should be filled.")]
        public int Thickness { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the line drawing algorithm used to
        /// draw the rectangle outline.
        /// </summary>
        [Description("Specifies the line drawing algorithm used to draw the rectangle outline.")]
        public LineFlags LineType { get; set; } = LineFlags.Connected8;

        /// <summary>
        /// Gets or sets the number of fractional bits in the rectangle coordinates.
        /// </summary>
        [Description("The number of fractional bits in the rectangle coordinates.")]
        public int Shift { get; set; }

        /// <summary>
        /// Returns the rectangle drawing operation.
        /// </summary>
        /// <inheritdoc/>
        protected override Action<IplImage> GetRenderer()
        {
            var size = Size;
            var origin = Origin;
            var color = Color;
            var thickness = Thickness;
            var lineType = LineType;
            var shift = Shift;
            return image =>
            {
                CV.Rectangle(image, new Rect(origin.X, origin.Y, size.Width, size.Height), color, thickness, lineType, shift);
            };
        }
    }
}
