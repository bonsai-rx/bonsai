using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Description("Finds the camera intrinsic parameters from several views of a chessboard calibration pattern.")]
    public class CalibrateCameraChessboard : Transform<KeyPointCollection[], CameraCalibration>
    {
        public CalibrateCameraChessboard()
        {
            CalibrationFlags = CameraCalibrationFlags.FixPrincipalPoint;
        }

        [Description("The number of inner corners per chessboard row and column.")]
        public Size PatternSize { get; set; }

        [Description("The physical size of each chessboard checker, in metric units.")]
        public float CheckerSize { get; set; }

        [Description("The available operation flags for calibrating camera intrinsics.")]
        public CameraCalibrationFlags CalibrationFlags { get; set; }

        public IObservable<CameraCalibration> Process(IObservable<KeyPointCollection> source)
        {
            return Process(source.Select(input => new[] { input }));
        }

        public override IObservable<CameraCalibration> Process(IObservable<KeyPointCollection[]> source)
        {
            return Observable.Defer(() =>
            {
                var guess = false;
                var cornerCount = 0;
                var checkerSize = 0f;
                var imageSize = new Size();
                var patternSize = new Size();
                var calibration = new CameraCalibration();
                var cameraMatrix = new Mat(3, 3, Depth.F32, 1);
                var distortion = new Mat(1, 5, Depth.F32, 1);
                Point3f[] chessboardCorners = null;
                return source.Select(input =>
                {
                    if (patternSize != PatternSize)
                    {
                        patternSize = PatternSize;
                        cornerCount = patternSize.Width * patternSize.Height;
                    }

                    if (checkerSize != CheckerSize && cornerCount > 0)
                    {
                        checkerSize = CheckerSize;
                        chessboardCorners = new Point3f[cornerCount];
                        for (int i = 0; i < chessboardCorners.Length; i++)
                        {
                            var x = i % patternSize.Width;
                            var y = i / patternSize.Width;
                            chessboardCorners[i].X = x * checkerSize;
                            chessboardCorners[i].Y = y * checkerSize;
                            chessboardCorners[i].Z = 0;
                        }
                    }

                    var flags = CalibrationFlags;
                    if (input.Length == 0)
                    {
                        if ((flags & CameraCalibrationFlags.UseIntrinsicGuess) == 0)
                        {
                            calibration = new CameraCalibration();
                        }
                        return calibration;
                    }

                    var totalPoints = 0;
                    var pointCounts = new int[input.Length];
                    for (int i = 0; i < pointCounts.Length; i++)
                    {
                        var image = input[i].Image;
                        if (input[i].Count == 0) continue;
                        if (imageSize.Width == 0) imageSize = image.Size;
                        else if (imageSize != image.Size)
                        {
                            throw new InvalidOperationException("Chessboard calibration features must come from same-sized images.");
                        }

                        pointCounts[i] = input[i].Count;
                        totalPoints += pointCounts[i];
                    }

                    var objectPoints = new Point3f[totalPoints];
                    var imagePoints = new Point2f[totalPoints];
                    totalPoints = 0;
                    for (int i = 0; i < input.Length; i++)
                    {
                        input[i].CopyTo(imagePoints, totalPoints);
                        Array.Copy(chessboardCorners, 0, objectPoints, totalPoints, input[i].Count);
                        totalPoints += input[i].Count;
                    }

                    if (!guess) flags &= ~CameraCalibrationFlags.UseIntrinsicGuess;
                    using (var objectPts = Mat.CreateMatHeader(objectPoints, objectPoints.Length, 3, Depth.F32, 1))
                    using (var imagePts = Mat.CreateMatHeader(imagePoints, imagePoints.Length, 2, Depth.F32, 1))
                    using (var pointCts = Mat.CreateMatHeader(pointCounts, pointCounts.Length, 1, Depth.S32, 1))
                    {
                        calibration.ReprojectionError = CV.CalibrateCamera2(
                            objectPts, imagePts, pointCts, imageSize,
                            cameraMatrix, distortion, null, null, flags);
                        Intrinsics.FromCameraMatrix(cameraMatrix, distortion, imageSize, out calibration.Intrinsics);
                    }

                    guess = true;
                    return calibration;
                });
            });
        }
    }
}
