using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Applies a rotation around an arbitrary axis.")]
    public class RotateAxisAngle : MatrixTransform
    {
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The vector specifying the direction of the axis of rotation.")]
        public Vector3 Axis { get; set; }

        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle describing the magnitude of the rotation about the axis.")]
        public float Angle { get; set; }

        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateFromAxisAngle(Axis, Angle, out result);
        }
    }
}
