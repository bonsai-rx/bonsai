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
    [Description("Creates a rotation matrix from an axis-angle representation.")]
    public class CreateAxisAngleRotation : Source<Matrix4>
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The vector specifying the direction of the axis of rotation.")]
        public Vector3 Axis { get; set; }

        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle describing the magnitude of the rotation about the axis.")]
        public float Angle { get; set; }

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateFromAxisAngle(Axis, Angle)));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => Matrix4.CreateFromAxisAngle(Axis, Angle));
        }
    }
}
