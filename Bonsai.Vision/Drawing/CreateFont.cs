using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision.Drawing
{
    [Description("Creates a font that can be passed to text rendering functions.")]
    public class CreateFont : Source<Font>
    {
        public CreateFont()
        {
            Scale = 1;
            Thickness = 1;
            LineType = LineFlags.Connected8;
        }

        [Description("The built-in font face used to create this font object.")]
        public FontFace FontFace { get; set; }

        [Description("The scale factor for the font.")]
        public double Scale { get; set; }

        [Description("The thickness of the text strokes.")]
        public int Thickness { get; set; }

        [Description("The algorithm used to draw the text strokes.")]
        public LineFlags LineType { get; set; }

        private Font Create()
        {
            return new Font(FontFace, Scale, Scale, 0, Thickness, LineType);
        }

        public override IObservable<Font> Generate()
        {
            return Observable.Defer(() => Observable.Return(Create()));
        }

        public IObservable<Font> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(draw => Create());
        }
    }
}
