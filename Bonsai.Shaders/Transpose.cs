using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Transposes the input matrix.")]
    public class Transpose : Transform<Matrix4, Matrix4>
    {
        public IObservable<Matrix3> Process(IObservable<Matrix3> source)
        {
            return source.Select(Matrix3.Transpose);
        }

        public override IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return source.Select(Matrix4.Transpose);
        }
    }
}
