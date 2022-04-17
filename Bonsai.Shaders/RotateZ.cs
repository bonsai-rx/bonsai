using OpenTK;
using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that applies a rotation about the z-axis to every
    /// transform in the sequence.
    /// </summary>
    [Description("Applies a rotation about the z-axis to every transform in the sequence.")]
    public class RotateZ : MatrixTransform
    {
        /// <summary>
        /// Gets or sets the angle describing the magnitude of the rotation
        /// about the z-axis.
        /// </summary>
        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle describing the magnitude of the rotation about the z-axis.")]
        public float Angle { get; set; }

        /// <summary>
        /// Initializes a transform matrix for a rotation about the z-axis.
        /// </summary>
        /// <inheritdoc/>
        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateRotationZ(Angle, out result);
        }
    }
}
