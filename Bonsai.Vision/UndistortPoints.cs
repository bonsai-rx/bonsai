using System;
using System.Linq;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Undistorts the observed point coordinates using the specified intrinsic camera matrix.")]
    public class UndistortPoints : IntrinsicsTransform
    {
        public IObservable<Point2f> Process(IObservable<Point2f> source)
        {
            return Process(source.Select(x => new[] { x })).Select(xs => xs[0]);
        }

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
