using Bonsai.IO;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that writes a sequence of camera intrinsics to a YML file.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [Description("Writes a sequence of camera intrinsics to a YML file.")]
    public class SaveIntrinsics : Sink<Intrinsics>
    {
        /// <summary>
        /// Gets or sets the name of the file on which to write the camera intrinsics.
        /// </summary>
        [FileNameFilter("YML Files (*.yml)|*.yml|All Files|*.*")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the file on which to write the camera intrinsics.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the optional suffix used to generate file names.
        /// </summary>
        [Description("The optional suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        /// <summary>
        /// Writes an observable sequence of camera intrinsic properties to the
        /// specified YML file.
        /// </summary>
        /// <param name="source">
        /// The sequence of camera intrinsic properties to write.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the
        /// camera intrinsics to the specified YML file.
        /// </returns>
        public override IObservable<Intrinsics> Process(IObservable<Intrinsics> source)
        {
            return source.Do(WriteIntrinsics);
        }

        /// <summary>
        /// Writes an observable sequence of camera intrinsic properties extracted from
        /// a camera calibration procedure to the specified YML file.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="CameraCalibration"/> objects containing the camera
        /// intrinsic properties to write.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the
        /// calibrated camera intrinsics to the specified YML file.
        /// </returns>
        public IObservable<CameraCalibration> Process(IObservable<CameraCalibration> source)
        {
            return source.Do(calibration => WriteIntrinsics(calibration.Intrinsics, calibration.ReprojectionError));
        }

        private void WriteIntrinsics(Intrinsics intrinsics)
        {
            WriteIntrinsics(intrinsics, null);
        }

        private void WriteIntrinsics(Intrinsics intrinsics, double? reprojectionError)
        {
            var fileName = FileName;
            if (!string.IsNullOrEmpty(fileName))
            {
                PathHelper.EnsureDirectory(fileName);
                fileName = PathHelper.AppendSuffix(fileName, Suffix);
                using (var storage = new MemStorage())
                using (var fileStorage = new FileStorage(fileName, storage, StorageFlags.FormatYaml | StorageFlags.Write))
                {
                    var imageSize = intrinsics.ImageSize;
                    if (imageSize.HasValue)
                    {
                        fileStorage.WriteInt("image_width", imageSize.Value.Width);
                        fileStorage.WriteInt("image_height", imageSize.Value.Height);
                    }

                    var focalLength = intrinsics.FocalLength;
                    var principalPoint = intrinsics.PrincipalPoint;
                    var cameraMatrix = Mat.FromArray(new double[,]
                    {
                        {focalLength.X, 0, principalPoint.X},
                        {0, focalLength.Y, principalPoint.Y},
                        {0, 0, 1}
                    });

                    var radialDistortion = intrinsics.RadialDistortion;
                    var tangentialDistortion = intrinsics.TangentialDistortion;
                    var distortionCoefficients = Mat.FromArray(new double[,]
                    {
                        { radialDistortion.X },
                        { radialDistortion.Y },
                        { tangentialDistortion.X },
                        { tangentialDistortion.Y },
                        { radialDistortion.Z }
                    });

                    fileStorage.Write("camera_matrix", cameraMatrix);
                    fileStorage.Write("distortion_coefficients", distortionCoefficients);
                    if (reprojectionError.HasValue) fileStorage.WriteReal("reprojection_error", reprojectionError.Value);
                }
            }
        }
    }
}
