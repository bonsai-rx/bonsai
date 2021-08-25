using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Computes the normal matrix for the specified input modelview matrix.")]
    public class NormalMatrix : Transform<Matrix4, Matrix4>
    {
        public IObservable<Matrix3> Process(IObservable<Matrix3> source)
        {
            return source.Select(input =>
            {
                if (input.Determinant == 0)
                {
                    return Matrix3.Zero;
                }
                input.Invert();
                input.Transpose();
                return input;
            });
        }

        public override IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return source.Select(input =>
            {
                if (input.Determinant == 0)
                {
                    return Matrix4.Zero;
                }
                input.Invert();
                input.Transpose();
                return input;
            });
        }
    }
}
