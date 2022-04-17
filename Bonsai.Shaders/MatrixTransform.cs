using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Provides an abstract base class for operators specifying cumulative matrix
    /// transform operations.
    /// </summary>
    public abstract class MatrixTransform : Transform<Matrix4, Matrix4>
    {
        /// <summary>
        /// Gets or sets a value specifying the order of relative matrix
        /// transform operations.
        /// </summary>
        [Description("Specifies the order of relative matrix transform operations.")]
        public MatrixOrder Order { get; set; }

        /// <summary>
        /// When overridden in a derived class, initializes the matrix transform
        /// that should be combined with each element in the sequence. 
        /// </summary>
        /// <param name="result">
        /// When this method returns, contains the relative matrix transform
        /// to be combined with each element in the sequence.
        /// </param>
        protected abstract void CreateTransform(out Matrix4 result);

        /// <summary>
        /// Performs a relative matrix transform operation on each matrix in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of transform matrices to be combined with the specified
        /// relative matrix transform.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Matrix4"/> objects representing the combined
        /// transform.
        /// </returns>
        public override IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return source.Select(input =>
            {
                CreateTransform(out Matrix4 result);
                if (Order == MatrixOrder.Append) Matrix4.Mult(ref input, ref result, out result);
                else Matrix4.Mult(ref result, ref input, out result);
                return result;
            });
        }
    }

    /// <summary>
    /// Specifies the transformation order for cumulative matrix transform operations.
    /// </summary>
    public enum MatrixOrder
    {
        /// <summary>
        /// Specifies that the new operation is applied after the preceding transform.
        /// </summary>
        Append,

        /// <summary>
        /// Specifies that the new operation is applied before the preceding transform.
        /// </summary>
        Prepend
    }
}
