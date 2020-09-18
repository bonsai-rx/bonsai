using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Starts playing the frame sequence of a video texture or texture array.")]
    public class PlayTextureSequence : Combinator<long>
    {
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture sequence.")]
        public string TextureName { get; set; }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The rate at which to playback the sequence. A value of 0 means the native frame rate will be used.")]
        public double PlaybackRate { get; set; }

        internal static IObservable<long> Process(ITextureSequence texture, double playbackRate)
        {
            playbackRate = playbackRate > 0 ? playbackRate : texture.PlaybackRate;
            var timer = new Timer { Period = TimeSpan.FromSeconds(1.0 / playbackRate) };
            return timer.Generate().TakeWhile(x => texture.MoveNext()).Finally(texture.Reset);
        }

        public IObservable<long> Process()
        {
            return Process(Observable.Return(0));
        }

        public IObservable<long> Process(IObservable<Texture> source)
        {
            return source.Select(input => Process((ITextureSequence)input, PlaybackRate)).Switch();
        }

        public override IObservable<long> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(input =>
            {
                var name = TextureName;
                var playbackRate = PlaybackRate;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A texture name must be specified.");
                }

                return ShaderManager.WindowSource.Take(1).SelectMany(window =>
                {
                    var texture = (ITextureSequence)window.ResourceManager.Load<Texture>(name);
                    return Process(texture, playbackRate);
                });
            }).Switch();
        }
    }
}
