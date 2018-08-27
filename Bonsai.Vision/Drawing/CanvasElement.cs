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
        protected abstract void Draw(IplImage image);

        public override IObservable<Canvas> Process(IObservable<Canvas> source)
        {
            return Observable.Create<Canvas>(observer =>
            {
                Action<IplImage> draw = image =>
                {
                    try { Draw(image); }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        throw;
                    }
                };
                return source.Select(input => new Canvas(input, draw)).SubscribeSafe(observer);
            });
        }
    }
}
