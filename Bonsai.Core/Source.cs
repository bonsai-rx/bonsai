using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai
{
    /// <summary>
    /// Represents a generator of observable sequences of data elements.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements emitted by the generator.</typeparam>
    [Source]
    [Combinator(MethodName = "Generate")]
    [WorkflowElementCategory(ElementCategory.Source)]
    public abstract class Source<TSource>
    {
        /// <summary>
        /// Generates an observable sequence of data elements.
        /// </summary>
        /// <returns>
        /// An observable sequence of data elements of type <typeparamref name="TSource"/>.
        /// </returns>
        public abstract IObservable<TSource> Generate();
    }
}
