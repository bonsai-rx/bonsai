using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Samples notifications of the input sequence whenever it is time to update a frame.")]
    public class SampleOnUpdateFrame : Combinator
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var update = updateFrame.Generate();
            return source.Sample(update);
        }
    }
}
