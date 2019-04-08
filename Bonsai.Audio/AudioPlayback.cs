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
        readonly CreateSource createSource = new CreateSource();

        public AudioPlayback()
        {
            SampleRate = 44100;
        }

        [Description("The name of the output device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName
        {
            get { return createSource.DeviceName; }
            set { createSource.DeviceName = value; }
        }

        [Description("The sample rate, in Hz, used to playback the input buffers.")]
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

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Publish(ps => Process(createSource.Generate().TakeUntil(ps.LastAsync()), ps));
        }

        public IObservable<Mat> Process(IObservable<Mat> dataSource, IObservable<AudioSource> audioSource)
        {
            return Process(audioSource, dataSource);
        }

        public IObservable<Mat> Process(IObservable<AudioSource> audioSource, IObservable<Mat> dataSource)
        {
            return audioSource.SelectMany(source =>
                dataSource.Do(input =>
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
                    AL.BufferData(buffer, format, input.Data, input.Rows * input.Step, SampleRate);
                    AL.SourceQueueBuffer(source.Id, buffer);

                    source.ClearBuffers(0);
                    if (AL.GetSourceState(source.Id) != ALSourceState.Playing)
                    {
                        AL.SourcePlay(source.Id);
                    }
                }));
        }
    }
}
