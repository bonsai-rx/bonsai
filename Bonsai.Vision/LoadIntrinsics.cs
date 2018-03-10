using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [DefaultProperty("FileName")]
    [Description("Loads a set of camera intrinsics from a YML file.")]
    public class LoadIntrinsics : Source<Intrinsics>
    {
        [Description("The name of the camera intrinsics file.")]
        [FileNameFilter("YML Files (*.yml)|*.yml|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
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
                    var imageWidth = fileStorage.ReadInt(root, "image_width");
                    var imageHeight = fileStorage.ReadInt(root, "image_height");
                    if (imageWidth > 0 && imageHeight > 0)
                    {
                        intrinsics.ImageSize = new Size(imageWidth, imageHeight);
                    }
                    else intrinsics.ImageSize = null;

                    using (var cameraMatrix = fileStorage.Read<Mat>(root, "camera_matrix"))
                    {
                        if (cameraMatrix != null)
                        {
                            var fx = cameraMatrix.GetReal(0, 0);
                            var fy = cameraMatrix.GetReal(1, 1);
                            var px = cameraMatrix.GetReal(0, 2);
                            var py = cameraMatrix.GetReal(1, 2);
                            intrinsics.FocalLength = new Point2d(fx, fy);
                            intrinsics.PrincipalPoint = new Point2d(px, py);
                        }
                        else
                        {
                            intrinsics.FocalLength = Point2d.Zero;
                            intrinsics.PrincipalPoint = Point2d.Zero;
                        }
                    }

                    using (var distortionCoefficients = fileStorage.Read<Mat>(root, "distortion_coefficients"))
                    {
                        if (distortionCoefficients != null)
                        {
                            var d0 = distortionCoefficients.GetReal(0);
                            var d1 = distortionCoefficients.GetReal(1);
                            var d2 = distortionCoefficients.GetReal(2);
                            var d3 = distortionCoefficients.GetReal(3);
                            var d4 = distortionCoefficients.GetReal(4);
                            intrinsics.RadialDistortion = new Point3d(d0, d1, d4);
                            intrinsics.TangentialDistortion = new Point2d(d2, d3);
                        }
                        else
                        {
                            intrinsics.RadialDistortion = Point3d.Zero;
                            intrinsics.TangentialDistortion = Point2d.Zero;
                        }
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
