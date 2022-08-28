using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that detects objects on each image in the sequence
    /// using a pre-trained cascade of boosted Haar classifiers.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [Description("Detects objects on each image in the sequence using a pre-trained cascade of boosted Haar classifiers.")]
    public class HaarCascade : Transform<IplImage, Rect[]>
    {
        /// <summary>
        /// Gets or sets the name of the file describing a trained Haar cascade classifier.
        /// </summary>
        [FileNameFilter("XML Files|*.xml|All Files|*.*")]
        [Description("The name of the file describing a trained Haar cascade classifier.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the factor by which the search window is scaled between
        /// subsequent scans.
        /// </summary>
        [Description("The factor by which the search window is scaled between subsequent scans.")]
        public double ScaleFactor { get; set; } = 1.1;

        /// <summary>
        /// Gets or sets the minimum number (minus 1) of neighbor rectangles that make
        /// up an object. All groups with smaller number of rectangles are rejected.
        /// </summary>
        [Description("The minimum number (minus 1) of neighbor rectangles that make up an object. All groups with smaller number of rectangles are rejected.")]
        public int MinNeighbors { get; set; } = 3;

        /// <summary>
        /// Gets or sets a value specifying the optional operation flags for the
        /// Haar cascade classifier.
        /// </summary>
        [Description("Specifies the optional operation flags for the Haar cascade classifier.")]
        public HaarDetectObjectFlags Flags { get; set; } = HaarDetectObjectFlags.None;

        /// <summary>
        /// Gets or sets the optional minimum window size. By default, it is set
        /// to the size specified in the cascade classifier file.
        /// </summary>
        [Description("The optional minimum window size. By default, it is set to the size specified in the cascade classifier file.")]
        public Size MinSize { get; set; }

        /// <summary>
        /// Gets or sets the optional maximum window size. By default, it is set to the total image size.
        /// </summary>
        [Description("The optional maximum window size. By default, it is set to the total image size.")]
        public Size MaxSize { get; set; }

        /// <summary>
        /// Gets or sets the optional offset to apply to individual object rectangles.
        /// </summary>
        [Description("The optional offset to apply to individual object rectangles.")]
        public Point Offset { get; set; }

        /// <summary>
        /// Detects objects on each image in an observable sequence using a pre-trained
        /// cascade of boosted Haar classifiers.
        /// </summary>
        /// <param name="source">
        /// The sequence of images on which to detect objects using the boosted
        /// Haar classifier cascade.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Rect"/> arrays representing the objects which
        /// were detected on each image of the <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<Rect[]> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var storage = new MemStorage();
                var cascade = HaarClassifierCascade.Load(FileName);
                return source.Select(input =>
                {
                    var offset = Offset;
                    var objects = cascade.DetectObjects(input, storage, ScaleFactor, MinNeighbors, Flags, MinSize, MaxSize);
                    var result = new Rect[objects.Count];
                    objects.CopyTo(result);
                    storage.Clear();
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i].X += offset.X;
                        result[i].Y += offset.Y;
                    }
                    return result;
                }).Finally(() =>
                {
                    cascade.Dispose();
                    storage.Dispose();
                });
            });
        }
    }
}
