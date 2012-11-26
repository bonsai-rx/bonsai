using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;

namespace Bonsai.Design
{
    public static class ControlObservable
    {
        public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, Control control)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            return source.ObserveOn(new ControlScheduler(control));
        }
    }
}
