using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Creates a 3x4 matrix.")]
    public class CreateMatrix3x4 : Source<Matrix3x4>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector4 Row0 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The second row of the matrix.")]
        public Vector4 Row1 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector4 Row2 { get; set; }

        public override IObservable<Matrix3x4> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix3x4(Row0, Row1, Row2)));
        }

        public IObservable<Matrix3x4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Matrix3x4(Row0, Row1, Row2));
        }
    }
}
