using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Creates a 2x4 matrix.")]
    public class CreateMatrix2x4 : Source<Matrix2x4>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector4 Row0 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector4 Row1 { get; set; }

        public override IObservable<Matrix2x4> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix2x4(Row0, Row1)));
        }

        public IObservable<Matrix2x4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Matrix2x4(Row0, Row1));
        }
    }
}
