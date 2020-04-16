using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [DefaultProperty("FileName")]
    [Description("Loads a set of camera extrinsics from a YML file.")]
    public class LoadExtrinsics : Source<Extrinsics>
    {
        [Description("The name of the camera extrinsics file.")]
        [FileNameFilter("YML Files (*.yml)|*.yml|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        Extrinsics CreateExtrinsics()
        {
            var fileName = FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException("A valid camera extrinsics file path was not specified.");
            }

            Extrinsics extrinsics;
            using (var storage = new MemStorage())
            using (var fileStorage = new FileStorage(fileName, storage, StorageFlags.FormatYaml))
            {
                if (fileStorage.IsInvalid)
                {
                    throw new InvalidOperationException("The specified camera extrinsics file does not exist.");
                }

                using (var root = fileStorage.GetRootFileNode())
                using (var rotation = fileStorage.GetFileNode(root, "rotation"))
                using (var translation = fileStorage.GetFileNode(root, "translation"))
                {
                    if (rotation != null)
                    {
                        extrinsics.Rotation.X = fileStorage.ReadReal(rotation, "x");
                        extrinsics.Rotation.Y = fileStorage.ReadReal(rotation, "y");
                        extrinsics.Rotation.Z = fileStorage.ReadReal(rotation, "z");
                    }
                    else extrinsics.Rotation = Point3d.Zero;

                    if (translation != null)
                    {
                        extrinsics.Translation.X = fileStorage.ReadReal(rotation, "x");
                        extrinsics.Translation.Y = fileStorage.ReadReal(rotation, "y");
                        extrinsics.Translation.Z = fileStorage.ReadReal(rotation, "z");
                    }
                    else extrinsics.Translation = Point3d.Zero;
                }
            }

            return extrinsics;
        }

        public override IObservable<Extrinsics> Generate()
        {
            return Observable.Defer(() => Observable.Return(CreateExtrinsics()));
        }

        public IObservable<Extrinsics> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => CreateExtrinsics());
        }
    }
}
