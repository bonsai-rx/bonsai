using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision.Drawing
{
    [Description("Draws a circle with the specified center and radius.")]
    public class Circle : CanvasElement
    {
        public Circle()
        {
            Thickness = 1;
            Color = Scalar.All(255);
            LineType = LineFlags.Connected8;
            Shift = 0;
        }

        [Description("The center of the circle.")]
        public Point Center { get; set; }

        [Description("The radius of the circle.")]
        public int Radius { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the circle.")]
        public Scalar Color { get; set; }

        [Description("The thickness of the circle boundary, if positive. Otherwise, indicates that the circle should be filled.")]
        public int Thickness { get; set; }

        [Description("The algorithm used to draw the circle boundary.")]
        public LineFlags LineType { get; set; }

        [Description("The number of fractional bits in the center coordinates and radius value.")]
        public int Shift { get; set; }

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
