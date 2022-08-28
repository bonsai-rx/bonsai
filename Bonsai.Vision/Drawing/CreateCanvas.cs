using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that creates an empty canvas using the specified
    /// size and pixel format.
    /// </summary>
    [Description("Creates an empty canvas using the specified size and pixel format.")]
    public class CreateCanvas : Source<Canvas>
    {
        /// <summary>
        /// Gets or sets the size of the canvas.
        /// </summary>
        [Description("The size of the canvas.")]
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the bit depth of individual pixels in the canvas.
        /// </summary>
        [Description("The bit depth of individual pixels in the canvas.")]
        public IplDepth Depth { get; set; } = IplDepth.U8;

        /// <summary>
        /// Gets or sets the number of channels in the canvas.
        /// </summary>
        [Description("The number of channels in the canvas.")]
        public int Channels { get; set; } = 3;

        /// <summary>
        /// Gets or sets the background color used to initialize all pixels in the canvas.
        /// If not specified, the bitmap memory will be allocated when rendering the canvas,
        /// but will not be initialized.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [TypeConverter(typeof(BgraScalarConverter))]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The optional background color used to initialize all pixels in the canvas.")]
        public Scalar? Color { get; set; } = Scalar.All(0);

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

        /// <summary>
        /// Generates an observable sequence that contains a single empty canvas
        /// with the specified size and pixel format.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="Canvas"/>
        /// class representing an empty canvas with no drawing operators.
        /// </returns>
        public override IObservable<Canvas> Generate()
        {
            return Observable.Create<Canvas>(observer =>
            {
                var canvas = Create(observer);
                return Observable.Return(canvas).SubscribeSafe(observer);
            });
        }

        /// <summary>
        /// Generates an observable sequence of canvas objects using the
        /// specified size and pixel format, and where each canvas is
        /// emitted only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new canvas
        /// objects.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Canvas"/> objects where each element
        /// represents an empty canvas with no drawing operators.
        /// </returns>
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
