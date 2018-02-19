using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Applies a relative model matrix transform specifying position, rotation and scale.")]
    public class TransformRelative : MatrixTransform
    {
        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        public TransformRelative()
        {
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The relative position of the model, in the local coordinate frame.")]
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The quaternion representing the relative rotation of the model, in the local coordinate frame.")]
        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The relative scale vector applied to the model, in the local coordinate frame.")]
        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4 temp;
            Matrix4.CreateScale(ref scale, out result);
            Matrix4.CreateFromQuaternion(ref rotation, out temp);
            Matrix4.Mult(ref result, ref temp, out result);
            Matrix4.CreateTranslation(ref position, out temp);
            Matrix4.Mult(ref result, ref temp, out result);
        }
    }
}
