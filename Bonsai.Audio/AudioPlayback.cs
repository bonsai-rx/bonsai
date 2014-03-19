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

        static Depth GetReducedDepth(Depth depth)
        {
            switch (depth)
            {
                case Depth.F64:
                    return Depth.F64;
                default:
                case Depth.U8:
                case Depth.S8:
                case Depth.U16:
                case Depth.S16:
                case Depth.S32:
                case Depth.F32:
                    return Depth.F32;
            }
        }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                Mat temp = null;
                Mat reduced = null;
                Mat transposed = null;
                var context = new AudioContext(DeviceName);
                var sourceId = AL.GenSource();
                return source.Do(input =>
                {
                    var convertDepth = input.Depth != Depth.S16;
                    var multiChannel = input.Rows > 1 && input.Cols > 1;
                    if (convertDepth || multiChannel)
                    {
                        var rows = multiChannel ? input.Cols : input.Rows;
                        var cols = multiChannel ? 1 : input.Cols;
                        if (temp == null || temp.Rows != rows || temp.Cols != cols)
                        {
                            temp = new Mat(rows, cols, Depth.S16, 1);
                            transposed = temp;
                            reduced = null;
                        }

                        var reducedDepth = multiChannel ? GetReducedDepth(input.Depth) : input.Depth;
                        if (multiChannel && (reduced == null || reduced.Depth != reducedDepth))
                        {
                            reduced = new Mat(1, input.Cols, reducedDepth, 1);
                        }

                        if (multiChannel &&
                           (transposed.Depth != reducedDepth ||
                            transposed.Cols != input.Rows))
                        {
                            transposed = new Mat(rows, cols, reducedDepth, 1);
                        }

                        if (multiChannel)
                        {
                            // Reduce multichannel
                            CV.Reduce(input, reduced, 0, ReduceOperation.Avg);
                            multiChannel = false;
                            input = reduced;

                            // Transpose multichannel to column order
                            CV.Transpose(input, transposed);
                            input = transposed;
                        }

                        // Convert if needed
                        if (input.Depth != temp.Depth)
                        {
                            CV.Convert(input, temp);
                        }

                        input = temp;
                    }

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
