using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision.Drawing
{
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class CanvasElement : Transform<Canvas, Canvas>
    {
        protected abstract Action<IplImage> GetRenderer();

        public override IObservable<Canvas> Process(IObservable<Canvas> source)
        {
            return Observable.Create<Canvas>(observer =>
            {
                return source.Select(input =>
                {
                    DrawingCall draw;
                    draw.Action = GetRenderer();
                    draw.Observer = observer;
                    return new Canvas(input, draw);
                }).SubscribeSafe(observer);
            });
        }
    }
}
