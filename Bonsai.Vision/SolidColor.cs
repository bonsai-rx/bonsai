using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Description("Produces a sequence with a single image where all pixels are set to the same color value.")]
    public class SolidColor : Source<IplImage>
    {
        public SolidColor()
        {
            Depth = IplDepth.U8;
            Channels = 3;
        }

        [Description("The size of the output image.")]
        public Size Size { get; set; }

        [Description("The target bit depth of individual image pixels.")]
        public IplDepth Depth { get; set; }

        [Description("The number of channels in the output image.")]
        public int Channels { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The color value to which all pixels in the output image will be set to.")]
        public Scalar Color { get; set; }

        IplImage CreateImage()
        {
            var image = new IplImage(Size, Depth, Channels);
            image.Set(Color);
            return image;
        }

        public override IObservable<IplImage> Generate()
        {
            return Observable.Defer(() => Observable.Return(CreateImage()));
        }

        public IObservable<IplImage> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => CreateImage());
        }
    }
}
