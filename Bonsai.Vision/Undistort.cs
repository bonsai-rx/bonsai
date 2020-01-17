using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Runtime.InteropServices;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Undistorts the input image using the specified intrinsic camera matrix.")]
    public class Undistort : IntrinsicsTransform
    {
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
