using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Reactive.Subjects;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    public class PublishBuilder : CombinatorBuilder
    {
        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Publish().RefCount();
        }
    }
}
