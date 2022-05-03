using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a perspective projection matrix
    /// from field of view parameters.
    /// </summary>
    [Description("Creates a perspective projection matrix from field of view parameters.")]
    public class CreatePerspectiveFieldOfView : Source<Matrix4>
    {
        /// <summary>
        /// Gets or sets the angle of the field of view in the y direction,
        /// in radians.
        /// </summary>
        [Range(Math.PI / 180, Math.PI - Math.PI / 180)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The angle of the field of view in the y direction, in radians.")]
        public float FovY { get; set; } = MathHelper.PiOver3;

        /// <summary>
        /// Gets or sets the aspect ratio of the viewport.
        /// </summary>
        [Description("The aspect ratio of the viewport.")]
        public float AspectRatio { get; set; } = 1;

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

        /// <summary>
        /// Generates an observable sequence that returns a 4x4 perspective
        /// projection matrix with the specified parameters.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Matrix4"/> object.
        /// </returns>
        public override IObservable<Matrix4> Generate()
        {
            return Observable.Defer(() => Observable.Return(Matrix4.CreatePerspectiveFieldOfView(FovY, AspectRatio, NearClip, FarClip)));
        }

        /// <summary>
        /// Generates an observable sequence of perspective matrices with the
        /// specified parameters, where each <see cref="Matrix4"/> object is
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
            return source.Select(x => Matrix4.CreatePerspectiveFieldOfView(FovY, AspectRatio, NearClip, FarClip));
        }
    }
}
