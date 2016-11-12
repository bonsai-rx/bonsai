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
    public class AudioPlayback : Sink<Mat>
    {
        public AudioPlayback()
        {
            Frequency = 44100;
        }

        [Description("The name of the output device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName { get; set; }

        [Description("The playback frequency (Hz) to use for input buffers.")]
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

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Using(
                () => AudioManager.ReserveContext(DeviceName),
                context =>
                {
                    var sourceId = AL.GenSource();
                    return source.Do(input =>
                    {
                        var transpose = input.Rows < input.Cols;
                        var channels = transpose ? input.Rows : input.Cols;
                        if (channels > 2)
                        {
                            throw new InvalidOperationException("Unsupported number of channels for the specified output format.");
                        }

                        var format = channels == 2 ? ALFormat.Stereo16 : ALFormat.Mono16;
                        var convertDepth = input.Depth != Depth.S16;
                        if (convertDepth || transpose)
                        {
                            // Convert if needed
                            if (convertDepth)
                            {
                                var temp = new Mat(input.Rows, input.Cols, Depth.S16, 1);
                                CV.Convert(input, temp);
                                input = temp;
                            }

                            // Transpose multichannel to column order
                            if (transpose)
                            {
                                var temp = new Mat(input.Cols, input.Rows, input.Depth, 1);
                                CV.Transpose(input, temp);
                                input = temp;
                            }
                        }

                        var buffer = AL.GenBuffer();
                        AL.BufferData(buffer, format, input.Data, input.Rows * input.Step, Frequency);
                        AL.SourceQueueBuffer(sourceId, buffer);

                        ClearBuffers(sourceId, 0);
                        if (AL.GetSourceState(sourceId) != ALSourceState.Playing)
                        {
                            AL.SourcePlay(sourceId);
                        }
                    }).Finally(() =>
                    {
                        int queuedBuffers;
                        AL.GetSource(sourceId, ALGetSourcei.BuffersQueued, out queuedBuffers);
                        ClearBuffers(sourceId, queuedBuffers);

                        AL.DeleteSource(sourceId);
                    });
                });
        }
    }
}
