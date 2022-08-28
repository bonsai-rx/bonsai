using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a rotation matrix from an axis-angle
    /// representation.
    /// </summary>
    [Description("Creates a rotation matrix from an axis-angle representation.")]
    public class CreateAxisAngleRotation : Source<Matrix4>
    {
        /// <summary>
        /// Gets or sets a 3D vector specifying the direction of the axis of rotation.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("Specifies the direction of the axis of rotation.")]
        public Vector3 Axis { get; set; }

        /// <summary>
        /// Gets or sets the angle describing the magnitude of the rotation about
        /// the axis.
        /// </summary>
        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle describing the magnitude of the rotation about the axis.")]
        public float Angle { get; set; }

        /// <summary>
        /// Generates an observable sequence that contains a single rotation matrix
        /// created from the specified axis-angle rotation.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Matrix4"/> rotation matrix.
        /// </returns>
        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreateFromAxisAngle(Axis, Angle)));
        }

        /// <summary>
        /// Generates an observable sequence of matrices representing the specified
        /// axis-angle rotation, and where each <see cref="Matrix4"/> object is
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
            return source.Select(input => Matrix4.CreateFromAxisAngle(Axis, Angle));
        }
    }
}
