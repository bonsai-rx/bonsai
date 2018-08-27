using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision.Drawing
{
    [Description("Combines the drawing elements of two or more canvases.")]
    public class MergeCanvas : Transform<Tuple<Canvas, Canvas>, Canvas>
    {
        public override IObservable<Canvas> Process(IObservable<Tuple<Canvas, Canvas>> source)
        {
            return source.Select(input => new Canvas(input.Item1, input.Item2));
        }
    }
}
