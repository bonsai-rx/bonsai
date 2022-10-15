using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies drawing contour outlines or filled
    /// interiors in an image.
    /// </summary>
    [ResetCombinator]
    [Description("Draws contour outlines or filled interiors in an image.")]
    public class DrawContours : CanvasElement
    {
        /// <summary>
        /// Gets or sets the first contour to draw.
        /// </summary>
        [XmlIgnore]
        [Description("The first contour to draw.")]
        public Seq Contour { get; set; }

        /// <summary>
        /// Gets or sets the color of the external contours.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the external contours.")]
        public Scalar ExternalColor { get; set; } = Scalar.All(255);

        /// <summary>
        /// Gets or sets the color of the internal holes.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the internal holes.")]
        public Scalar HoleColor { get; set; }

        /// <summary>
        /// Gets or sets the maximum level of the contour hierarchy to draw.
        /// </summary>
        [Description("The maximum level of the contour hierarchy to draw.")]
        public int MaxLevel { get; set; }

        /// <summary>
        /// Gets or sets the thickness of the contour lines, if positive.
        /// Otherwise, the contour interiors will be drawn.
        /// </summary>
        [Description("The thickness of the contour lines, if positive. Otherwise, the contour interiors will be drawn.")]
        public int Thickness { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value specifying the line drawing algorithm used to
        /// draw the contour boundaries.
        /// </summary>
        [Description("Specifies the line drawing algorithm used to draw the contour boundaries.")]
        public LineFlags LineType { get; set; } = LineFlags.Connected8;

        /// <summary>
        /// Returns the contour drawing operation.
        /// </summary>
        /// <inheritdoc/>
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
