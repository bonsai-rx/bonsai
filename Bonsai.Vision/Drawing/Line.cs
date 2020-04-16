using OpenCV.Net;
using System;
using System.ComponentModel;

namespace Bonsai.Vision.Drawing
{
    [Description("Draws a line segment connecting two points.")]
    public class Line : CanvasElement
    {
        public Line()
        {
            Thickness = 1;
            Color = Scalar.All(255);
            LineType = LineFlags.Connected8;
            Shift = 0;
        }

        [Description("The first point of the line segment.")]
        public Point Start { get; set; }

        [Description("The second point of the line segment.")]
        public Point End { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the line.")]
        public Scalar Color { get; set; }

        [Description("The thickness of the line.")]
        public int Thickness { get; set; }

        [Description("The algorithm used to draw the line.")]
        public LineFlags LineType { get; set; }

        [Description("The number of fractional bits in the point coordinates.")]
        public int Shift { get; set; }

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
