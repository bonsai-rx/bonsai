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
            BufferLength = 441;
            Frequency = 44100;
        }

        [Description("The name of the capture device from which to acquire samples.")]
        [TypeConverter(typeof(CaptureDeviceNameConverter))]
        public string DeviceName { get; set; }

        [Description("The sampling frequency (Hz) used by the audio capture device.")]
        public int Frequency { get; set; }

        [Description("The length of the output buffer in samples.")]
        public int BufferLength { get; set; }

        public override IDisposable Load()
        {
            capture = new OpenTK.Audio.AudioCapture(DeviceName, Frequency, ALFormat.Mono16, BufferLength * 2);
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
                    var bufferLength = BufferLength;
                    return HighResolutionScheduler.Default.Schedule<int>((int)(1000.0 * bufferLength / (2 * Frequency)), TimeSpan.Zero, (interval, self) =>
                    {
                        while (capture.AvailableSamples > bufferLength)
                        {
                            var buffer = new CvMat(1, bufferLength, CvMatDepth.CV_16S, 1);
                            capture.ReadSamples(buffer.Data, bufferLength);
                            observer.OnNext(buffer);
                        }
                        self(interval, TimeSpan.FromMilliseconds(interval));
                    });
                }));
        }
    }
}
