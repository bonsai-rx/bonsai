using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that computes a sequence of camera extrinsics from
    /// sets of 3D-2D point correspondences and the specified camera intrinsics.
    /// </summary>
    [Description("Computes a sequence of camera extrinsics from sets of 3D-2D point correspondences and the specified camera intrinsics.")]
    public class SolvePnP : IntrinsicsTransform
    {
        Extrinsics FindExtrinsics(
            Point3d[] objectPoints,
            Point2d[] imagePoints,
            Mat cameraMatrix,
            Mat distortionCoefficients,
            Point3d[] rotation,
            Point3d[] translation,
            bool useExtrinsicGuess)
        {
            using (var rotationVector = Mat.CreateMatHeader(rotation, 1, 3, Depth.F64, 1))
            using (var translationVector = Mat.CreateMatHeader(translation, 1, 3, Depth.F64, 1))
            {
                var objectPts = Mat.FromArray(objectPoints, objectPoints.Length, 1, Depth.F64, 3);
                var imagePts = Mat.FromArray(imagePoints, imagePoints.Length, 1, Depth.F64, 2);
                CV.FindExtrinsicCameraParams2(
                    objectPts, imagePts,
                    cameraMatrix, distortionCoefficients,
                    rotationVector, translationVector,
                    useExtrinsicGuess);
                return new Extrinsics
                {
                    Rotation = rotation[0],
                    Translation = translation[0]
                };
            }
        }

        /// <summary>
        /// Computes an observable sequence of camera extrinsics from sets of 3D-2D
        /// point correspondences and the specified camera intrinsics.
        /// </summary>
        /// <param name="source">
        /// A sequence of 3D-2D point correspondences used to compute the camera
        /// extrinsics. For each 3D point in the first array, the corresponding 2D
        /// point in the second array represents the matching projection of that 3D
        /// point in the camera image.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Extrinsics"/> objects representing the camera
        /// extrinsics, such as position and rotation, computed from each set of
        /// 3D-2D point correspondences.
        /// </returns>
        public IObservable<Extrinsics> Process(IObservable<Tuple<Point3d[], Point2d[]>> source)
        {
            return Observable.Defer(() =>
            {
                Mat cameraMatrix = null;
                Mat distortionCoefficients = null;
                var rotation = new Point3d[1];
                var translation = new Point3d[1];
                return source.Select(input =>
                {
                    if (cameraMatrix != Intrinsics || distortionCoefficients != Distortion)
                    {
                        cameraMatrix = Intrinsics;
                        distortionCoefficients = Distortion;
                    }

                    return FindExtrinsics(input.Item1, input.Item2, cameraMatrix, distortionCoefficients, rotation, translation, false);
                });
            });
        }

        /// <summary>
        /// Computes an observable sequence of camera extrinsics from sets of 3D-2D
        /// point correspondences, the specified camera intrinsics and an initial
        /// estimate of the camera extrinsics.
        /// </summary>
        /// <param name="source">
        /// A sequence of triplets containing the 3D-2D point correspondences and a
        /// prior estimate used to compute the camera extrinsics. For each 3D point
        /// in the first array, the corresponding 2D point in the second array
        /// represents the matching projection of that 3D point in the camera image.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Extrinsics"/> objects representing the camera
        /// extrinsics, such as position and rotation, computed from each set of
        /// 3D-2D point correspondences and an initial estimate of the extrinsics.
        /// </returns>
        public IObservable<Extrinsics> Process(IObservable<Tuple<Point3d[], Point2d[], Extrinsics>> source)
        {
            return Observable.Defer(() =>
            {
                Mat cameraMatrix = null;
                Mat distortionCoefficients = null;
                var rotation = new Point3d[1];
                var translation = new Point3d[1];
                return source.Select(input =>
                {
                    if (cameraMatrix != Intrinsics || distortionCoefficients != Distortion)
                    {
                        cameraMatrix = Intrinsics;
                        distortionCoefficients = Distortion;
                    }

                    rotation[0] = input.Item3.Rotation;
                    translation[0] = input.Item3.Translation;
                    return FindExtrinsics(input.Item1, input.Item2, cameraMatrix, distortionCoefficients, rotation, translation, true);
                });
            });
        }
    }
}
