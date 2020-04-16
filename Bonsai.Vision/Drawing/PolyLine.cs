using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    [Description("Draws one or more polygonal curves.")]
    public class PolyLine : CanvasElement
    {
        public PolyLine()
        {
            Thickness = 1;
            Color = Scalar.All(255);
            LineType = LineFlags.Connected8;
            Shift = 0;
        }

        [XmlIgnore]
        [Description("The array of vertices specifying each polyline.")]
        public Point[][] Points { get; set; }

        [Description("Indicates whether the polylines should be closed. If closed, a line is drawn between the first and last vertex of every contour.")]
        public bool Closed { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the polylines.")]
        public Scalar Color { get; set; }

        [Description("The thickness of the polyline edges.")]
        public int Thickness { get; set; }

        [Description("The algorithm used to draw the polylines.")]
        public LineFlags LineType { get; set; }

        [Description("The number of fractional bits in the vertex coordinates.")]
        public int Shift { get; set; }

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
