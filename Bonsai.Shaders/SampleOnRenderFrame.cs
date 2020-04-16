using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Samples notifications of the input sequence whenever it is time to draw a frame.")]
    public class SampleOnRenderFrame : Combinator
    {
        static readonly RenderFrame renderFrame = new RenderFrame();

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var render = renderFrame.Generate();
            return source.Sample(render);
        }
    }
}
