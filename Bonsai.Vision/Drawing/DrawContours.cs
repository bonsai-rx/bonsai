using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    [Description("Draws contour outlines or filled interiors in an image.")]
    public class DrawContours : CanvasElement
    {
        public DrawContours()
        {
            Thickness = 1;
            ExternalColor = Scalar.All(255);
            LineType = LineFlags.Connected8;
        }

        [XmlIgnore]
        [Description("The first contour to draw.")]
        public Seq Contour { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the external contours.")]
        public Scalar ExternalColor { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the internal holes.")]
        public Scalar HoleColor { get; set; }

        [Description("The maximum level of the contour hierarchy to draw.")]
        public int MaxLevel { get; set; }

        [Description("The thickness of the contour lines, if positive. Otherwise, the contour interiors will be drawn.")]
        public int Thickness { get; set; }

        [Description("The algorithm used to draw the contour boundaries.")]
        public LineFlags LineType { get; set; }

        protected override Action<IplImage> GetRenderer()
        {
            var contour = Contour;
            var externalColor = ExternalColor;
            var holeColor = HoleColor;
            var maxLevel = MaxLevel;
            var thickness = Thickness;
            var lineType = LineType;
            return image =>
            {
                if (contour != null)
                {
                    CV.DrawContours(image, contour, externalColor, holeColor, maxLevel, thickness, lineType);
                }
            };
        }
    }
}
