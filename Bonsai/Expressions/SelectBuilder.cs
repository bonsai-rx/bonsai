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
    [XmlType("Select", Namespace = Constants.XmlNamespace)]
    public class SelectBuilder : CombinatorBuilder
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .First(m => m.Name == "Select" &&
                                                                           m.GetParameters().Length == 2);

        [Browsable(false)]
        public LoadableElement Projection { get; set; }

        public override Expression Build()
        {
            var processMethod = Projection.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                                    .First(m => m.Name == "Process" && m.GetParameters().Length == 1);

            var projectionGenericArguments = processMethod.GetParameters()
                                                          .Select(info => info.ParameterType)
                                                          .Concat(Enumerable.Repeat(processMethod.ReturnType, 1))
                                                          .ToArray();

            var selectorType = Expression.GetFuncType(projectionGenericArguments);
            var selectorDelegate = Delegate.CreateDelegate(selectorType, Projection, processMethod);
            var selector = Expression.Constant(selectorDelegate);
            return Expression.Call(selectMethod.MakeGenericMethod(projectionGenericArguments), Source, selector);
        }
    }
}
