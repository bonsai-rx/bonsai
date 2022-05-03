using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that calculates the transpose of every matrix
    /// in the sequence.
    /// </summary>
    [Description("Calculates the transpose of every matrix in the sequence.")]
    public class Transpose : Transform<Matrix4, Matrix4>
    {
        /// <summary>
        /// Calculates the transpose of every 3x3 matrix in an observable sequence.
        /// </summary>
        /// <param name="source">The sequence of 3x3 matrices to transpose.</param>
        /// <returns>The sequence of transposed 3x3 matrices.</returns>
        public IObservable<Matrix3> Process(IObservable<Matrix3> source)
        {
            return source.Select(Matrix3.Transpose);
        }

        /// <summary>
        /// Calculates the transpose of every 4x4 matrix in an observable sequence.
        /// </summary>
        /// <param name="source">The sequence of 4x4 matrices to transpose.</param>
        /// <returns>The sequence of transposed 4x4 matrices.</returns>
        public override IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return source.Select(Matrix4.Transpose);
        }
    }
}
