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
    /// Represents a generator of observable sequences of data values.
    /// </summary>
    /// <typeparam name="TSource">The type of values emitted by each notification.</typeparam>
    [Source]
    [WorkflowElementCategory(ElementCategory.Source)]
    public abstract class Source<TSource>
    {
        /// <summary>
        /// Generates an observable sequence of values.
        /// </summary>
        /// <returns>An observable sequence of data values.</returns>
        public abstract IObservable<TSource> Generate();
    }
}
