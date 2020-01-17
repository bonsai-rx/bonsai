using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    [Description("Performs image segmentation using an online estimation of the background.")]
    public class BackgroundSubtraction : Transform<IplImage, IplImage>
    {
        public BackgroundSubtraction()
        {
            BackgroundFrames = 1;
        }

        [Description("The number of frames to use for initial background estimation.")]
        public int BackgroundFrames { get; set; }

        [Range(0, 1)]
        [Precision(2, .01)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("Determines how fast the online estimation of the background is adapted.")]
        public double AdaptationRate { get; set; }

        [Range(0, 255)]
        [Precision(0, 1)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
        [Description("The threshold value used to test whether individual pixels are foreground or background.")]
        public double ThresholdValue { get; set; }

        [TypeConverter(typeof(ThresholdTypeConverter))]
        [Description("The type of threshold to apply to individual pixels.")]
        public ThresholdTypes ThresholdType { get; set; }

        [Description("Specifies the subtraction method used to isolate foreground pixels.")]
        public SubtractionMethod SubtractionMethod { get; set; }

        class ThresholdTypeConverter : EnumConverter
        {
            public ThresholdTypeConverter(Type type)
                : base(type)
            {
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(
                    base.GetStandardValues(context)
                    .Cast<ThresholdTypes>()
                    .Where(type => type != ThresholdTypes.Otsu)
                    .ToArray());
            }
        }

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

    public enum SubtractionMethod
    {
        Absolute,
        Bright,
        Dark
    }
}
