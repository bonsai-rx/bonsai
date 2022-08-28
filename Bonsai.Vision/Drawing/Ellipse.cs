using OpenCV.Net;
using System;
using System.ComponentModel;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies drawing an ellipse outline, filled ellipse,
    /// elliptic arc, or filled elliptic sector.
    /// </summary>
    [Description("Draws an ellipse outline, filled ellipse, elliptic arc, or filled elliptic sector.")]
    public class Ellipse : CanvasElement
    {
        /// <summary>
        /// Gets or sets the center of the ellipse.
        /// </summary>
        [Description("The center of the ellipse.")]
        public Point Center { get; set; }

        /// <summary>
        /// Gets or sets the length of the ellipse axes.
        /// </summary>
        [Description("The length of the ellipse axes.")]
        public Size Axes { get; set; }

        /// <summary>
        /// Gets or sets the rotation angle of the ellipse, in degrees.
        /// </summary>
        [Range(0, 360)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The rotation angle of the ellipse, in degrees.")]
        public double Angle { get; set; }

        /// <summary>
        /// Gets or sets the starting angle of the elliptic arc, in degrees.
        /// </summary>
        [Range(0, 360)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The starting angle of the elliptic arc, in degrees.")]
        public double StartAngle { get; set; }

        /// <summary>
        /// Gets or sets the ending angle of the elliptic arc, in degrees.
        /// </summary>
        [Range(0, 360)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The ending angle of the elliptic arc, in degrees.")]
        public double EndAngle { get; set; } = 360;

        /// <summary>
        /// Gets or sets the color of the ellipse.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the ellipse.")]
        public Scalar Color { get; set; } = Scalar.All(255);

        /// <summary>
        /// Gets or sets the thickness of the ellipse boundary, if positive.
        /// Otherwise, indicates that the ellipse should be filled.
        /// </summary>
        [Description("The thickness of the ellipse boundary, if positive. Otherwise, indicates that the ellipse should be filled.")]
        public int Thickness { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the line drawing algorithm used to
        /// draw the ellipse boundary.
        /// </summary>
        [Description("Specifies the line drawing algorithm used to draw the ellipse boundary.")]
        public LineFlags LineType { get; set; } = LineFlags.Connected8;

        /// <summary>
        /// Gets or sets the number of fractional bits in the center coordinates and axes' values.
        /// </summary>
        [Description("The number of fractional bits in the center coordinates and axes' values.")]
        public int Shift { get; set; }

        /// <summary>
        /// Returns the ellipse drawing operation.
        /// </summary>
        /// <inheritdoc/>
        protected override Action<IplImage> GetRenderer()
        {
            var center = Center;
            var axes = Axes;
            var angle = Angle;
            var startAngle = StartAngle;
            var endAngle = EndAngle;
            var color = Color;
            var thickness = Thickness;
            var lineType = LineType;
            var shift = Shift;
            return image =>
            {
                CV.Ellipse(image, center, axes, angle, startAngle, endAngle, color, thickness, lineType, shift);
            };
        }
    }
}
