using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a 4x4 matrix containing 3D rotation,
    /// scale, position and projection.
    /// </summary>
    [Description("Creates a 4x4 matrix containing 3D rotation, scale, position and projection.")]
    public class CreateMatrix4 : Source<Matrix4>
    {
        /// <summary>
        /// Gets or sets the top row of the matrix.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The top row of the matrix.")]
        public Vector4 Row0 { get; set; }

        /// <summary>
        /// Gets or sets the second row of the matrix.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The second row of the matrix.")]
        public Vector4 Row1 { get; set; }

        /// <summary>
        /// Gets or sets the third row of the matrix.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The third row of the matrix.")]
        public Vector4 Row2 { get; set; }

        /// <summary>
        /// Gets or sets the bottom row of the matrix.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The bottom row of the matrix.")]
        public Vector4 Row3 { get; set; }

        /// <summary>
        /// Generates an observable sequence that contains a single 4x4 matrix
        /// with the specified rows.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Matrix4"/> object.
        /// </returns>
        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Matrix4(Row0, Row1, Row2, Row3)));
        }

        /// <summary>
        /// Generates an observable sequence of 4x4 matrices with the specified
        /// rows, and where each <see cref="Matrix4"/> object is emitted only
        /// when an observable sequence emits a notification.
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
            return source.Select(input => new Matrix4(Row0, Row1, Row2, Row3));
        }
    }
}
