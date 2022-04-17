using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a model matrix specifying position,
    /// rotation and scale.
    /// </summary>
    [Description("Creates a model matrix specifying position, rotation and scale.")]
    public class CreateTransform : Source<Matrix4>
    {
        Vector3 position;
        Quaternion rotation;
        Vector3 scale;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTransform"/> class.
        /// </summary>
        public CreateTransform()
        {
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }

        /// <summary>
        /// Gets or sets the position of the model, in the local coordinate frame.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The position of the model, in the local coordinate frame.")]
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Gets or sets the quaternion representing the rotation of the model,
        /// in the local coordinate frame.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The quaternion representing the rotation of the model, in the local coordinate frame.")]
        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        /// <summary>
        /// Gets or sets the scale vector applied to the model, in the local
        /// coordinate frame.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The scale vector applied to the model, in the local coordinate frame.")]
        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        Matrix4 GetTransform()
        {
            Matrix4.CreateScale(ref scale, out Matrix4 result);
            Matrix4.CreateFromQuaternion(ref rotation, out Matrix4 temp);
            Matrix4.Mult(ref result, ref temp, out result);
            Matrix4.CreateTranslation(ref position, out temp);
            Matrix4.Mult(ref result, ref temp, out result);
            return result;
        }

        /// <summary>
        /// Generates an observable sequence that returns a model matrix
        /// specifying position, rotation and scale.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Matrix4"/> object.
        /// </returns>
        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(GetTransform()));
        }

        /// <summary>
        /// Generates an observable sequence of model matrices specifying position,
        /// rotation and scale, and where each <see cref="Matrix4"/> object is
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
            return source.Select(input => GetTransform());
        }
    }
}
