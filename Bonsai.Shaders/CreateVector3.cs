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
    [Description("Creates a 3D vector element.")]
    public class CreateVector3 : Source<Vector3>
    {
        [Description("The x-component of the vector.")]
        public float X { get; set; }

        [Description("The y-component of the vector.")]
        public float Y { get; set; }

        [Description("The z-component of the vector.")]
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
