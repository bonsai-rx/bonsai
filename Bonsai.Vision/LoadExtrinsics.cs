using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that loads a set of camera extrinsics from a YML file.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [Description("Loads a set of camera extrinsics from a YML file.")]
    public class LoadExtrinsics : Source<Extrinsics>
    {
        /// <summary>
        /// Gets or sets the name of the camera extrinsics file.
        /// </summary>
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
                        extrinsics.Translation.X = fileStorage.ReadReal(translation, "x");
                        extrinsics.Translation.Y = fileStorage.ReadReal(translation, "y");
                        extrinsics.Translation.Z = fileStorage.ReadReal(translation, "z");
                    }
                    else extrinsics.Translation = Point3d.Zero;
                }
            }

            return extrinsics;
        }

        /// <summary>
        /// Generates an observable sequence that contains the camera extrinsics
        /// loaded from the specified YML file.
        /// </summary>
        /// <returns>
        /// A sequence containing a single <see cref="Extrinsics"/> object representing
        /// the camera extrinsics loaded from the specified YML file.
        /// </returns>
        public override IObservable<Extrinsics> Generate()
        {
            return Observable.Defer(() => Observable.Return(CreateExtrinsics()));
        }

        /// <summary>
        /// Generates an observable sequence of camera extrinsics loaded from the
        /// specified YML file, and where each <see cref="Extrinsics"/> object
        /// is loaded only when an observable sequence raises a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for loading new camera
        /// extrinsics.
        /// </param>
        /// <returns>
        /// The sequence of <see cref="Extrinsics"/> objects loaded from the specified
        /// YML file. The most current file name is used to load the parameters after
        /// each notification in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Extrinsics> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => CreateExtrinsics());
        }
    }
}
