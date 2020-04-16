using OpenTK;
using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    [Description("Applies a rotation around the z-axis.")]
    public class RotateZ : MatrixTransform
    {
        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle describing the magnitude of the rotation about the z-axis.")]
        public float Angle { get; set; }

        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateRotationZ(Angle, out result);
        }
    }
}
