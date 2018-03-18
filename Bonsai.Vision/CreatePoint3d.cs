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
    [Description("Creates a double-precision 3D point value.")]
    public class CreatePoint3d : Source<Point3d>
    {
        [Description("The x-component of the point.")]
        public double X { get; set; }

        [Description("The y-component of the point.")]
        public double Y { get; set; }

        [Description("The z-component of the point.")]
        public double Z { get; set; }

        public override IObservable<Point3d> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Point3d(X, Y, Z)));
        }

        public IObservable<Point3d> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Point3d(X, Y, Z));
        }
    }
}
