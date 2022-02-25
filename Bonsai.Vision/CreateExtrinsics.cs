using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that creates a set of parameters specifying
    /// the camera extrinsics.
    /// </summary>
    [Description("Creates a set of parameters specifying the camera extrinsics.")]    
    public class CreateExtrinsics : Source<Extrinsics>
    {
        /// <summary>
        /// Gets or sets the rotation vector transforming object-space coordinates into
        /// camera-space coordinates.
        /// </summary>
        [Description("The rotation vector transforming object-space coordinates into camera-space coordinates.")]
        public Point3d Rotation { get; set; }

        /// <summary>
        /// Gets or sets the translation vector transforming object-space coordinates into
        /// camera-space coordinates.
        /// </summary>
        [Description("The translation vector transforming object-space coordinates into camera-space coordinates.")]
        public Point3d Translation { get; set; }

        Extrinsics Create()
        {
            return new Extrinsics
            {
                Rotation = Rotation,
                Translation = Translation
            };
        }

        /// <summary>
        /// Generates an observable sequence that contains the camera extrinsics
        /// using the specified rotation and translation vectors.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Extrinsics"/> structure.
        /// </returns>
        public override IObservable<Extrinsics> Generate()
        {
            return Observable.Defer(() => Observable.Return(Create()));
        }

        /// <summary>
        /// Generates an observable sequence of camera extrinsics objects using the
        /// specified rotation and translation vectors, and where each <see cref="Extrinsics"/>
        /// object is emitted only when an observable sequence raises a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new camera extrinsics.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Extrinsics"/> objects.
        /// </returns>
        public IObservable<Extrinsics> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => Create());
        }
    }
}
