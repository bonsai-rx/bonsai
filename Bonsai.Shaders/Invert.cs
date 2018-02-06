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
    [Description("Converts the input matrix into its inverse.")]
    public class Invert : Transform<Matrix4, Matrix4>
    {
        public IObservable<Matrix3> Process(IObservable<Matrix3> source)
        {
            return source.Select(input => input.Inverted());
        }

        public override IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return source.Select(input => input.Inverted());
        }
    }
}
