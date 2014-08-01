using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using AForge.Video.DirectShow;
using AForge.Video;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Design;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Globalization;

namespace Bonsai.Video
{
    [Description("Produces a sequence of images acquired from a DirectShow based video capture device.")]
    [Editor("Bonsai.Video.Design.VideoCaptureDeviceEditor, Bonsai.Video.Design", typeof(ComponentEditor))]
    public class VideoCaptureDevice : VideoCapture
    {
        [TypeConverter(typeof(IndexConverter))]
        [Description("The index of the video capture device from which to acquire images.")]
        public int Index { get; set; }

        [TypeConverter(typeof(FormatConverter))]
        [Description("The output format of the video capture device.")]
        public VideoFormat Format { get; set; }

        protected override IVideoSource CreateVideoSource()
        {
            var index = Index;
            var deviceFilters = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (index >= deviceFilters.Count)
            {
                throw new InvalidOperationException("The specified video input device is not available.");
            }

            var format = Format;
            var videoSource = new AForge.Video.DirectShow.VideoCaptureDevice(deviceFilters[index].MonikerString);
            if (format != null)
            {
                var videoResolution = Array.Find(videoSource.VideoCapabilities, capabilities => format.Equals(capabilities));
                videoSource.VideoResolution = videoResolution;
            }
            return videoSource;
        }

        class IndexConverter : Int32Converter
        {
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                var text = value as string;
                if (!string.IsNullOrEmpty(text))
                {
                    return int.Parse(text.Split(' ')[0], culture);
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    var index = (int)value;
                    var deviceFilters = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                    if (index >= 0 && index < deviceFilters.Count)
                    {
                        return string.Format(culture, "{0} ({1})", index, deviceFilters[index].Name);
                    }
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var deviceFilters = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                return new StandardValuesCollection(Enumerable.Range(0, deviceFilters.Count).ToArray());
            }
        }

        class FormatConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                {
                    return true;
                }

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                var text = value as string;
                if (text != null)
                {
                    return VideoFormat.Parse(text);
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var deviceInfo = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                var videoCaptureDevice = (VideoCaptureDevice)context.Instance;
                var index = videoCaptureDevice.Index;

                var videoSource = new AForge.Video.DirectShow.VideoCaptureDevice(deviceInfo[index].MonikerString);
                var frameSizes = Array.ConvertAll(videoSource.VideoCapabilities, capabilities => new VideoFormat(capabilities));
                return new StandardValuesCollection(frameSizes);
            }
        }
    }
}
