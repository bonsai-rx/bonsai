using Bonsai.Reactive;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Starts playing the frame sequence of a video texture or texture array.")]
    public class PlayTextureSequence : Combinator<ElementIndex<Texture>>
    {
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture sequence.")]
        public string TextureName { get; set; }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The rate at which to playback the sequence. A value of 0 means the native frame rate will be used.")]
        public double PlaybackRate { get; set; }

        [Description("Indicates whether the video should loop when the end of the file is reached.")]
        public bool Loop { get; set; }

        internal static IObservable<ElementIndex<Texture>> Process(ITextureSequence texture, double playbackRate, bool loop)
        {
            playbackRate = playbackRate > 0 ? playbackRate : texture.PlaybackRate;
            var timer = new Timer { Period = TimeSpan.FromSeconds(1.0 / playbackRate) };
            return Observable.Using(
                () => texture.GetEnumerator(loop),
                enumerator => timer.Generate().TakeWhile(x => enumerator.MoveNext()).Select(x => enumerator.Current));
        }

        public IObservable<ElementIndex<Texture>> Process()
        {
            return Process(Observable.Return(0));
        }

        public IObservable<ElementIndex<Texture>> Process(IObservable<Texture> source)
        {
            return source.Select(input => Process((ITextureSequence)input, PlaybackRate, Loop)).Switch();
        }

        public override IObservable<ElementIndex<Texture>> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(input =>
            {
                var name = TextureName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A texture name must be specified.");
                }

                var loop = Loop;
                var playbackRate = PlaybackRate;
                return ShaderManager.WindowSource.Take(1).SelectMany(window =>
                {
                    var texture = (ITextureSequence)window.ResourceManager.Load<Texture>(name);
                    return Process(texture, playbackRate, loop);
                });
            }).Switch();
        }
    }
}
