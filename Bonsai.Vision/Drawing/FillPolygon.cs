using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    [Description("Fills an area bounded by several polygonal contours.")]
    public class FillPolygon : CanvasElement
    {
        public FillPolygon()
        {
            Color = Scalar.All(255);
            LineType = LineFlags.Connected8;
            Shift = 0;
        }

        [XmlIgnore]
        [Description("The array of vertices specifying each polygon boundary.")]
        public Point[][] Points { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the filled area.")]
        public Scalar Color { get; set; }

        [Description("The algorithm used to draw the polygon boundaries.")]
        public LineFlags LineType { get; set; }

        [Description("The number of fractional bits in the vertex coordinates.")]
        public int Shift { get; set; }

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
