using OpenTK;
using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that applies a rotation around an arbitrary axis
    /// to every transform in the sequence.
    /// </summary>
    [Description("Applies a rotation around an arbitrary axis to every transform in the sequence.")]
    public class RotateAxisAngle : MatrixTransform
    {
        /// <summary>
        /// Gets or sets a 3D vector specifying the direction of the axis of rotation.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("Specifies the direction of the axis of rotation.")]
        public Vector3 Axis { get; set; }

        /// <summary>
        /// Gets or sets the angle describing the magnitude of the rotation
        /// about the axis.
        /// </summary>
        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle describing the magnitude of the rotation about the axis.")]
        public float Angle { get; set; }

        /// <summary>
        /// Initializes a transform matrix for applying a rotation around the
        /// specified axis.
        /// </summary>
        /// <inheritdoc/>
        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateFromAxisAngle(Axis, Angle, out result);
        }
    }
}
