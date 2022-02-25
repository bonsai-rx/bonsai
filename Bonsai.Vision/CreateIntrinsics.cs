using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that creates a set of parameters specifying
    /// the camera intrinsics.
    /// </summary>
    [Description("Creates a set of parameters specifying the camera intrinsics.")]
    public class CreateIntrinsics : Source<Intrinsics>
    {
        /// <summary>
        /// Gets or sets the image size, in pixels, for the camera intrinsics.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The optional image size, in pixels, for the camera intrinsics.")]
        public Size? ImageSize { get; set; }

        /// <summary>
        /// Gets or sets the focal length of the camera, in units of pixels.
        /// </summary>
        [Description("The focal length of the camera, in units of pixels.")]
        public Point2d FocalLength { get; set; } = new Point2d(1, 1);

        /// <summary>
        /// Gets or sets the principal point of the camera, in pixels, usually at the image center.
        /// </summary>
        [Description("The principal point of the camera, in pixexls, usually at the image center.")]
        public Point2d PrincipalPoint { get; set; }

        /// <summary>
        /// Gets or sets the radial distortion coefficients.
        /// </summary>
        [Description("The radial distortion coefficients.")]
        public Point3d RadialDistortion { get; set; }

        /// <summary>
        /// Gets or sets the tangential distortion coefficients.
        /// </summary>
        [Description("The tangential distortion coefficients.")]
        public Point2d TangentialDistortion { get; set; }

        Intrinsics Create()
        {
            return new Intrinsics
            {
                ImageSize = ImageSize,
                FocalLength = FocalLength,
                PrincipalPoint = PrincipalPoint,
                RadialDistortion = RadialDistortion,
                TangentialDistortion = TangentialDistortion
            };
        }

        /// <summary>
        /// Generates an observable sequence that contains the camera intrinsics
        /// using the specified focal length and distortion parameters.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Intrinsics"/> structure.
        /// </returns>
        public override IObservable<Intrinsics> Generate()
        {
            return Observable.Defer(() => Observable.Return(Create()));
        }

        /// <summary>
        /// Generates an observable sequence of camera intrinsics using the specified
        /// focal length and distortion parameters, and where each <see cref="Intrinsics"/>
        /// object is emitted only when an observable sequence raises a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new camera intrinsics.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Intrinsics"/> objects.
        /// </returns>
        public IObservable<Intrinsics> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => Create());
        }
    }
}
