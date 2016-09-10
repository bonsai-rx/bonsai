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
    [Description("Creates a 2D vector element.")]
    public class CreateVector2 : Source<Vector2>
    {
        [Description("The x-component of the vector.")]
        public float X { get; set; }

        [Description("The y-component of the vector.")]
        public float Y { get; set; }

        public override IObservable<Vector2> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Vector2(X, Y)));
        }

        public IObservable<Vector2> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Vector2(X, Y));
        }
    }
}
