using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a 2D vector element.
    /// </summary>
    [Description("Creates a 2D vector element.")]
    public class CreateVector2 : Source<Vector2>
    {
        /// <summary>
        /// Gets or sets the x-component of the vector.
        /// </summary>
        [Description("The x-component of the vector.")]
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the y-component of the vector.
        /// </summary>
        [Description("The y-component of the vector.")]
        public float Y { get; set; }

        /// <summary>
        /// Generates an observable sequence that returns a 2D vector element.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Vector2"/> object.
        /// </returns>
        public override IObservable<Vector2> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Vector2(X, Y)));
        }

        /// <summary>
        /// Generates an observable sequence of 2D vectors, where each
        /// <see cref="Vector2"/> object is emitted only when an observable
        /// sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new vectors.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Vector2"/> values.
        /// </returns>
        public IObservable<Vector2> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Vector2(X, Y));
        }
    }
}
