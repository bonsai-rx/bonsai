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
    [Description("Creates a 4x4 matrix containing 3D rotation, scale, position and projection.")]
    public class CreateMatrix4 : Source<Matrix4>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector4 Row0 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The second row of the matrix.")]
        public Vector4 Row1 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The third row of the matrix.")]
        public Vector4 Row2 { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector4 Row3 { get; set; }

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix4(Row0, Row1, Row2, Row3)));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Matrix4(Row0, Row1, Row2, Row3));
        }
    }
}
