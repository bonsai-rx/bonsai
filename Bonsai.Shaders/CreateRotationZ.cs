using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a rotation matrix for a rotation
    /// about the z-axis.
    /// </summary>
    [Description("Creates a rotation matrix for a rotation about the z-axis.")]
    public class CreateRotationZ : Source<Matrix4>
    {
        /// <summary>
        /// Gets or sets the angle describing the magnitude of the rotation
        /// about the z-axis.
        /// </summary>
        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle describing the magnitude of the rotation about the z-axis.")]
        public float Angle { get; set; }

        /// <summary>
        /// Generates an observable sequence that returns a rotation matrix
        /// for a rotation with the specified angle about the z-axis.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Matrix4"/> object.
        /// </returns>
        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateRotationZ(Angle)));
        }

        /// <summary>
        /// Generates an observable sequence of rotation matrices for a rotation with
        /// the specified angle about the z-axis, where each <see cref="Matrix4"/>
        /// object is emitted only when an observable sequence emits a notification.
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
            return source.Select(input => Matrix4.CreateRotationZ(Angle));
        }
    }
}
