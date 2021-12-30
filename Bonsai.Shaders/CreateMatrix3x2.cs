using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Creates a 3x2 matrix.")]
    public class CreateMatrix3x2 : Source<Matrix3x2>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector2 Row0 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The second row of the matrix.")]
        public Vector2 Row1 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector2 Row2 { get; set; }

        public override IObservable<Matrix3x2> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix3x2(Row0, Row1, Row2)));
        }

        public IObservable<Matrix3x2> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Matrix3x2(Row0, Row1, Row2));
        }
    }
}
