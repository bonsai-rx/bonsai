using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Shaders
{
    [Description("Creates a 2x2 matrix.")]
    public class CreateMatrix2 : Source<Matrix2>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector2 Row0 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector2 Row1 { get; set; }

        public override IObservable<Matrix2> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix2(Row0, Row1)));
        }

        public IObservable<Matrix2> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Matrix2(Row0, Row1));
        }
    }
}
