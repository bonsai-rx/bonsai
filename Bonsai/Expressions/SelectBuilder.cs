using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("Select")]
    [TypeDescriptionProvider(typeof(BuilderDescriptionProvider<SelectBuilder>))]
    public class SelectBuilder : CombinatorBuilder
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .First(m => m.Name == "Select" &&
                                                                           m.GetParameters().Length == 2);

        [Browsable(false)]
        public LoadableElement Projection { get; set; }

        public override Expression Build()
        {
            var projectionGenericArguments = ExpressionBuilder.GetProjectionGenericArguments(Projection);
            var selectorType = Expression.GetFuncType(projectionGenericArguments);

            var processMethod = Projection.GetType().GetMethod("Process");
            var selectorDelegate = Delegate.CreateDelegate(selectorType, Projection, processMethod);
            var selector = Expression.Constant(selectorDelegate);
            return Expression.Call(selectMethod.MakeGenericMethod(projectionGenericArguments), Source, selector);
        }
    }
}
