using System;
using System.ComponentModel;
using System.Drawing;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that updates the active scissor box in the shader
    /// window. Any fragments falling outside the scissor box will be discarded.
    /// </summary>
    [Description("Updates the active scissor box in the shader window. Any fragments falling outside the scissor box will be discarded.")]
    public class UpdateScissorState : Sink
    {
        /// <summary>
        /// Gets or sets the x-coordinate of the lower left corner of the scissor box.
        /// </summary>
        [Description("The x-coordinate of the lower left corner of the scissor box.")]
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the lower left corner of the scissor box.
        /// </summary>
        [Description("The y-coordinate of the lower left corner of the scissor box.")]
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets the width of the scissor box, in normalized coordinates.
        /// </summary>
        [Description("The width of the scissor box, in normalized coordinates.")]
        public float Width { get; set; } = 1;

        /// <summary>
        /// Gets or sets the height of the scissor box, in normalized coordinates.
        /// </summary>
        [Description("The height of the scissor box, in normalized coordinates.")]
        public float Height { get; set; } = 1;

        /// <summary>
        /// Updates the active scissor box in the shader window whenever an
        /// observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to update the active scissor box.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of updating the
        /// active scissor box in the shader window.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.WindowSource,
                (input, window) =>
                {
                    window.Scissor = new RectangleF(X, Y, Width, Height);
                    return input;
                });
        }
    }
}
