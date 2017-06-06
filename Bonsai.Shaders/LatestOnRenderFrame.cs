using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Samples the latest notifications of the input sequence on the render frame event.")]
    public class LatestOnRenderFrame : Combinator
    {
        static readonly RenderFrame renderFrame = new RenderFrame();

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var render = renderFrame.Generate();
            return source.CombineLatest(render, (x, evt) => x).Sample(render);
        }
    }
}
