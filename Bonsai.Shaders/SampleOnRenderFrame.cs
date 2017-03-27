using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

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
