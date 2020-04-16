using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Xml.Serialization;
using Font = System.Drawing.Font;

namespace Bonsai.Vision.Drawing
{
    public abstract class AddTextBase : CanvasElement
    {
        internal AddTextBase()
        {
            TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            Font = SystemFonts.DefaultFont;
            Color = Scalar.All(255);
        }

        [Description("The text to draw.")]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public string Text { get; set; }

        [XmlIgnore]
        [Description("The font style used to render the text strokes.")]
        public Font Font { get; set; }

        [Category("Text Style")]
        [Description("The horizontal alignment of the text.")]
        public StringAlignment Alignment { get; set; }

        [Category("Text Style")]
        [Description("The vertical alignment of the text.")]
        public StringAlignment LineAlignment { get; set; }

        [Category("Text Style")]
        [Description("The rendering mode used for the text strokes.")]
        public TextRenderingHint TextRenderingHint { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color of the text.")]
        public Scalar Color { get; set; }

        [Browsable(false)]
        [XmlElement("Font")]
        public string FontXml
        {
            get
            {
                var font = Font;
                if (font == null || font == SystemFonts.DefaultFont) return null;
                var converter = new FontConverter();
                return converter.ConvertToString(Font);
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var converter = new FontConverter();
                    Font = (Font)converter.ConvertFromString(value);
                }
                else Font = SystemFonts.DefaultFont;
            }
        }

        static PixelFormat GetImageFormat(IplImage image)
        {
            if (image.Depth != IplDepth.U8)
            {
                throw new ArgumentException("Unsupported image bit depth. Only unsigned byte images are supported.");
            }

            switch (image.Channels)
            {
                case 3: return PixelFormat.Format24bppRgb;
                case 4: return PixelFormat.Format32bppArgb;
                default: throw new ArgumentException("Unsupported number of image channels. Only color images are supported.");
            }
        }

        internal Action<IplImage> GetRenderer<TState>(
            TState state,
            Action<IplImage, Graphics, string, Font, Brush, StringFormat, TState> renderer)
        {
            var text = Text;
            var font = Font;
            var color = Color;
            var alignment = Alignment;
            var lineAlignment = LineAlignment;
            var textRenderingHint = TextRenderingHint;
            return image =>
            {
                var pixelFormat = GetImageFormat(image);
                using (var bitmap = new Bitmap(image.Width, image.Height, image.WidthStep, pixelFormat, image.ImageData))
                using (var brush = new SolidBrush(System.Drawing.Color.FromArgb((int)color.Val3, (int)color.Val2, (int)color.Val1, (int)color.Val0)))
                using (var graphics = Graphics.FromImage(bitmap))
                using (var format = new StringFormat())
                {
                    format.Alignment = alignment;
                    format.LineAlignment = lineAlignment;
                    graphics.TextRenderingHint = textRenderingHint;
                    renderer(image, graphics, text, font, brush, format, state);
                }
            };
        }
    }
}
