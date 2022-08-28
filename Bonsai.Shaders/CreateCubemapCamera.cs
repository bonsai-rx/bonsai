using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that generates a sequence of perspective camera
    /// objects which can be used to render a dynamic cubemap texture.
    /// </summary>
    [Description("Generates a sequence of perspective camera objects which can be used to render a dynamic cubemap texture.")]
    public class CreateCubemapCamera : Source<Camera>
    {
        /// <summary>
        /// Gets or sets the eye, or camera position, in the world coordinate frame.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The eye, or camera position, in the world coordinate frame.")]
        public Vector3 Eye { get; set; }

        /// <summary>
        /// Gets or sets the distance to the near clip plane.
        /// </summary>
        [Category("Z-Clipping")]
        [Description("The distance to the near clip plane.")]
        public float NearClip { get; set; } = 0.1f;

        /// <summary>
        /// Gets or sets the distance to the far clip plane.
        /// </summary>
        [Category("Z-Clipping")]
        [Description("The distance to the far clip plane.")]
        public float FarClip { get; set; } = 1000f;

        IEnumerable<Camera> GenerateCubemapViews()
        {
            var eye = Eye;
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1, NearClip, FarClip);
            yield return new Camera(Matrix4.LookAt(eye, eye + Vector3.UnitX, -Vector3.UnitY), projection);
            yield return new Camera(Matrix4.LookAt(eye, eye - Vector3.UnitX, -Vector3.UnitY), projection);
            yield return new Camera(Matrix4.LookAt(eye, eye + Vector3.UnitY, Vector3.UnitZ), projection);
            yield return new Camera(Matrix4.LookAt(eye, eye - Vector3.UnitY, -Vector3.UnitZ), projection);
            yield return new Camera(Matrix4.LookAt(eye, eye + Vector3.UnitZ, -Vector3.UnitY), projection);
            yield return new Camera(Matrix4.LookAt(eye, eye - Vector3.UnitZ, -Vector3.UnitY), projection);
        }

        /// <summary>
        /// Generates an observable sequence of perspective camera objects which
        /// can be used to render a dynamic cubemap texture.
        /// </summary>
        /// <returns>
        /// A sequence of six <see cref="Camera"/> objects corresponding to each
        /// direction of the cubemap, respectively right (+X), left (-X), top (+Y),
        /// bottom (-Y), back (+Z), and front (-Z).
        /// </returns>
        public override IObservable<Camera> Generate()
        {
            return GenerateCubemapViews().ToObservable();
        }

        /// <summary>
        /// Generates an observable sequence of perspective camera objects which
        /// can be used to render a dynamic cubemap texture, where the set of
        /// of <see cref="Camera"/> objects for each cubemap is emitted only
        /// when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting each new
        /// group of six cubemap views.
        /// </param>
        /// <returns>
        /// The sequence of <see cref="Camera"/> objects corresponding to each
        /// direction of the cubemap, respectively right (+X), left (-X), top (+Y),
        /// bottom (-Y), back (+Z), and front (-Z), for each notification in
        /// the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Camera> Generate<TSource>(IObservable<TSource> source)
        {
            return source.SelectMany(input => GenerateCubemapViews());
        }
    }
}
