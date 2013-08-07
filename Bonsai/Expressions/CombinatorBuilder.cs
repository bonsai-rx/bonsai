using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("Combinator", Namespace = Constants.XmlNamespace)]
    public class CombinatorBuilder : BinaryCombinatorExpressionBuilder
    {
        public LoadableElement Combinator { get; set; }

        public override Expression Build()
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var combinatorExpression = Expression.Constant(Combinator);
            var combinatorType = Combinator.GetType();
            if (Other != null)
            {
                var combinatorAttributes = combinatorType.GetCustomAttributes(typeof(BinaryCombinatorAttribute), true);
                var methodName = ((BinaryCombinatorAttribute)combinatorAttributes.Single()).MethodName;
                var processMethod = combinatorType.GetMethods(bindingAttributes)
                                                  .Single(m => m.Name == methodName && m.GetParameters().Length == 2);
                return ExpressionBuilder.Call(combinatorExpression, processMethod, Source, Other);
            }
            else
            {
                var combinatorAttributes = combinatorType.GetCustomAttributes(typeof(CombinatorAttribute), true);
                var methodName = ((CombinatorAttribute)combinatorAttributes.Single()).MethodName;
                var processMethod = combinatorType.GetMethods(bindingAttributes)
                                                  .Single(m => m.Name == methodName && m.GetParameters().Length == 1);
                return ExpressionBuilder.Call(combinatorExpression, processMethod, Source);
            }
        }
    }
}
