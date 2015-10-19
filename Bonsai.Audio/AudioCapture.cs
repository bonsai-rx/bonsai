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
            BufferLength = 10;
            Frequency = 44100;

            source = Observable.Create<Mat>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var frequency = Frequency;
                    var bufferLength = BufferLength;
                    var bufferSize = (int)Math.Ceiling(frequency * bufferLength / 1000);
                    var captureInterval = TimeSpan.FromMilliseconds((int)(bufferLength / 2 + 0.5));
                    var captureBufferSize = bufferSize * 4;

                    lock (captureLock)
                    {
                        using (var capture = new OpenTK.Audio.AudioCapture(DeviceName, frequency, ALFormat.Mono16, captureBufferSize))
                        using (var captureSignal = new ManualResetEvent(false))
                        {
                            capture.Start();
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                while (capture.AvailableSamples >= bufferSize)
                                {
                                    var buffer = new Mat(1, bufferSize, Depth.S16, 1);
                                    capture.ReadSamples(buffer.Data, bufferSize);
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

        [Description("The sampling frequency (Hz) used by the audio capture device.")]
        public int Frequency { get; set; }

        [Description("The length of the sample buffer (ms).")]
        public double BufferLength { get; set; }

        public override IObservable<Mat> Generate()
        {
            return source;
        }
    }
}
