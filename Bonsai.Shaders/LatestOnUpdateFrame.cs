using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Samples the latest notifications of the input sequence on the update frame event.")]
    public class LatestOnUpdateFrame : Combinator
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var update = updateFrame.Generate();
            return source.CombineLatest(update, (x, evt) => x).Sample(update);
        }
    }
}
