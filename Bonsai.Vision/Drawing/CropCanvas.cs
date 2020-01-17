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
    [Description("Crops the active drawing subregion of the input canvas.")]
    public class CropCanvas : Transform<Canvas, Canvas>
    {
        [Description("The region of interest inside the input canvas.")]
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Rect RegionOfInterest { get; set; }

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
