using Bonsai.Reactive;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that starts playing the frame sequence of a
    /// video texture or texture array.
    /// </summary>
    [Description("Starts playing the frame sequence of a video texture or texture array.")]
    public class PlayTextureSequence : Combinator<ElementIndex<Texture>>
    {
        /// <summary>
        /// Gets or sets the name of the texture sequence.
        /// </summary>
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture sequence.")]
        public string TextureName { get; set; }

        /// <summary>
        /// Gets or sets the rate at which to playback the sequence. A value of
        /// zero means the native frame rate will be used.
        /// </summary>
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The rate at which to playback the sequence. A value of zero means the native frame rate will be used.")]
        public double PlaybackRate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the video should loop when
        /// the end of the file is reached.
        /// </summary>
        [Description("Indicates whether the video should loop when the end of the file is reached.")]
        public bool Loop { get; set; }

        internal static IObservable<ElementIndex<Texture>> Process(ITextureSequence texture, double playbackRate, bool loop)
        {
            playbackRate = playbackRate > 0 ? playbackRate : texture.PlaybackRate;
            var periodTicks = (long)(TimeSpan.TicksPerSecond / playbackRate);
            var timer = new Timer { Period = TimeSpan.FromTicks(periodTicks) };
            return Observable.Using(
                () => texture.GetEnumerator(loop),
                enumerator => timer.Generate().TakeWhile(x => enumerator.MoveNext()).Select(x => enumerator.Current));
        }

        /// <summary>
        /// Generates an observable sequence that starts playing the frames of
        /// a video texture or texture array in order.
        /// </summary>
        /// <returns>
        /// An observable sequence reporting the zero-based index of the frame
        /// which is currently active in the specified texture.
        /// </returns>
        public IObservable<ElementIndex<Texture>> Process()
        {
            return Process(Observable.Return(0));
        }

        /// <summary>
        /// Starts playing the frames of the video texture or texture array in
        /// an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Texture"/> objects for which to start
        /// playing the frames in orer. The texture must be either a video
        /// texture or a texture array.
        /// </param>
        /// <returns>
        /// An observable sequence reporting the zero-based index of the frame
        /// which is currently active in the specified texture.
        /// </returns>
        public IObservable<ElementIndex<Texture>> Process(IObservable<Texture> source)
        {
            return source.Select(input => Process((ITextureSequence)input, PlaybackRate, Loop)).Switch();
        }

        /// <summary>
        /// Starts playing the frames of a video texture or texture array
        /// in order whenever an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to start playing the
        /// video texture or texture array.
        /// </param>
        /// <returns>
        /// An observable sequence reporting the zero-based index of the frame
        /// which is currently active in the specified texture.
        /// </returns>
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
