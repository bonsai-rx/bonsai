using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a 2x2 matrix.
    /// </summary>
    [Description("Creates a 2x2 matrix.")]
    public class CreateMatrix2 : Source<Matrix2>
    {
        /// <summary>
        /// Gets or sets the top row of the matrix.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector2 Row0 { get; set; }

        /// <summary>
        /// Gets or sets the bottom row of the matrix.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector2 Row1 { get; set; }

        /// <summary>
        /// Generates an observable sequence that contains a single 2x2 matrix
        /// with the specified rows.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Matrix2"/> object.
        /// </returns>
        public override IObservable<Matrix2> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix2(Row0, Row1)));
        }

        /// <summary>
        /// Generates an observable sequence of 2x2 matrices with the specified
        /// rows, and where each <see cref="Matrix2"/> object is emitted only
        /// when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new matrices.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Matrix2"/> values.
        /// </returns>
        public IObservable<Matrix2> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Matrix2(Row0, Row1));
        }
    }
}
