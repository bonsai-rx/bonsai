using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Reflection;

namespace Bonsai.Expressions
{
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("Source", Namespace = Constants.XmlNamespace)]
    public class SourceBuilder : ExpressionBuilder
    {
        public LoadableElement Source { get; set; }

        public override Expression Build()
        {
            var sourceType = Source.GetType();
            var sourceExpression = Expression.Constant(Source);
            var sourceAttributes = sourceType.GetCustomAttributes(typeof(SourceAttribute), true);
            var methodName = ((SourceAttribute)sourceAttributes.Single()).MethodName;
            return Expression.Call(sourceExpression, methodName, null);
        }
    }
}
