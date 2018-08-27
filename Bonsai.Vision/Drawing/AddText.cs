using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    [Description("Renders text strokes with the specified font and color at a given location.")]
    public class AddText : CanvasElement
    {
        [Description("The text to draw.")]
        public string Text { get; set; }

        [Description("The coordinates of the bottom-left corner of the first letter.")]
        public Point Origin { get; set; }

        [XmlIgnore]
        [Description("The font style used to render the text strokes.")]
        public Font Font { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("The color of the text.")]
        public Scalar Color { get; set; }

        protected override void Draw(IplImage image)
        {
            var text = Text;
            if (!string.IsNullOrEmpty(text))
            {
                CV.PutText(image, text, Origin, Font, Color);
            }
        }
    }
}
