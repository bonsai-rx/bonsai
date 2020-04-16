using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [DefaultProperty("FileName")]
    [Description("Loads a set of camera intrinsics from a YML file.")]
    public class LoadIntrinsics : Source<Intrinsics>
    {
        [Description("The name of the camera intrinsics file.")]
        [FileNameFilter("YML Files (*.yml)|*.yml|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        Intrinsics CreateIntrinsics()
        {
            var fileName = FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException("A valid camera intrinsics file path was not specified.");
            }

            Intrinsics intrinsics;
            using (var storage = new MemStorage())
            using (var fileStorage = new FileStorage(fileName, storage, StorageFlags.FormatYaml))
            {
                if (fileStorage.IsInvalid)
                {
                    throw new InvalidOperationException("The specified camera intrinsics file does not exist.");
                }

                using (var root = fileStorage.GetRootFileNode())
                {
                    Size? imageSize;
                    var imageWidth = fileStorage.ReadInt(root, "image_width");
                    var imageHeight = fileStorage.ReadInt(root, "image_height");
                    if (imageWidth > 0 && imageHeight > 0)
                    {
                        imageSize = new Size(imageWidth, imageHeight);
                    }
                    else imageSize = null;

                    using (var cameraMatrix = fileStorage.Read<Mat>(root, "camera_matrix"))
                    using (var distortionCoefficients = fileStorage.Read<Mat>(root, "distortion_coefficients"))
                    {
                        Intrinsics.FromCameraMatrix(cameraMatrix, distortionCoefficients, imageSize, out intrinsics);
                    }
                }
            }

            return intrinsics;
        }

        public override IObservable<Intrinsics> Generate()
        {
            return Observable.Defer(() => Observable.Return(CreateIntrinsics()));
        }

        public IObservable<Intrinsics> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => CreateIntrinsics());
        }
    }
}
