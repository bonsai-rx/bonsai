using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    [Description("Produces a sequence of events whenever a keyboard key is depressed.")]
    public class KeyDown : Source<Keys>
    {
        [Description("The target keys to be observed.")]
        public Keys Filter { get; set; }

        [Description("Indicates whether to ignore character repetitions when a key is held down.")]
        public bool SuppressRepetitions { get; set; }

        public override IObservable<Keys> Generate()
        {
            Func<Keys, bool> predicate = key => Filter == Keys.None || key == Filter;
            var source = InterceptKeys.Instance.KeyDown.Where(predicate);
            if (SuppressRepetitions)
            {
                source = source
                    .Window(() => InterceptKeys.Instance.KeyUp.Where(predicate))
                    .SelectMany(sequence => sequence.Take(1));
            }
            return source;
        }
    }
}
