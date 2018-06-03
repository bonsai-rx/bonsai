using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Shaders
{
    [Description("Creates a 3x3 matrix containing 3D rotation and scale.")]
    public class CreateMatrix3 : Source<Matrix3>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector3 Row0 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The second row of the matrix.")]
        public Vector3 Row1 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector3 Row2 { get; set; }

        public override IObservable<Matrix3> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix3(Row0, Row1, Row2)));
        }

        public IObservable<Matrix3> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Matrix3(Row0, Row1, Row2));
        }
    }
}
