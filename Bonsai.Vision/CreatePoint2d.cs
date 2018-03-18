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
    [Description("Creates a double-precision 2D point value.")]
    public class CreatePoint2d : Source<Point2d>
    {
        [Description("The x-component of the point.")]
        public double X { get; set; }

        [Description("The y-component of the point.")]
        public double Y { get; set; }

        public override IObservable<Point2d> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Point2d(X, Y)));
        }

        public IObservable<Point2d> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Point2d(X, Y));
        }
    }
}
