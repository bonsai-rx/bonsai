using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;

namespace Bonsai.Expressions
{
    [XmlType("CombineLatest")]
    public class CombineLatestBuilder : ExpressionBuilder
    {
        static readonly MethodInfo combineLatestMethod = typeof(Observable).GetMethod("CombineLatest");

        [XmlIgnore]
        [Browsable(false)]
        public Expression First { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public Expression Second { get; set; }

        [Browsable(false)]
        public LoadableElement Projection { get; set; }

        public override Expression Build()
        {
            var projectionGenericArguments = ExpressionBuilder.GetProjectionGenericArguments(Projection);
            var selectorType = Expression.GetFuncType(projectionGenericArguments);

            var processMethod = Projection.GetType().GetMethod("Process");
            var selectorDelegate = Delegate.CreateDelegate(selectorType, Projection, processMethod);
            var selector = Expression.Constant(selectorDelegate);
            return Expression.Call(combineLatestMethod.MakeGenericMethod(projectionGenericArguments), First, Second, selector);
        }
    }
}
