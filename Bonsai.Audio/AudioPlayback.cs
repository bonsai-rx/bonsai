using System;
using System.Linq;
using OpenTK.Audio.OpenAL;
using System.ComponentModel;
using OpenCV.Net;
using System.Reactive.Linq;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that plays a sequence of buffered samples to the specified audio device.
    /// </summary>
    [Description("Plays the sequence of buffered samples to the specified audio device.")]
    public class AudioPlayback : Sink<Mat>
    {
        readonly CreateSource createSource = new CreateSource();

        /// <summary>
        /// Gets or sets the name of the audio device used for playback.
        /// </summary>
        [Description("The name of the audio device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName
        {
            get { return createSource.DeviceName; }
            set { createSource.DeviceName = value; }
        }

        /// <summary>
        /// Gets or sets the optional name of the source used to playback the audio buffers.
        /// </summary>
        [TypeConverter(typeof(SourceNameConverter))]
        [Description("The optional name of the source used to playback the audio buffers.")]
        public string SourceName { get; set; }

        /// <summary>
        /// Gets or sets the sample rate, in Hz, used to playback the audio buffers.
        /// </summary>
        [Description("The sample rate, in Hz, used to playback the audio buffers.")]
        public int SampleRate { get; set; } = 44100;

        /// <summary>
        /// Gets or sets the sample rate, in Hz, used to playback the audio buffers.
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
        /// Plays an observable sequence of buffered samples to the specified audio device.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> objects representing the buffered audio samples
        /// to queue for playback on the specified audio device.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of queueing the audio buffers for
        /// playback on the specified audio device.
        /// </returns>
        /// <remarks>
        /// This operator only subscribes to the <paramref name="source"/> sequence after
        /// initializing the audio context on the specified audio device.
        /// </remarks>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return createSource.Generate().SelectMany(audioSource =>
                Process(source, Observable.Return(audioSource)).Finally(audioSource.Dispose));
        }

        /// <summary>
        /// Plays an observable sequence of buffered samples to all the specified audio sources.
        /// </summary>
        /// <param name="dataSource">
        /// A sequence of <see cref="Mat"/> objects representing the buffered audio samples
        /// to queue for playback on all the active audio sources.
        /// </param>
        /// <param name="audioSource">
        /// A sequence of <see cref="AudioSource"/> objects on which to queue the buffered
        /// audio samples for playback.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="dataSource"/>
        /// sequence but where there is an additional side effect of queueing the audio buffers
        /// for playback on all the active audio sources.
        /// </returns>
        /// <remarks>
        /// This operator only subscribes to the <paramref name="dataSource"/> sequence
        /// after initializing the audio context on the specified audio device.
        /// </remarks>
        public IObservable<Mat> Process(IObservable<Mat> dataSource, IObservable<AudioSource> audioSource)
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

        /// <summary>
        /// Plays an observable sequence of buffered samples to all the specified audio sources.
        /// </summary>
        /// <param name="audioSource">
        /// A sequence of <see cref="AudioSource"/> objects on which to queue the buffered
        /// audio samples for playback.
        /// </param>
        /// <param name="dataSource">
        /// A sequence of <see cref="Mat"/> objects representing the buffered audio samples
        /// to queue for playback on all the active audio sources.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="dataSource"/>
        /// sequence but where there is an additional side effect of queueing the audio buffers
        /// for playback on all the active audio sources.
        /// </returns>
        /// <remarks>
        /// This operator only subscribes to the <paramref name="dataSource"/> sequence
        /// after initializing the audio context on the specified audio device.
        /// </remarks>
        [Obsolete]
        public IObservable<Mat> Process(IObservable<AudioSource> audioSource, IObservable<Mat> dataSource)
        {
            return Process(dataSource, audioSource);
        }
    }
}
