using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that returns the scale component of every matrix
    /// in the sequence.
    /// </summary>
    [Description("Returns the scale component of every matrix in the sequence.")]
    public class ExtractScale : Transform<Matrix4, Vector3>
    {
        /// <summary>
        /// Returns the scale component of every 4x4 matrix in an observable
        /// sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of 4x4 matrices for which to extract the scale
        /// component.
        /// </param>
        /// <returns>
        /// A <see cref="Vector3"/> object representing the scale component
        /// of each 4x4 matrix in the sequence.
        /// </returns>
        public override IObservable<Vector3> Process(IObservable<Matrix4> source)
        {
            return source.Select(input => input.ExtractScale());
        }
    }
}
