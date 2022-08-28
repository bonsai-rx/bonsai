using System;
using System.ComponentModel;
using System.Drawing;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that updates the active viewport in the shader window.
    /// </summary>
    [Description("Updates the active viewport in the shader window.")]
    public class UpdateViewportState : Sink
    {
        /// <summary>
        /// Gets or sets the x-coordinate of the lower left corner of the viewport.
        /// </summary>
        [Description("The x-coordinate of the lower left corner of the viewport.")]
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the lower left corner of the viewport.
        /// </summary>
        [Description("The y-coordinate of the lower left corner of the viewport.")]
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets the width of the viewport rectangle, in normalized coordinates.
        /// </summary>
        [Description("The width of the viewport rectangle, in normalized coordinates.")]
        public float Width { get; set; } = 1;

        /// <summary>
        /// Gets or sets the height of the viewport rectangle, in normalized coordinates.
        /// </summary>
        [Description("The height of the viewport rectangle, in normalized coordinates.")]
        public float Height { get; set; } = 1;

        /// <summary>
        /// Updates the active viewport in the shader window whenever an observable
        /// sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to update the active viewport.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of updating the
        /// active viewport in the shader window.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.WindowSource,
                (input, window) =>
                {
                    window.Viewport = new RectangleF(X, Y, Width, Height);
                    return input;
                });
        }
    }
}
