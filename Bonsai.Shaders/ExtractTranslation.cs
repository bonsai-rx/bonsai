using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that returns the translation component of every
    /// matrix in the sequence.
    /// </summary>
    [Description("Returns the translation component of every matrix in the sequence.")]
    public class ExtractTranslation : Transform<Matrix4, Vector3>
    {
        /// <summary>
        /// Returns the translation component of every 4x4 matrix in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of 4x4 matrices for which to extract the translation
        /// component.
        /// </param>
        /// <returns>
        /// A <see cref="Vector3"/> object representing the translation component
        /// of each 4x4 matrix in the sequence.
        /// </returns>
        public override IObservable<Vector3> Process(IObservable<Matrix4> source)
        {
            return source.Select(input => input.ExtractTranslation());
        }
    }
}
