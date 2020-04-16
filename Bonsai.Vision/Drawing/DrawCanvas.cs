using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision.Drawing
{
    [Description("Renders all the drawing elements in the input canvas as an image.")]
    public class DrawCanvas : Transform<Canvas, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<Canvas> source)
        {
            return source.Select(canvas => canvas.Draw());
        }
    }
}
