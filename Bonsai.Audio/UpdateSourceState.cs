using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    public abstract class UpdateSourceState : Sink
    {
        internal UpdateSourceState()
        {
        }

        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        [Description("The name of the output device used for playback.")]
        public string DeviceName { get; set; }

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

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Process(source, SourceName);
        }

        public IObservable<AudioSource> Process(IObservable<AudioSource> source)
        {
            var sourceName = SourceName;
            if (sourceName != null && sourceName.Length > 0) return Process(source, sourceName);
            else return source.Do(input => Update(new[] { input.Id }));
        }

        public IObservable<AudioSource[]> Process(IObservable<AudioSource[]> source)
        {
            var sourceName = SourceName;
            if (sourceName != null && sourceName.Length > 0) return Process(source, sourceName);
            else return source.Do(input => Update(Array.ConvertAll(input, audioSource => audioSource.Id)));
        }

        internal abstract void Update(int[] sources);
    }
}
