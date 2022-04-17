using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a rotation matrix from a quaternion
    /// representation.
    /// </summary>
    [Description("Creates a rotation matrix from a quaternion representation.")]
    public class CreateQuaternionRotation : Source<Matrix4>
    {
        /// <summary>
        /// Gets or sets the quaternion representing the rotation transform.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The quaternion representing the rotation transform.")]
        public Quaternion Rotation { get; set; } = Quaternion.Identity;

        /// <summary>
        /// Generates an observable sequence that returns a rotation matrix
        /// corresponding to the specified quaternion.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Matrix4"/> object.
        /// </returns>
        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateFromQuaternion(Rotation)));
        }

        /// <summary>
        /// Generates an observable sequence of rotation matrices from the
        /// specified quaternion, where each <see cref="Matrix4"/> object is
        /// emitted only when an observable sequence emits a notification.
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
            return source.Select(input => Matrix4.CreateFromQuaternion(Rotation));
        }
    }
}
