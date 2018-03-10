using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Description("Computes the camera extrinsics from a set of 3D-2D point correspondences.")]
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
