using OpenTK;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that applies a scale factor along the specified
    /// axes to every transform in the sequence.
    /// </summary>
    [Description("Applies a scale factor along the specified axes to every transform in the sequence.")]
    public class Scale : MatrixTransform
    {
        /// <summary>
        /// Gets or sets the scale factor for the x-axis.
        /// </summary>
        [Range(0, 2)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The scale factor for the x-axis.")]
        public float X { get; set; } = 1;

        /// <summary>
        /// Gets or sets the scale factor for the y-axis.
        /// </summary>
        [Range(0, 2)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The scale factor for the y-axis.")]
        public float Y { get; set; } = 1;

        /// <summary>
        /// Gets or sets the scale factor for the z-axis.
        /// </summary>
        [Range(0, 2)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The scale factor for the z-axis.")]
        public float Z { get; set; } = 1;

        /// <summary>
        /// Initializes a transform matrix for applying a scale factor along
        /// the specified axes.
        /// </summary>
        /// <inheritdoc/>
        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateScale(X, Y, Z, out result);
        }
    }
}
