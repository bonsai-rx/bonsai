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
            Format = ALFormat.Mono16;
        }

        [Description("The name of the output device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName { get; set; }

        [Description("The playback frequency (Hz) used by the output device.")]
        public int Frequency { get; set; }

        [TypeConverter(typeof(FormatConverter))]
        [Description("The format of the data buffered to the output device.")]
        public ALFormat Format { get; set; }

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
            return Observable.Defer(() =>
            {
                var context = new AudioContext(DeviceName);
                var sourceId = AL.GenSource();
                return source.Do(input =>
                {
                    var format = Format;
                    var targetDepth = (int)format % 2 == 0 ? Depth.S8 : Depth.S16;
                    var targetChannels = format > ALFormat.Mono16 ? 2 : 1;
                    var validChannels = input.Rows == targetChannels || input.Cols == targetChannels;
                    if (!validChannels)
                    {
                        throw new InvalidOperationException("Unsupported number of channels for the specified output format.");
                    }

                    var transpose = input.Rows > 1 && input.Rows == targetChannels ? true : false;
                    var convertDepth = input.Depth != targetDepth;
                    if (convertDepth || transpose)
                    {
                        // Convert if needed
                        if (convertDepth)
                        {
                            var temp = new Mat(input.Rows, input.Cols, targetDepth, 1);
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
                    if (AL.GetSourceState(sourceId) != ALSourceState.Playing)
                    {
                        AL.SourcePlay(sourceId);
                    }

                    ClearBuffers(sourceId, 0);
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

        class FormatConverter : EnumConverter
        {
            public FormatConverter(Type type)
                : base(type)
            {
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[]
                {
                    ALFormat.Mono8,
                    ALFormat.Mono16,
                    ALFormat.Stereo8,
                    ALFormat.Stereo16
                });
            }
        }
    }
}
