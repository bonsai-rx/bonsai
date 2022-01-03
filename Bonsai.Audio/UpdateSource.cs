using OpenTK;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that updates the properties of an audio source.
    /// </summary>
    [Description("Updates the properties of an audio source.")]
    public class UpdateSource : Sink
    {
        /// <summary>
        /// Gets or sets the name of the audio device used for playback.
        /// </summary>
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        [Description("The name of the audio device used for playback.")]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the audio source to update.
        /// </summary>
        [TypeConverter(typeof(SourceNameConverter))]
        [Description("The name of the audio source to update.")]
        public string SourceName { get; set; }

        /// <summary>
        /// Gets or sets the current location of the audio source in three-dimensional space.
        /// If this property is not set, the location of the audio source will not be updated.
        /// </summary>
        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current location of the audio source in three-dimensional space.")]
        public Vector3? Position { get; set; }

        /// <summary>
        /// Gets or sets the current velocity of the audio source in three-dimensional space.
        /// If this property is not set, the velocity of the audio source will not be updated.
        /// </summary>
        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current velocity of the audio source in three-dimensional space.")]
        public Vector3? Velocity { get; set; }

        /// <summary>
        /// Gets or sets the current direction vector of the audio source. If this property
        /// is not set, the direction of the audio source will not be updated.
        /// </summary>
        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current direction vector of the audio source.")]
        public Vector3? Direction { get; set; }

        private IObservable<TSource> Process<TSource>(IObservable<TSource> source, string sourceName)
        {
            return Observable.Using(
                () => AudioManager.ReserveContext(DeviceName),
                resource =>
                {
                    var audioSource = resource.Context.ResourceManager.Load<AudioSource>(sourceName);
                    return source.Do(input => Update(audioSource));
                });
        }

        /// <summary>
        /// Updates the properties of the specified audio source whenever the source sequence
        /// emits a notification.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to trigger the update of
        /// the audio source.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of updating the properties of the
        /// specified audio source whenever the sequence emits a notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Process(source, SourceName);
        }

        /// <summary>
        /// Updates the properties of all the audio sources in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="AudioSource"/> objects whose properties should be updated.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of updating the properties of each of the
        /// audio sources in the sequence.
        /// </returns>
        /// <remarks>
        /// If <see cref="SourceName"/> is not null or empty, this method behaves as
        /// <see cref="Process{TSource}(IObservable{TSource})"/>.
        /// </remarks>
        public IObservable<AudioSource> Process(IObservable<AudioSource> source)
        {
            var sourceName = SourceName;
            if (!string.IsNullOrEmpty(sourceName)) return Process(source, sourceName);
            else return source.Do(Update);
        }

        void Update(AudioSource source)
        {
            if (TryGetValue(Position, out Vector3 position))
            {
                source.Position = position;
            }

            if (TryGetValue(Velocity, out Vector3 velocity))
            {
                source.Velocity = velocity;
            }

            if (TryGetValue(Direction, out Vector3 direction))
            {
                source.Direction = direction;
            }
        }

        static bool TryGetValue<T>(T? nullable, out T value) where T : struct
        {
            value = nullable.GetValueOrDefault();
            return nullable.HasValue;
        }
    }
}
