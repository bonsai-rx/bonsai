using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that handles sharing of sequences between multiple
    /// branches of an expression builder workflow by publishing notifications across branches.
    /// </summary>
    [XmlType("Publish", Namespace = Constants.XmlNamespace)]
    [Description("Shares an observable sequence between multiple branches by publishing notifications across branches.")]
    public class PublishBuilder : MulticastBuilder
    {
        internal override IObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        {
            return source.Publish(selector);
        }
    }
}
