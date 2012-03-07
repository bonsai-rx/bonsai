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
    [XmlType("Where", Namespace = Constants.XmlNamespace)]
    public class WhereBuilder : CombinatorExpressionBuilder
    {
        static readonly MethodInfo whereMethod = typeof(Observable).GetMethods()
                                                                   .First(m => m.Name == "Where" &&
                                                                          m.GetParameters().Length == 2);

        public LoadableElement Filter { get; set; }

        public override Expression Build()
        {
            Delegate predicateDelegate;
            var observableType = Source.Type.GetGenericArguments()[0];
            var filterType = ExpressionBuilder.GetFilterGenericArgument(Filter);
            var processMethod = Filter.GetType().GetMethod("Process");

            if (observableType.IsValueType && filterType == typeof(object))
            {
                var filterExpression = Expression.Constant(Filter);
                var parameter = Expression.Parameter(observableType);
                var body = Expression.Call(filterExpression, processMethod, Expression.Convert(parameter, typeof(object)));
                predicateDelegate = Expression.Lambda(body, parameter).Compile();
            }
            else
            {
                var predicateType = Expression.GetFuncType(new[] { observableType, typeof(bool) });
                predicateDelegate = Delegate.CreateDelegate(predicateType, Filter, processMethod);
            }

            var predicate = Expression.Constant(predicateDelegate);
            return Expression.Call(whereMethod.MakeGenericMethod(observableType), Source, predicate);
        }
    }
}
