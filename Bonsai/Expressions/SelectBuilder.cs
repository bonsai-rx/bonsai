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
    public class SelectBuilder : CombinatorExpressionBuilder
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public LoadableElement Transform { get; set; }

        public override Expression Build()
        {
            var processMethod = Transform.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                                   .Single(m => m.Name == "Process" && m.GetParameters().Length == 1);

            var transformGenericArguments = processMethod.GetParameters()
                                                          .Select(info => info.ParameterType)
                                                          .Concat(Enumerable.Repeat(processMethod.ReturnType, 1))
                                                          .ToArray();

            var selectorType = Expression.GetFuncType(transformGenericArguments);
            var selectorDelegate = Delegate.CreateDelegate(selectorType, Transform, processMethod);
            var selector = Expression.Constant(selectorDelegate);
            return Expression.Call(selectMethod.MakeGenericMethod(transformGenericArguments), Source, selector);
        }
    }
}
