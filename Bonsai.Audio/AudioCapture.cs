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

namespace Bonsai.Audio
{
    [Description("Produces a sequence of buffered samples acquired from the specified audio capture device.")]
    public class AudioCapture : Source<CvMat>
    {
        OpenTK.Audio.AudioCapture capture;

        public AudioCapture()
        {
            BufferLength = 10;
            Frequency = 44100;
        }

        [Description("The name of the capture device from which to acquire samples.")]
        [TypeConverter(typeof(CaptureDeviceNameConverter))]
        public string DeviceName { get; set; }

        [Description("The sampling frequency (Hz) used by the audio capture device.")]
        public int Frequency { get; set; }

        [Description("The length of the sample buffer (ms).")]
        public double BufferLength { get; set; }

        public override IDisposable Load()
        {
            var bufferSize = (int)(BufferLength * Frequency * 0.001 / BlittableValueType.StrideOf(short.MinValue));
            capture = new OpenTK.Audio.AudioCapture(DeviceName, Frequency, ALFormat.Mono16, bufferSize);
            return base.Load();
        }

        protected override void Unload()
        {
            capture.Dispose();
            base.Unload();
        }

        protected override IObservable<CvMat> Generate()
        {
            return Observable.Using(
                () =>
                {
                    capture.Start();
                    return Disposable.Create(capture.Stop);
                },
                resource => Observable.Create<CvMat>(observer =>
                {
                    return HighResolutionScheduler.Default.Schedule<int>((int)(BufferLength / 2 + 0.5), TimeSpan.Zero, (interval, self) =>
                    {
                        int availableSamples = capture.AvailableSamples;
                        if(availableSamples > 0)
                        {
                            var buffer = new CvMat(1, availableSamples, CvMatDepth.CV_16S, 1);
                            capture.ReadSamples(buffer.Data, availableSamples);
                            observer.OnNext(buffer);
                        }
                        self(interval, TimeSpan.FromMilliseconds(interval));
                    });
                }));
        }
    }
}
