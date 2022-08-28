using System;
using OpenTK.Audio.OpenAL;
using System.ComponentModel;
using System.Threading;
using System.Reactive.Linq;
using OpenCV.Net;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that generates a sequence of buffered samples acquired from the
    /// specified audio capture device.
    /// </summary>
    [Description("Generates a sequence of buffered samples acquired from the specified audio capture device.")]
    public class AudioCapture : Source<Mat>
    {
        readonly IObservable<Mat> source;
        readonly object captureLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioCapture"/> class.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the name of the capture device from which to acquire samples.
        /// </summary>
        [Description("The name of the capture device from which to acquire samples.")]
        [TypeConverter(typeof(CaptureDeviceNameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the sample rate used by the audio capture device, in Hz.
        /// </summary>
        [Description("The sample rate used by the audio capture device, in Hz.")]
        public int SampleRate { get; set; }

        /// <summary>
        /// Gets or sets the sample rate used by the audio capture device, in Hz.
        /// </summary>
        [Browsable(false)]
        [Obsolete("Use SampleRate instead for consistent wording with signal processing operator properties.")]
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

        /// <summary>
        /// Gets a value indicating whether the <see cref="Frequency"/> property should be serialized.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        public bool FrequencySpecified
        {
            get { return Frequency.HasValue; }
        }

        /// <summary>
        /// Gets or sets the format of capture buffer samples.
        /// </summary>
        [TypeConverter(typeof(SampleFormatConverter))]
        [Description("The format of capture buffer samples.")]
        public ALFormat SampleFormat { get; set; }

        /// <summary>
        /// Gets or sets the length of the capture buffer, in milliseconds.
        /// </summary>
        [Description("The length of the capture buffer, in milliseconds.")]
        public double BufferLength { get; set; }

        /// <summary>
        /// Generates an observable sequence of buffered audio samples acquired
        /// from the specified audio capture device.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing audio capture
        /// buffers of a fixed length. See <see cref="BufferLength"/>.
        /// </returns>
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

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] { ALFormat.Mono16, ALFormat.Stereo16 });
            }
        }
    }
}
