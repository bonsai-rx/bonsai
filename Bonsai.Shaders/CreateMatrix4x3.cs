using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Creates a 4x3 matrix.")]
    public class CreateMatrix4x3 : Source<Matrix4x3>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector3 Row0 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The second row of the matrix.")]
        public Vector3 Row1 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The third row of the matrix.")]
        public Vector3 Row2 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector3 Row3 { get; set; }

        public override IObservable<Matrix4x3> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix4x3(Row0, Row1, Row2, Row3)));
        }

        public IObservable<Matrix4x3> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Matrix4x3(Row0, Row1, Row2, Row3));
        }
    }
}
