using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a translation matrix.
    /// </summary>
    [Description("Creates a translation matrix.")]
    public class CreateTranslation : Source<Matrix4>
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
        /// Generates an observable sequence that returns a translation matrix.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Matrix4"/> object.
        /// </returns>
        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateTranslation(X, Y, Z)));
        }

        /// <summary>
        /// Generates an observable sequence of translation matrices, where each
        /// <see cref="Matrix4"/> object is emitted only when an observable
        /// sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new matrices.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Matrix4"/> values.
        /// </returns>
        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => Matrix4.CreateTranslation(X, Y, Z));
        }
    }
}
