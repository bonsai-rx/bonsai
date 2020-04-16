using Bonsai.IO;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [DefaultProperty("FileName")]
    [Description("Saves a set of camera extrinsics to a YML file.")]
    public class SaveExtrinsics : Sink<Extrinsics>
    {
        [FileNameFilter("YML Files (*.yml)|*.yml|All Files|*.*")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the output camera extrinsics file.")]
        public string FileName { get; set; }

        [Description("The optional suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        public override IObservable<Extrinsics> Process(IObservable<Extrinsics> source)
        {
            return source.Do(extrinsics =>
            {
                var fileName = FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    PathHelper.EnsureDirectory(fileName);
                    fileName = PathHelper.AppendSuffix(fileName, Suffix);
                    using (var storage = new MemStorage())
                    using (var fileStorage = new FileStorage(fileName, storage, StorageFlags.FormatYaml | StorageFlags.Write))
                    {
                        fileStorage.StartWriteStruct("rotation", StructStorageFlags.Map);
                        fileStorage.WriteReal("x", extrinsics.Rotation.X);
                        fileStorage.WriteReal("y", extrinsics.Rotation.Y);
                        fileStorage.WriteReal("z", extrinsics.Rotation.Z);
                        fileStorage.EndWriteStruct();

                        fileStorage.StartWriteStruct("translation", StructStorageFlags.Map);
                        fileStorage.WriteReal("x", extrinsics.Translation.X);
                        fileStorage.WriteReal("y", extrinsics.Translation.Y);
                        fileStorage.WriteReal("z", extrinsics.Translation.Z);
                        fileStorage.EndWriteStruct();
                    }
                }
            });
        }
    }
}
