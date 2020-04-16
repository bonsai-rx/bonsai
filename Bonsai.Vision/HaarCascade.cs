using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [DefaultProperty("FileName")]
    [Description("Detects objects in the input image using a pre-trained cascade of boosted Haar classifiers.")]
    public class HaarCascade : Transform<IplImage, Rect[]>
    {
        public HaarCascade()
        {
            ScaleFactor = 1.1;
            MinNeighbors = 3;
            Flags = HaarDetectObjectFlags.None;
        }

        [FileNameFilter("XML Files|*.xml|All Files|*.*")]
        [Description("The name of the file describing a trained Haar cascade classifier.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        [Description("The factor by which the search window is scaled between subsequent scans.")]
        public double ScaleFactor { get; set; }

        [Description("The minimum number (minus 1) of neighbor rectangles that make up an object. All groups with smaller number of rectangles are rejected.")]
        public int MinNeighbors { get; set; }

        [Description("The optional operation flags for the Haar cascade classifier.")]
        public HaarDetectObjectFlags Flags { get; set; }

        [Description("The optional minimum window size. By default, it is set to the size specified in the cascade classifier file.")]
        public Size MinSize { get; set; }

        [Description("The optional maximum window size. By default, it is set to the total image size.")]
        public Size MaxSize { get; set; }

        [Description("The optional offset to apply to individual object rectangles.")]
        public Point Offset { get; set; }

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
