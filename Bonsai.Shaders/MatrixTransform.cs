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
    public abstract class MatrixTransform : Transform<Matrix4, Matrix4>
    {
        [Description("The order of relative matrix transform operations.")]
        public MatrixOrder Order { get; set; }

        protected abstract void CreateTransform(out Matrix4 result);

        public override IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return source.Select(input =>
            {
                Matrix4 result;
                CreateTransform(out result);
                if (Order == MatrixOrder.Append) Matrix4.Mult(ref input, ref result, out result);
                else Matrix4.Mult(ref result, ref input, out result);
                return result;
            });
        }
    }

    public enum MatrixOrder
    {
        Append,
        Prepend
    }
}
