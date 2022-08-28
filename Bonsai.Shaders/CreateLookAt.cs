using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a view matrix specifying a camera
    /// looking at a target position.
    /// </summary>
    [Description("Creates a view matrix specifying a camera looking at a target position.")]
    public class CreateLookAt : Source<Matrix4>
    {
        Vector3 eye;
        Vector3 target;
        Vector3 up;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateLookAt"/> class.
        /// </summary>
        public CreateLookAt()
        {
            target = -Vector3.UnitZ;
            up = Vector3.UnitY;
        }

        /// <summary>
        /// Gets or sets the eye, or camera position, in the world coordinate frame.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The eye, or camera position, in the world coordinate frame.")]
        public Vector3 Eye
        {
            get { return eye; }
            set { eye = value; }
        }

        /// <summary>
        /// Gets or sets the target position in the world coordinate frame.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The target position in the world coordinate frame.")]
        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        /// <summary>
        /// Gets or sets a 3D vector specifying the up vector of the camera, in the
        /// world coordinate frame. Should not be parallel to the camera direction.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("Specifies the up vector of the camera, in the world coordinate frame. Should not be parallel to the camera direction.")]
        public Vector3 Up
        {
            get { return up; }
            set { up = value; }
        }

        /// <summary>
        /// Generates an observable sequence that contains a single view matrix
        /// representing a camera with the specified position and up vector,
        /// looking at the specified target.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Matrix4"/> look-at view
        /// matrix for transforming world space into camera space.
        /// </returns>
        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.LookAt(
                eye.X, eye.Y, eye.Z,
                target.X, target.Y, target.Z,
                up.X, up.Y, up.Z)));
        }

        /// <summary>
        /// Generates an observable sequence of view matrices representing a camera
        /// with the specified position and up vector, looking at the specified
        /// target, where each <see cref="Matrix4"/> object is emitted only when an
        /// observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new matrices.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Matrix4"/> look-at view matrices
        /// for transforming world space into camera space.
        /// </returns>
        public IObservable<Matrix4> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => Matrix4.LookAt(
                eye.X, eye.Y, eye.Z,
                target.X, target.Y, target.Z,
                up.X, up.Y, up.Z));
        }
    }
}
