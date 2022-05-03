using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that returns the rotation component of every
    /// matrix in the sequence.
    /// </summary>
    [Description("Returns the rotation component of every matrix in the sequence.")]
    public class ExtractRotation : Transform<Matrix4, Quaternion>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to row-normalize the input
        /// matrix. Keep this unless you know the input is already normalized.
        /// </summary>
        [Description("Indicates whether to row-normalize the input matrix. Keep this unless you know the input is already normalized.")]
        public bool RowNormalize { get; set; } = true;

        /// <summary>
        /// Returns the rotation component of every 4x4 matrix in an observable
        /// sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of 4x4 matrices for which to extract the rotation
        /// component.
        /// </param>
        /// <returns>
        /// A <see cref="Quaternion"/> object representing the rotation component
        /// of each 4x4 matrix in the sequence.
        /// </returns>
        public override IObservable<Quaternion> Process(IObservable<Matrix4> source)
        {
            return source.Select(input => input.ExtractRotation(RowNormalize));
        }
    }
}
