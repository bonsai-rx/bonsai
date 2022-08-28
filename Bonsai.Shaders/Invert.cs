using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that converts every matrix in the sequence to
    /// its inverse.
    /// </summary>
    [Description("Converts every matrix in the sequence to its inverse.")]
    public class Invert : Transform<Matrix4, Matrix4>
    {
        /// <summary>
        /// Converts every 3x3 matrix in an observable sequence to its inverse.
        /// </summary>
        /// <param name="source">The sequence of 3x3 matrices to invert.</param>
        /// <returns>The sequence of inverted 3x3 matrices.</returns>
        public IObservable<Matrix3> Process(IObservable<Matrix3> source)
        {
            return source.Select(input => input.Inverted());
        }

        /// <summary>
        /// Converts every 4x4 matrix in an observable sequence to its inverse.
        /// </summary>
        /// <param name="source">The sequence of 4x4 matrices to invert.</param>
        /// <returns>The sequence of inverted 4x4 matrices.</returns>
        public override IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return source.Select(input => input.Inverted());
        }
    }
}
