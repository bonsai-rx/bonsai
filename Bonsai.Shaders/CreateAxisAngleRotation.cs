using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class CreateAxisAngleRotation : Source<Matrix4>
    {
        [TypeConverter("OpenCV.Net.NumericAggregateConverter, OpenCV.Net")]
        public Vector3 Axis { get; set; }

        public float Angle { get; set; }

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateFromAxisAngle(Axis, Angle)));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => Matrix4.CreateFromAxisAngle(Axis, Angle));
        }
    }
}
