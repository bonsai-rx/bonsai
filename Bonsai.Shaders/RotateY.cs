using OpenTK;
using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that applies a rotation about the y-axis to every
    /// transform in the sequence.
    /// </summary>
    [Description("Applies a rotation about the y-axis to every transform in the sequence.")]
    public class RotateY : MatrixTransform
    {
        /// <summary>
        /// Gets or sets the angle describing the magnitude of the rotation
        /// about the y-axis.
        /// </summary>
        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle describing the magnitude of the rotation about the y-axis.")]
        public float Angle { get; set; }

        /// <summary>
        /// Initializes a transform matrix for a rotation about the y-axis.
        /// </summary>
        /// <inheritdoc/>
        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateRotationY(Angle, out result);
        }
    }
}
