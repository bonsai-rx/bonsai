using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies filling an area bounded by several
    /// polygonal contours.
    /// </summary>
    [Description("Fills an area bounded by several polygonal contours.")]
    public class FillPolygon : CanvasElement
    {
        /// <summary>
        /// Gets or sets the array of vertices specifying each polygon boundary.
        /// </summary>
        [XmlIgnore]
        [Description("The array of vertices specifying each polygon boundary.")]
        public Point[][] Points { get; set; }

        /// <summary>
        /// Gets or sets the color of the filled area.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the filled area.")]
        public Scalar Color { get; set; } = Scalar.All(255);

        /// <summary>
        /// Gets or sets a value specifying the line drawing algorithm used to
        /// draw the polygon boundaries.
        /// </summary>
        [Description("Specifies the line drawing algorithm used to draw the polygon boundaries.")]
        public LineFlags LineType { get; set; } = LineFlags.Connected8;

        /// <summary>
        /// Gets or sets the number of fractional bits in the vertex coordinates.
        /// </summary>
        [Description("The number of fractional bits in the vertex coordinates.")]
        public int Shift { get; set; }

        /// <summary>
        /// Returns the polygon filling operation.
        /// </summary>
        /// <inheritdoc/>
        protected override Action<IplImage> GetRenderer()
        {
            var points = Points;
            var color = Color;
            var lineType = LineType;
            var shift = Shift;
            return image =>
            {
                if (points != null && points.Length > 0)
                {
                    CV.FillPoly(image, points, color, lineType, shift);
                }
            };
        }
    }
}
