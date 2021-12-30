using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Creates a 2x3 matrix.")]
    public class CreateMatrix2x3 : Source<Matrix2x3>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector3 Row0 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector3 Row1 { get; set; }

        public override IObservable<Matrix2x3> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix2x3(Row0, Row1)));
        }

        public IObservable<Matrix2x3> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Matrix2x3(Row0, Row1));
        }
    }
}
