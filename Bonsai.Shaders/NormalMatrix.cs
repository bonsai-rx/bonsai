using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that computes the normal matrix for each
    /// modelview matrix in the sequence.
    /// </summary>
    [Description("Computes the normal matrix for each modelview matrix in the sequence.")]
    public class NormalMatrix : Transform<Matrix4, Matrix4>
    {
        /// <summary>
        /// Computes the normal matrix for each 3x3 modelview matrix in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of 3x3 modelview matrices for which to compute the
        /// normal matrix.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Matrix3"/> objects representing the normal
        /// matrix for each modelview matrix in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<Matrix3> Process(IObservable<Matrix3> source)
        {
            return source.Select(input =>
            {
                if (input.Determinant == 0)
                {
                    return Matrix3.Zero;
                }
                input.Invert();
                input.Transpose();
                return input;
            });
        }

        /// <summary>
        /// Computes the normal matrix for each 4x4 modelview matrix in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of 4x4 modelview matrices for which to compute the
        /// normal matrix.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Matrix4"/> objects representing the normal
        /// matrix for each modelview matrix in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public override IObservable<Matrix4> Process(IObservable<Matrix4> source)
        {
            return source.Select(input =>
            {
                if (input.Determinant == 0)
                {
                    return Matrix4.Zero;
                }
                input.Invert();
                input.Transpose();
                return input;
            });
        }
    }
}
