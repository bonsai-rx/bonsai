using OpenTK;
using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that applies a rotation about the x-axis to every
    /// transform in the sequence.
    /// </summary>
    [Description("Applies a rotation about the x-axis to every transform in the sequence.")]
    public class RotateX : MatrixTransform
    {
        /// <summary>
        /// Gets or sets the angle describing the magnitude of the rotation
        /// about the x-axis.
        /// </summary>
        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle describing the magnitude of the rotation about the x-axis.")]
        public float Angle { get; set; }

        /// <summary>
        /// Initializes a transform matrix for a rotation about the x-axis.
        /// </summary>
        /// <inheritdoc/>
        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateRotationX(Angle, out result);
        }
    }
}
