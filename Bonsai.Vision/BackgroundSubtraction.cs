using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class BackgroundSubtraction : Transform<IplImage, IplImage>
    {
        public BackgroundSubtraction()
        {
            BackgroundFrames = 1;
        }

        public int BackgroundFrames { get; set; }

        [Range(0, 1)]
        [Precision(2, .01)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public double AdaptationRate { get; set; }

        [Range(0, 255)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        [TypeConverter(typeof(ThresholdTypeConverter))]
        public ThresholdTypes ThresholdType { get; set; }

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
                IplImage image = null;
                IplImage difference = null;
                IplImage background = null;
                int averageCount = 0;
                return source.Select(input =>
                {
                    if (averageCount == 0)
                    {
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
