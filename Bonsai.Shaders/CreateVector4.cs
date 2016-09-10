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
    [Description("Creates a 4D vector element.")]
    public class CreateVector4 : Source<Vector4>
    {
        [Description("The x-component of the vector.")]
        public float X { get; set; }

        [Description("The y-component of the vector.")]
        public float Y { get; set; }

        [Description("The z-component of the vector.")]
        public float Z { get; set; }

        [Description("The w-component of the vector.")]
        public float W { get; set; }

        public override IObservable<Vector4> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Vector4(X, Y, Z, W)));
        }

        public IObservable<Vector4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Vector4(X, Y, Z, W));
        }
    }
}
