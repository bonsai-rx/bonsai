using OpenTK;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that applies a translation along the specified
    /// axes to every transform in the sequence.
    /// </summary>
    [Description("Applies a translation along the specified axes to every transform in the sequence.")]
    public class Translate : MatrixTransform
    {
        /// <summary>
        /// Gets or sets the translation along the x-axis.
        /// </summary>
        [Range(-1, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The translation along the x-axis.")]
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the translation along the y-axis.
        /// </summary>
        [Range(-1, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The translation along the y-axis.")]
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets the translation along the z-axis.
        /// </summary>
        [Range(-1, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The translation along the z-axis.")]
        public float Z { get; set; }

        /// <summary>
        /// Initializes a transform matrix for applying a translation along the
        /// specified axes.
        /// </summary>
        /// <inheritdoc/>
        protected override void CreateTransform(out Matrix4 result)
        {
            Matrix4.CreateTranslation(X, Y, Z, out result);
        }
    }
}
