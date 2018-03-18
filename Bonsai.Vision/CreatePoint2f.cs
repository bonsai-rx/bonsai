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
    [Description("Creates a single-precision 2D point value.")]
    public class CreatePoint2f : Source<Point2f>
    {
        [Description("The x-component of the point.")]
        public float X { get; set; }

        [Description("The y-component of the point.")]
        public float Y { get; set; }

        public override IObservable<Point2f> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Point2f(X, Y)));
        }

        public IObservable<Point2f> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Point2f(X, Y));
        }
    }
}
