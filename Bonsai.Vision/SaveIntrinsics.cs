using Bonsai.IO;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [DefaultProperty("FileName")]
    [Description("Saves a set of camera intrinsics to a YML file.")]
    public class SaveIntrinsics : Sink<Intrinsics>
    {
        [FileNameFilter("YML Files (*.yml)|*.yml|All Files|*.*")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the output camera intrinsics file.")]
        public string FileName { get; set; }

        [Description("The optional suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        public override IObservable<Intrinsics> Process(IObservable<Intrinsics> source)
        {
            return source.Do(WriteIntrinsics);
        }

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
