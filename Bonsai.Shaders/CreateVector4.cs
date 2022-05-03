using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a 4D vector element.
    /// </summary>
    [Description("Creates a 4D vector element.")]
    public class CreateVector4 : Source<Vector4>
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
        /// Gets or sets the z-component of the vector.
        /// </summary>
        [Description("The z-component of the vector.")]
        public float Z { get; set; }

        /// <summary>
        /// Gets or sets the w-component of the vector.
        /// </summary>
        [Description("The w-component of the vector.")]
        public float W { get; set; }

        /// <summary>
        /// Generates an observable sequence that returns a 4D vector element.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Vector4"/> object.
        /// </returns>
        public override IObservable<Vector4> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Vector4(X, Y, Z, W)));
        }

        /// <summary>
        /// Generates an observable sequence of 4D vectors, where each
        /// <see cref="Vector4"/> object is emitted only when an observable
        /// sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new vectors.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Vector4"/> values.
        /// </returns>
        public IObservable<Vector4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Vector4(X, Y, Z, W));
        }
    }
}
