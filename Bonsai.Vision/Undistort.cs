using System;
using System.Linq;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that undistorts each image in the sequence using
    /// the specified camera intrinsics.
    /// </summary>
    [Description("Undistorts each image in the sequence using the specified camera intrinsics.")]
    public class Undistort : IntrinsicsTransform
    {
        /// <summary>
        /// Undistorts each image in an observable sequence using the specified
        /// camera intrinsics.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to undistort.
        /// </param>
        /// <returns>
        /// The sequence of undistorted images.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                Mat mapX = null;
                Mat mapY = null;
                Mat cameraMatrix = null;
                Mat distortionCoefficients = null;
                return source.Select(input =>
                {
                    if (mapX == null || mapY == null || mapX.Size != input.Size)
                    {
                        mapX = new Mat(input.Size, Depth.F32, 1);
                        mapY = new Mat(input.Size, Depth.F32, 1);
                    }

                    if (cameraMatrix != Intrinsics || distortionCoefficients != Distortion)
                    {
                        cameraMatrix = Intrinsics;
                        distortionCoefficients = Distortion;
                        CV.InitUndistortRectifyMap(cameraMatrix, distortionCoefficients, null, cameraMatrix, mapX, mapY);
                    }

                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    CV.Remap(input, output, mapX, mapY);
                    return output;
                });
            });
        }
    }
}
