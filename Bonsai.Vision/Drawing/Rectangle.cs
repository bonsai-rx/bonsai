using OpenCV.Net;
using System;
using System.ComponentModel;

namespace Bonsai.Vision.Drawing
{
    [Description("Draws a simple, thick, or filled rectangle with the specified origin and size.")]
    public class Rectangle : CanvasElement
    {
        public Rectangle()
        {
            Thickness = 1;
            Color = Scalar.All(255);
            LineType = LineFlags.Connected8;
            Shift = 0;
        }

        [Description("The coordinates of the top-left corner of the rectangle.")]
        public Point Origin { get; set; }

        [Description("The size of the rectangle.")]
        public Size Size { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the rectangle.")]
        public Scalar Color { get; set; }

        [Description("The thickness of the rectangle outline, if positive. Otherwise, indicates that the rectangle should be filled.")]
        public int Thickness { get; set; }

        [Description("The algorithm used to draw the rectangle outline.")]
        public LineFlags LineType { get; set; }

        [Description("The number of fractional bits in the rectangle coordinates.")]
        public int Shift { get; set; }

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
