using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class CreateTranslation : Source<Matrix4>
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateTranslation(X, Y, Z)));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => Matrix4.CreateTranslation(X, Y, Z));
        }
    }
}
