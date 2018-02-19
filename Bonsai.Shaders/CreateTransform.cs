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
    [Description("Creates a model matrix specifying position, rotation and scale.")]
    public class CreateTransform : Source<Matrix4>
    {
        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        public CreateTransform()
        {
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The position of the model, in the local coordinate frame.")]
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The quaternion representing the rotation of the model, in the local coordinate frame.")]
        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The scale vector applied to the model, in the local coordinate frame.")]
        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        Matrix4 GetTransform()
        {
            Matrix4 result, temp;
            Matrix4.CreateScale(ref scale, out result);
            Matrix4.CreateFromQuaternion(ref rotation, out temp);
            Matrix4.Mult(ref result, ref temp, out result);
            Matrix4.CreateTranslation(ref position, out temp);
            Matrix4.Mult(ref result, ref temp, out result);
            return result;
        }

        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer((() => Observable.Return(GetTransform())));
        }

        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => GetTransform());
        }
    }
}
