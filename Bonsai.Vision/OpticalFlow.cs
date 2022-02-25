using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that computes dense optical flow using Gunnar Farneback’s
    /// algorithm across all images in the sequence.
    /// </summary>
    [Description("Computes dense optical flow using Gunnar Farneback’s algorithm across all images in the sequence.")]
    public class OpticalFlow : Transform<Tuple<IplImage, IplImage>, IplImage>
    {
        /// <summary>
        /// Gets or sets a value specifying the image scale (less than 1) used to
        /// build the pyramids for each image.
        /// </summary>
        [Range(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("Specifies the image scale (less than 1) to build the pyramids for each image.")]
        public double PyramidScale { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets the number of pyramid layers, including the initial image.
        /// </summary>
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The number of pyramid layers, including the initial image.")]
        public int Levels { get; set; } = 3;

        /// <summary>
        /// Gets or sets the averaging window size. Larger values increase robustness
        /// to noise and fast motion, but yield a blurred motion field.
        /// </summary>
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The averaging window size. Larger values increase robustness to noise and fast motion, but yield a blurred motion field.")]
        public int WindowSize { get; set; } = 12;

        /// <summary>
        /// Gets or sets the number of iterations of the algorithm at each pyramid level.
        /// </summary>
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The number of iterations of the algorithm at each pyramid level.")]
        public int Iterations { get; set; } = 3;

        /// <summary>
        /// Gets or sets the size of the pixel neighborhood used to find polynomial
        /// expansion in each pixel.
        /// </summary>
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The size of the pixel neighborhood used to find polynomial expansion in each pixel.")]
        public int PolyN { get; set; } = 5;

        /// <summary>
        /// Gets or sets the standard deviation of the Gaussian used to smooth the
        /// derivatives used as a basis for the polynomial expansion.
        /// </summary>
        [Precision(2, 0.01)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The standard deviation of the Gaussian used to smooth the derivatives used as a basis for the polynomial expansion.")]
        public double PolySigma { get; set; } = 1.1;

        /// <summary>
        /// Gets or sets a value specifying the operation flags for the optical
        /// flow algorithm.
        /// </summary>
        [Description("Specifies the operation flags for the optical flow algorithm.")]
        public FarnebackFlowFlags Flags { get; set; } = FarnebackFlowFlags.None;

        /// <summary>
        /// Computes dense optical flow using Gunnar Farneback’s algorithm across
        /// all images in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images across which to compute dense optical flow.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the flow field
        /// between the current and the previous image in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Process(source.Publish(ps => ps.Zip(ps.Skip(1), (previous, next) => Tuple.Create(previous, next))));
        }

        /// <summary>
        /// Computes dense optical flow using Gunnar Farneback’s algorithm across
        /// all image pairs in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of image pairs across which to compute the dense optical flow.
        /// The optical flow is computed from the first to the second image in the pair.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the flow field
        /// between the first and the second image of each pair in the <paramref name="source"/>
        /// sequence.
        /// </returns>
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
