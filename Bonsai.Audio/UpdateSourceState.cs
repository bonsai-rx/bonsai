using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Audio
{
    /// <summary>
    /// Provides an abstract base class for operators that update the state of
    /// specified audio sources.
    /// </summary>
    public abstract class UpdateSourceState : Sink
    {
        internal UpdateSourceState()
        {
        }

        /// <summary>
        /// Gets or sets the name of the audio device used for playback.
        /// </summary>
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        [Description("The name of the audio device used for playback.")]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the audio source, or a comma-separated list
        /// of names if specifying multiple sources.
        /// </summary>
        [TypeConverter(typeof(SourceNameArrayConverter))]
        [Description("The name of the audio source, or a comma-separated list of names if specifying multiple sources.")]
        public string[] SourceName { get; set; }

        private IObservable<TSource> Process<TSource>(IObservable<TSource> source, string[] sourceName)
        {
            return Observable.Using(
                () => AudioManager.ReserveContext(DeviceName),
                resource =>
                {
                    var resourceManager = resource.Context.ResourceManager;
                    if (sourceName == null || sourceName.Length == 0) return source;
                    var audioSources = Array.ConvertAll(sourceName, name => resourceManager.Load<AudioSource>(name).Id);
                    return source.Do(input => Update(audioSources));
                });
        }

        /// <summary>
        /// Updates the state of the specified audio source whenever the source sequence
        /// emits a notification.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to trigger the update of
        /// the audio source.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of updating the state of the specified
        /// audio source whenever the sequence emits a new notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Process(source, SourceName);
        }

        /// <summary>
        /// Updates the state of all the audio sources in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="AudioSource"/> objects whose state should be updated.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of updating the state of each of the
        /// audio sources in the sequence.
        /// </returns>
        /// <remarks>
        /// If <see cref="SourceName"/> is not null or empty, this method behaves as
        /// <see cref="Process{TSource}(IObservable{TSource})"/>.
        /// </remarks>
        public IObservable<AudioSource> Process(IObservable<AudioSource> source)
        {
            var sourceName = SourceName;
            if (sourceName != null && sourceName.Length > 0) return Process(source, sourceName);
            else return source.Do(input => Update(new[] { input.Id }));
        }

        /// <summary>
        /// Updates the state of all the audio sources in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="AudioSource"/> arrays containing all the audio sources
        /// whose state should be updated.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of updating the state of all the
        /// audio sources.
        /// </returns>
        /// <remarks>
        /// If <see cref="SourceName"/> is not null or empty, this method behaves as
        /// <see cref="Process{TSource}(IObservable{TSource})"/>.
        /// </remarks>
        public IObservable<AudioSource[]> Process(IObservable<AudioSource[]> source)
        {
            var sourceName = SourceName;
            if (sourceName != null && sourceName.Length > 0) return Process(source, sourceName);
            else return source.Do(input => Update(Array.ConvertAll(input, audioSource => audioSource.Id)));
        }

        internal abstract void Update(int[] sources);
    }
}
