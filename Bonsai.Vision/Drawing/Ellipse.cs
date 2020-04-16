using OpenCV.Net;
using System;
using System.ComponentModel;

namespace Bonsai.Vision.Drawing
{
    [Description("Draws ellipse outline, filled ellipse, elliptic arc, or filled elliptic sector.")]
    public class Ellipse : CanvasElement
    {
        public Ellipse()
        {
            EndAngle = 360;
            Thickness = 1;
            Color = Scalar.All(255);
            LineType = LineFlags.Connected8;
            Shift = 0;
        }

        [Description("The center of the ellipse.")]
        public Point Center { get; set; }

        [Description("The length of the ellipse axes.")]
        public Size Axes { get; set; }

        [Range(0, 360)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The rotation angle of the ellipse, in degrees.")]
        public double Angle { get; set; }

        [Range(0, 360)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The starting angle of the elliptic arc, in degrees.")]
        public double StartAngle { get; set; }

        [Range(0, 360)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The ending angle of the elliptic arc, in degrees.")]
        public double EndAngle { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the ellipse.")]
        public Scalar Color { get; set; }

        [Description("The thickness of the ellipse boundary, if positive. Otherwise, indicates that the ellipse should be filled.")]
        public int Thickness { get; set; }

        [Description("The algorithm used to draw the ellipse boundary.")]
        public LineFlags LineType { get; set; }

        [Description("The number of fractional bits in the center coordinates and axes' values.")]
        public int Shift { get; set; }

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
