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
    [Description("Creates an empty canvas using the specified size and pixel format.")]
    public class CreateCanvas : Source<Canvas>
    {
        public CreateCanvas()
        {
            Color = Scalar.All(0);
            Depth = IplDepth.U8;
            Channels = 3;
        }

        [Description("The size of the output canvas.")]
        public Size Size { get; set; }

        [Description("The target bit depth of individual pixels.")]
        public IplDepth Depth { get; set; }

        [Description("The number of channels in the output canvas.")]
        public int Channels { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The optional background color used to initialize all pixels in the canvas.")]
        public Scalar? Color { get; set; }

        private Canvas Create(IObserver<Canvas> observer)
        {
            var color = Color;
            var size = Size;
            var depth = Depth;
            var channels = Channels;
            return new Canvas(() =>
            {
                try
                {
                    var output = new IplImage(size, depth, channels);
                    if (color.HasValue) output.Set(color.Value);
                    return output;
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                    throw;
                }
            });
        }

        public override IObservable<Canvas> Generate()
        {
            return Observable.Create<Canvas>(observer =>
            {
                var canvas = Create(observer);
                return Observable.Return(canvas).SubscribeSafe(observer);
            });
        }

        public IObservable<Canvas> Generate<TSource>(IObservable<TSource> source)
        {
            return Observable.Create<Canvas>(observer =>
            {
                return source.Select(input => Create(observer))
                             .SubscribeSafe(observer);
            });
        }
    }
}
