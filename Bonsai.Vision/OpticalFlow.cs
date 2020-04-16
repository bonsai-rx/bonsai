using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [Description("Computes dense optical flow using Gunnar Farneback’s algorithm.")]
    public class OpticalFlow : Transform<Tuple<IplImage, IplImage>, IplImage>
    {
        public OpticalFlow()
        {
            PyramidScale = 0.5;
            Levels = 3;
            WindowSize = 12;
            Iterations = 3;
            PolyN = 5;
            PolySigma = 1.1;
            Flags = FarnebackFlowFlags.None;
        }

        [Range(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("Specifies the image scale (less than 1) to build the pyramids for each image.")]
        public double PyramidScale { get; set; }

        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The number of pyramid layers, including the initial image.")]
        public int Levels { get; set; }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The averaging window size. Larger values increase robustness to noise and fast motion, but yield a blurred motion field.")]
        public int WindowSize { get; set; }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The number of iterations the algorithm does at each pyramid level.")]
        public int Iterations { get; set; }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("Size of the pixel neighborhood used to find polynomial expansion in each pixel.")]
        public int PolyN { get; set; }

        [Precision(2, 0.01)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("Standard deviation of the Gaussian used to smooth derivatives used as a basis for the polynomial expansion.")]
        public double PolySigma { get; set; }

        [Description("The operation flags for the optical flow algorithm.")]
        public FarnebackFlowFlags Flags { get; set; }

        public IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Process(source.Publish(ps => ps.Zip(ps.Skip(1), (previous, next) => Tuple.Create(previous, next))));
        }

        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return source.Select(input =>
            {
                var previous = input.Item1;
                var next = input.Item2;
                var output = new IplImage(previous.Size, IplDepth.F32, 2);
                CV.CalcOpticalFlowFarneback(
                    previous,
                    next,
                    output,
                    PyramidScale,
                    Levels,
                    WindowSize,
                    Iterations,
                    PolyN,
                    PolySigma,
                    Flags);
                return output;
            });
        }
    }
}
