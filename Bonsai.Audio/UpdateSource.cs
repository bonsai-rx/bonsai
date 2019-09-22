using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    [Description("Updates the properties of an audio source.")]
    public class UpdateSource : Sink
    {
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        [Description("The name of the output device used for playback.")]
        public string DeviceName { get; set; }

        [TypeConverter(typeof(SourceNameConverter))]
        [Description("The name of the audio source to update.")]
        public string SourceName { get; set; }

        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current location of the audio source in three-dimensional space.")]
        public Vector3? Position { get; set; }

        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current velocity of the audio source in three-dimensional space.")]
        public Vector3? Velocity { get; set; }

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

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Process(source, SourceName);
        }

        public IObservable<AudioSource> Process(IObservable<AudioSource> source)
        {
            var sourceName = SourceName;
            if (!string.IsNullOrEmpty(sourceName)) return Process(source, sourceName);
            else return source.Do(Update);
        }

        void Update(AudioSource source)
        {
            Vector3 position;
            if (TryGetValue(Position, out position))
            {
                source.Position = position;
            }

            Vector3 velocity;
            if (TryGetValue(Velocity, out velocity))
            {
                source.Velocity = velocity;
            }

            Vector3 direction;
            if (TryGetValue(Direction, out direction))
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
