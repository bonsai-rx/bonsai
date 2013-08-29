using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK;
using System.ComponentModel;
using System.Runtime.InteropServices;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Audio
{
    [Description("Plays the sequence of buffered samples to the specified audio output device.")]
    public class AudioPlayback : Sink<CvMat>
    {
        public AudioPlayback()
        {
            Frequency = 44100;
        }

        [Description("The name of the output device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName { get; set; }

        [Description("The playback frequency (Hz) used by the output device.")]
        public int Frequency { get; set; }

        static void ClearBuffers(int source, int input)
        {
            int[] freeBuffers;
            if (input == 0)
            {
                int processedBuffers;
                AL.GetSource(source, ALGetSourcei.BuffersProcessed, out processedBuffers);
                if (processedBuffers == 0)
                    return;

                freeBuffers = AL.SourceUnqueueBuffers(source, processedBuffers);
            }
            else
            {
                freeBuffers = AL.SourceUnqueueBuffers(source, input);
            }

            AL.DeleteBuffers(freeBuffers);
        }

        public override IObservable<CvMat> Process(IObservable<CvMat> source)
        {
            return Observable.Defer(() =>
            {
                var context = new AudioContext(DeviceName);
                var sourceId = AL.GenSource();
                return source.Do(input =>
                {
                    var buffer = AL.GenBuffer();
                    AL.BufferData(buffer, ALFormat.Mono16, input.Data, input.Rows * input.Step, Frequency);

                    AL.SourceQueueBuffer(sourceId, buffer);
                    if (AL.GetSourceState(sourceId) != ALSourceState.Playing)
                    {
                        AL.SourcePlay(sourceId);
                    }

                    ClearBuffers(sourceId,0);
                }).Finally(() =>
                {
                    int queuedBuffers;
                    AL.GetSource(sourceId, ALGetSourcei.BuffersQueued, out queuedBuffers);
                    ClearBuffers(sourceId, queuedBuffers);

                    AL.DeleteSource(sourceId);
                    context.Dispose();
                });
            });
        }
    }
}
