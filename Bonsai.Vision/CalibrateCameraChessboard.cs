using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that finds the camera intrinsic parameters from
    /// several views of a chessboard calibration pattern.
    /// </summary>
    [Description("Finds the camera intrinsic parameters from several views of a chessboard calibration pattern.")]
    public class CalibrateCameraChessboard : Transform<KeyPointCollection[], CameraCalibration>
    {
        /// <summary>
        /// Gets or sets the number of inner corners per chessboard row and column.
        /// </summary>
        [Description("The number of inner corners per chessboard row and column.")]
        public Size PatternSize { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the operation flags used for calibrating
        /// camera intrinsics.
        /// </summary>
        [Description("Specifies the operation flags used for calibrating camera intrinsics.")]
        public CameraCalibrationFlags CalibrationFlags { get; set; } = CameraCalibrationFlags.FixPrincipalPoint;

        /// <summary>
        /// Finds the camera intrinsic parameters from an observable sequence of
        /// views of a chessboard calibration pattern.
        /// </summary>
        /// <param name="source">
        /// A sequence of image features extracted from different views of a
        /// chessboard calibration pattern.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="CameraCalibration"/> objects containing the camera
        /// intrinsic parameters and current re-projection error after processing
        /// each view of the chessboard pattern.
        /// </returns>
        public IObservable<CameraCalibration> Process(IObservable<KeyPointCollection> source)
        {
            return Process(source.Select(input => new[] { input }));
        }

        /// <summary>
        /// Finds the camera intrinsic parameters from an observable sequence of
        /// batches of views of a chessboard calibration pattern.
        /// </summary>
        /// <param name="source">
        /// A sequence of image features extracted from different views of a
        /// chessboard calibration pattern.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="CameraCalibration"/> objects containing the camera
        /// intrinsic parameters and current re-projection error after processing
        /// each batch of views of the chessboard pattern.
        /// </returns>
        public override IObservable<CameraCalibration> Process(IObservable<KeyPointCollection[]> source)
        {
            return Observable.Defer(() =>
            {
                var guess = false;
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
                        chessboardCorners = new Point3f[patternSize.Width * patternSize.Height];
                        for (int i = 0; i < chessboardCorners.Length; i++)
                        {
                            chessboardCorners[i].X = i % patternSize.Width;
                            chessboardCorners[i].Y = i / patternSize.Width;
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
