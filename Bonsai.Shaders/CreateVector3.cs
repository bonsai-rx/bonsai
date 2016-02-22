using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class CreateVector3 : Source<Vector3>
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public override IObservable<Vector3> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Vector3(X, Y, Z)));
        }

        public IObservable<Vector3> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Vector3(X, Y, Z));
        }
    }
}
