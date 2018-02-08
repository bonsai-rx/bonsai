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
    [Description("Returns the scale component of the input matrix.")]
    public class ExtractScale : Transform<Matrix4, Vector3>
    {
        public override IObservable<Vector3> Process(IObservable<Matrix4> source)
        {
            return source.Select(input => input.ExtractScale());
        }
    }
}
