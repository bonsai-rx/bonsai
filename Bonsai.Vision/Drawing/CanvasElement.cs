using OpenCV.Net;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Provides an abstract base class for operators that specify a new drawing
    /// operation to be applied to every canvas in the sequence.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class CanvasElement : Transform<Canvas, Canvas>
    {
        /// <summary>
        /// When overridden in a derived class, returns the drawing operation
        /// to be applied to the canvas bitmap during rendering.
        /// </summary>
        /// <returns>
        /// The <see cref="Action{IplImage}"/> object that will be invoked to apply
        /// the drawing operation to the canvas bitmap during rendering.
        /// </returns>
        protected abstract Action<IplImage> GetRenderer();

        /// <summary>
        /// Specifies a new drawing operation to be applied to every
        /// canvas in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of canvas objects.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Canvas"/> objects, where each instance
        /// represents the result of layering the new drawing operation
        /// on top of all the operations in the canvas.
        /// </returns>
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
