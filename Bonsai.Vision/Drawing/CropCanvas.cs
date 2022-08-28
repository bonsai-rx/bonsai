using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that crops the active drawing subregion of each
    /// canvas in the sequence.
    /// </summary>
    [Description("Crops the active drawing subregion of each canvas in the sequence.")]
    public class CropCanvas : Transform<Canvas, Canvas>
    {
        /// <summary>
        /// Gets or sets a rectangle specifying the region of interest inside the canvas.
        /// </summary>
        [Description("Specifies the region of interest inside the canvas.")]
        [Editor("Bonsai.Vision.Design.IplImageRectangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Rect RegionOfInterest { get; set; }

        /// <summary>
        /// Crops the active drawing subregion of each canvas in an observable
        /// sequence.
        /// </summary>
        /// <param name="source">A sequence of canvas objects.</param>
        /// <returns>
        /// A sequence of <see cref="Canvas"/> objects where each new canvas
        /// will use the specified subregion of the original image for all its
        /// drawing operations.
        /// </returns>
        public override IObservable<Canvas> Process(IObservable<Canvas> source)
        {
            return Observable.Create<Canvas>(observer =>
            {
                return source.Select(input =>
                {
                    var rect = RegionOfInterest;
                    var subCanvas = new SubCanvas(image =>
                    {
                        try
                        {
                            if (rect.Width > 0 && rect.Height > 0)
                            {
                                return image.GetSubRect(rect);
                            }

                            return image;
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            throw;
                        }
                    });

                    DrawingCall draw;
                    draw.Action = subCanvas.Draw;
                    draw.Observer = observer;
                    return new Canvas(input, draw);
                }).SubscribeSafe(observer);
            });
        }
    }
}
