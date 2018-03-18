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
    [Description("Creates an integral 2D point value.")]
    public class CreatePoint : Source<Point>
    {
        [Description("The x-component of the point.")]
        public int X { get; set; }

        [Description("The y-component of the point.")]
        public int Y { get; set; }

        public override IObservable<Point> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Point(X, Y)));
        }

        public IObservable<Point> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Point(X, Y));
        }
    }
}
