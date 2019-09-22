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

        [TypeConverter(typeof(SourceNameConverter))]
        [Description("The optional name of the source used to playback the input buffers.")]
        public string SourceName { get; set; }

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
            return createSource.Generate().SelectMany(audioSource =>
                Process(Observable.Return(audioSource), source).Finally(audioSource.Dispose));
        }

        public IObservable<Mat> Process(IObservable<Mat> dataSource, IObservable<AudioSource> audioSource)
        {
            return Process(audioSource, dataSource);
        }

        public IObservable<Mat> Process(IObservable<AudioSource> audioSource, IObservable<Mat> dataSource)
        {
            return Observable.Using(
                () => AudioManager.ReserveContext(DeviceName),
                resource => audioSource.SelectMany(source => dataSource.Do(input =>
                {
                    var buffer = AL.GenBuffer();
                    BufferHelper.UpdateBuffer(buffer, input, SampleRate);
                    AL.SourceQueueBuffer(source.Id, buffer);

                    source.ClearBuffers(0);
                    if (AL.GetSourceState(source.Id) != ALSourceState.Playing)
                    {
                        AL.SourcePlay(source.Id);
                    }
                })));
        }
    }
}
