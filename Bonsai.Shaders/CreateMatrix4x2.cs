using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Creates a 4x2 matrix.")]
    public class CreateMatrix4x2 : Source<Matrix4x2>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector2 Row0 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The second row of the matrix.")]
        public Vector2 Row1 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The third row of the matrix.")]
        public Vector2 Row2 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector2 Row3 { get; set; }

        public override IObservable<Matrix4x2> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix4x2(Row0, Row1, Row2, Row3)));
        }

        public IObservable<Matrix4x2> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Matrix4x2(Row0, Row1, Row2, Row3));
        }
    }
}
