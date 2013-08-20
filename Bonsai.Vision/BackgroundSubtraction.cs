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
        [Editor(DesignTypes.TrackbarEditor, typeof(UITypeEditor))]
        public double ThresholdValue { get; set; }

        [TypeConverter(typeof(ThresholdTypeConverter))]
        public ThresholdType ThresholdType { get; set; }

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
                    .Cast<ThresholdType>()
                    .Where(type => type != ThresholdType.Otsu)
                    .ToArray());
            }
        }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Create<IplImage>(observer =>
            {
                IplImage image = null;
                IplImage difference = null;
                IplImage background = null;
                int averageCount = 0;

                var process = source.Select(input =>
                {
                    if (averageCount == 0)
                    {
                        image = new IplImage(input.Size, 32, input.NumChannels);
                        difference = new IplImage(input.Size, 32, input.NumChannels);
                        background = new IplImage(input.Size, 32, input.NumChannels);
                        background.SetZero();
                    }

                    var output = new IplImage(input.Size, 8, input.NumChannels);
                    if (averageCount < BackgroundFrames)
                    {
                        averageCount++;
                        output.SetZero();
                        ImgProc.cvAcc(input, background, CvArr.Null);
                        if (averageCount == BackgroundFrames)
                        {
                            Core.cvConvertScale(background, background, 1.0 / averageCount, 0);
                        }
                    }
                    else
                    {
                        Core.cvConvert(input, image);
                        switch (SubtractionMethod)
                        {
                            case SubtractionMethod.Bright:
                                Core.cvSub(image, background, difference, CvArr.Null);
                                break;
                            case SubtractionMethod.Dark:
                                Core.cvSub(background, image, difference, CvArr.Null);
                                break;
                            case SubtractionMethod.Absolute:
                            default:
                                Core.cvAbsDiff(image, background, difference);
                                break;
                        }

                        if (AdaptationRate > 0)
                        {
                            ImgProc.cvRunningAvg(image, background, AdaptationRate, CvArr.Null);
                        }

                        ImgProc.cvThreshold(difference, output, ThresholdValue, 255, ThresholdType);
                    }

                    return output;
                }).Subscribe(observer);

                var close = Disposable.Create(() =>
                {
                    averageCount = 0;
                    if (background != null)
                    {
                        image.Close();
                        difference.Close();
                        background.Close();
                    }
                });

                return new CompositeDisposable(process, close);
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
