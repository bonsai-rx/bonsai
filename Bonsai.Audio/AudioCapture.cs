using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Audio.OpenAL;
using System.ComponentModel;
using System.Threading;
using OpenTK;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive;
using System.Runtime.InteropServices;
using OpenCV.Net;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    [Description("Produces a sequence of buffered samples acquired from the specified audio capture device.")]
    public class AudioCapture : Source<Mat>
    {
        IObservable<Mat> source;
        readonly object captureLock = new object();

        public AudioCapture()
        {
            SampleFormat = ALFormat.Mono16;
            BufferLength = 10;
            SampleRate = 44100;

            source = Observable.Create<Mat>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var sampleRate = SampleRate;
                    var bufferLength = BufferLength;
                    var sampleFormat = SampleFormat;
                    var channelCount = SampleFormat == ALFormat.Stereo16 ? 2 : 1;
                    var bufferSize = (int)Math.Ceiling(sampleRate * bufferLength / 1000);
                    var readBuffer = sampleFormat == ALFormat.Stereo16 ? new Mat(bufferSize, channelCount, Depth.S16, 1) : null;
                    var captureInterval = TimeSpan.FromMilliseconds((int)(bufferLength / 2 + 0.5));
                    var captureBufferSize = bufferSize * 4;

                    lock (captureLock)
                    {
                        using (var capture = new OpenTK.Audio.AudioCapture(DeviceName, sampleRate, sampleFormat, captureBufferSize))
                        using (var captureSignal = new ManualResetEvent(false))
                        {
                            capture.Start();
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                while (capture.AvailableSamples >= bufferSize)
                                {
                                    var buffer = new Mat(channelCount, bufferSize, Depth.S16, 1);
                                    if (readBuffer != null)
                                    {
                                        capture.ReadSamples(readBuffer.Data, bufferSize);
                                        CV.Transpose(readBuffer, buffer);
                                    }
                                    else capture.ReadSamples(buffer.Data, bufferSize);
                                    observer.OnNext(buffer);
                                }

                                captureSignal.WaitOne(captureInterval);
                            }
                            capture.Stop();
                        }
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            })
            .PublishReconnectable()
            .RefCount();
        }

        [Description("The name of the capture device from which to acquire samples.")]
        [TypeConverter(typeof(CaptureDeviceNameConverter))]
        public string DeviceName { get; set; }

        [Description("The sample rate used by the audio capture device, in Hz.")]
        public int SampleRate { get; set; }

        [Browsable(false)]
        public int? Frequency
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    SampleRate = value.Value;
                }
            }
        }

        [Browsable(false)]
        public bool FrequencySpecified
        {
            get { return Frequency.HasValue; }
        }

        [TypeConverter(typeof(SampleFormatConverter))]
        [Description("The requested capture buffer format.")]
        public ALFormat SampleFormat { get; set; }

        [Description("The length of the capture buffer (ms).")]
        public double BufferLength { get; set; }

        public override IObservable<Mat> Generate()
        {
            return source;
        }

        class SampleFormatConverter : EnumConverter
        {
            public SampleFormatConverter(Type type)
                : base(type)
            {
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
                return new StandardValuesCollection(new[] { ALFormat.Mono16, ALFormat.Stereo16 });
            }
        }
    }
}
