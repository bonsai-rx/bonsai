using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.Reactive;

namespace Bonsai.Expressions
{
    [XmlType("Unit", Namespace = Constants.XmlNamespace)]
    [Description("Converts a sequence of any type into a sequence of Unit type elements.")]
    public class UnitBuilder : ExpressionBuilder
    {
        public UnitBuilder()
            : base(minArguments: 1, maxArguments: 1)
        {
        }

        public override Expression Build()
        {
            var source = Arguments.Values.Single();
            return Expression.Call(typeof(UnitBuilder), "Process", source.Type.GetGenericArguments(), source);
        }

        static IObservable<Unit> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(xs => Unit.Default);
        }
    }
}
