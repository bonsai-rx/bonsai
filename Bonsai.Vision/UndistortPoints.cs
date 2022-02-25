using System;
using System.Linq;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that undistorts each point coordinate in the sequence
    /// using the specified camera intrinsics.
    /// </summary>
    [Description("Undistorts each point coordinate in the sequence using the specified camera intrinsics.")]
    public class UndistortPoints : IntrinsicsTransform
    {
        /// <summary>
        /// Undistorts each point coordinate in an observable sequence using the
        /// specified camera intrinsics.
        /// </summary>
        /// <param name="source">
        /// The sequence of points to undistort using the camera intrinsics.
        /// </param>
        /// <returns>
        /// A sequence of points where each value represents the point corresponding
        /// to the original sequence, if it were projected in the undistorted image
        /// obtained by the specified camera intrinsics.
        /// </returns>
        public IObservable<Point2f> Process(IObservable<Point2f> source)
        {
            return Process(source.Select(x => new[] { x })).Select(xs => xs[0]);
        }

        /// <summary>
        /// Undistorts each array of points in an observable sequence using the
        /// specified camera intrinsics.
        /// </summary>
        /// <param name="source">
        /// The sequence of arrays of points to undistort using the camera intrinsics.
        /// </param>
        /// <returns>
        /// A sequence of arrays of points where each value represents the point
        /// corresponding in the original array, if it were projected in the
        /// undistorted image obtained by the specified camera intrinsics.
        /// </returns>
        public IObservable<Point2f[]> Process(IObservable<Point2f[]> source)
        {
            return Observable.Defer(() =>
            {
                Mat cameraMatrix = null;
                Mat distortionCoefficients = null;
                return source.Select(input =>
                {
                    if (cameraMatrix != Intrinsics || distortionCoefficients != Distortion)
                    {
                        cameraMatrix = Intrinsics;
                        distortionCoefficients = Distortion;
                    }

                    var output = new Point2f[input.Length];
                    using (var inputHeader = Mat.CreateMatHeader(input, input.Length, 1, Depth.F32, 2))
                    using (var outputHeader = Mat.CreateMatHeader(output, output.Length, 1, Depth.F32, 2))
                    {
                        CV.UndistortPoints(inputHeader, outputHeader, cameraMatrix, distortionCoefficients);
                    }
                    return output;
                });
            });
        }

        /// <summary>
        /// Undistorts each matrix of points in an observable sequence using the
        /// specified camera intrinsics.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> values representing a row or column
        /// vector of points to undistort using the camera intrinsics.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects where each value represents a
        /// row or column vector of points which correspond to the original matrix,
        /// if each point was projected in the undistorted image obtained by the
        /// specified camera intrinsics.
        /// </returns>
        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                Mat cameraMatrix = null;
                Mat distortionCoefficients = null;
                return source.Select(input =>
                {
                    if (cameraMatrix != Intrinsics || distortionCoefficients != Distortion)
                    {
                        cameraMatrix = Intrinsics;
                        distortionCoefficients = Distortion;
                    }

                    var output = new Mat(input.Size, input.Depth, input.Channels);
                    CV.UndistortPoints(input, output, cameraMatrix, distortionCoefficients);
                    return output;
                });
            });
        }
    }
}
