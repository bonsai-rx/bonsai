using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies drawing one or more polygonal curves.
    /// </summary>
    [Description("Draws one or more polygonal curves.")]
    public class PolyLine : CanvasElement
    {
        /// <summary>
        /// Gets or sets the array of vertices specifying each polyline.
        /// </summary>
        [XmlIgnore]
        [Description("The array of vertices specifying each polyline.")]
        public Point[][] Points { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the polylines should be closed.
        /// If closed, a line is drawn between the first and last vertex of every contour.
        /// </summary>
        [Description("Indicates whether the polylines should be closed. If closed, a line is drawn between the first and last vertex of every contour.")]
        public bool Closed { get; set; }

        /// <summary>
        /// Gets or sets the color of the polylines.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the polylines.")]
        public Scalar Color { get; set; } = Scalar.All(255);

        /// <summary>
        /// Gets or sets the thickness of the polyline edges.
        /// </summary>
        [Description("The thickness of the polyline edges.")]
        public int Thickness { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the line drawing algorithm used to
        /// draw the polylines.
        /// </summary>
        [Description("Specifies the line drawing algorithm used to draw the polylines.")]
        public LineFlags LineType { get; set; } = LineFlags.Connected8;

        /// <summary>
        /// Gets or sets the number of fractional bits in the vertex coordinates.
        /// </summary>
        [Description("The number of fractional bits in the vertex coordinates.")]
        public int Shift { get; set; }

        /// <summary>
        /// Returns the polyline drawing operation.
        /// </summary>
        /// <inheritdoc/>
        protected override Action<IplImage> GetRenderer()
        {
            var points = Points;
            var closed = Closed;
            var color = Color;
            var thickness = Thickness;
            var lineType = LineType;
            var shift = Shift;
            return image =>
            {
                if (points != null && points.Length > 0)
                {
                    CV.PolyLine(image, points, closed, color, thickness, lineType, shift);
                }
            };
        }
    }
}
