using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that handles sharing of sequences between multiple
    /// branches of an expression builder workflow by eagerly replaying notifications
    /// across branches.
    /// </summary>
    [XmlType("Replay", Namespace = Constants.XmlNamespace)]
    [Description("Shares an observable sequence between multiple branches by replaying notifications across branches.")]
    public class ReplayBuilder : MulticastBuilder
    {
        internal override IObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        {
            return selector(source.Replay(Scheduler.Immediate).RefCount());
        }
    }
}
