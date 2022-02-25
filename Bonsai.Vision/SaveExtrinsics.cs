using Bonsai.IO;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that writes a sequence of camera extrinsics to a YML file.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [Description("Writes a sequence of camera extrinsics to a YML file.")]
    public class SaveExtrinsics : Sink<Extrinsics>
    {
        /// <summary>
        /// Gets or sets the name of the file on which to write the camera extrinsics.
        /// </summary>
        [FileNameFilter("YML Files (*.yml)|*.yml|All Files|*.*")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the file on which to write the camera extrinsics.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the optional suffix used to generate file names.
        /// </summary>
        [Description("The optional suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        /// <summary>
        /// Writes an observable sequence of camera extrinsic properties to the
        /// specified YML file.
        /// </summary>
        /// <param name="source">
        /// The sequence of camera extrinsic properties to write.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the
        /// camera extrinsics to the specified YML file.
        /// </returns>
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
