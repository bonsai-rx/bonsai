using System;
using System.Linq;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that performs image segmentation of every frame in
    /// the sequence using an online estimation of the background.
    /// </summary>
    [Description("Performs image segmentation of every frame in the sequence using an online estimation of the background.")]
    public class BackgroundSubtraction : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the number of frames to use for initial background estimation.
        /// </summary>
        [Description("The number of frames to use for initial background estimation.")]
        public int BackgroundFrames { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value determining how fast the online estimation of the
        /// background is adapted.
        /// </summary>
        [Range(0, 1)]
        [Precision(2, .01)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("Determines how fast the online estimation of the background is adapted.")]
        public double AdaptationRate { get; set; }

        /// <summary>
        /// Gets or sets the threshold value used to test whether individual pixels
        /// are foreground or background.
        /// </summary>
        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The threshold value used to test whether individual pixels are foreground or background.")]
        public double ThresholdValue { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the type of threshold to apply to
        /// individual pixels.
        /// </summary>
        [TypeConverter(typeof(ThresholdTypeConverter))]
        [Description("Specifies the type of threshold to apply to individual pixels.")]
        public ThresholdTypes ThresholdType { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the subtraction method used to isolate
        /// foreground pixels.
        /// </summary>
        [Description("Specifies the subtraction method used to isolate foreground pixels.")]
        public SubtractionMethod SubtractionMethod { get; set; }

        class ThresholdTypeConverter : EnumConverter
        {
            public ThresholdTypeConverter(Type type)
                : base(type)
            {
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(
                    base.GetStandardValues(context)
                    .Cast<ThresholdTypes>()
                    .Where(type => type != ThresholdTypes.Otsu)
                    .ToArray());
            }
        }

        /// <summary>
        /// Performs image segmentation of every frame in an observable sequence
        /// using an online estimation of the background.
        /// </summary>
        /// <param name="source">
        /// The sequence of frames on which to perform image segmentation.
        /// </param>
        /// <returns>
        /// The sequence of images which have been segmented into foreground and
        /// background pixels.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                int averageCount = 0;
                IplImage image = null;
                IplImage difference = null;
                IplImage background = null;
                return source.Select(input =>
                {
                    if (background == null || background.Size != input.Size)
                    {
                        averageCount = 0;
                        image = new IplImage(input.Size, IplDepth.F32, input.Channels);
                        difference = new IplImage(input.Size, IplDepth.F32, input.Channels);
                        background = new IplImage(input.Size, IplDepth.F32, input.Channels);
                        background.SetZero();
                    }

                    var output = new IplImage(input.Size, IplDepth.U8, input.Channels);
                    if (averageCount < BackgroundFrames)
                    {
                        averageCount++;
                        output.SetZero();
                        CV.Acc(input, background);
                        if (averageCount == BackgroundFrames)
                        {
                            CV.ConvertScale(background, background, 1.0 / averageCount, 0);
                        }
                    }
                    else
                    {
                        CV.Convert(input, image);
                        switch (SubtractionMethod)
                        {
                            case SubtractionMethod.Bright:
                                CV.Sub(image, background, difference);
                                break;
                            case SubtractionMethod.Dark:
                                CV.Sub(background, image, difference);
                                break;
                            case SubtractionMethod.Absolute:
                            default:
                                CV.AbsDiff(image, background, difference);
                                break;
                        }

                        if (AdaptationRate > 0)
                        {
                            CV.RunningAvg(image, background, AdaptationRate);
                        }

                        CV.Threshold(difference, output, ThresholdValue, 255, ThresholdType);
                    }

                    return output;
                });
            });
        }
    }

    /// <summary>
    /// Specifies the subtraction method used to isolate foreground pixels.
    /// </summary>
    public enum SubtractionMethod
    {
        /// <summary>
        /// Take the absolute difference between the online estimation of the background
        /// and the current frame so that any pixels which are different from the
        /// the background can be considered foreground.
        /// </summary>
        Absolute,

        /// <summary>
        /// Subtract the online estimation of the background from the current image so
        /// that only pixels which are brighter than the background can be classified
        /// as foreground.
        /// </summary>
        Bright,

        /// <summary>
        /// Subtract the current image from the online estimation of the background so
        /// that only pixels which are darker than the background can be classified
        /// as foreground.
        /// </summary>
        Dark
    }
}
