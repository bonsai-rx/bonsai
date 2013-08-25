using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [SourceMapping]
    [PropertyMapping]
    [XmlType("Combinator", Namespace = Constants.XmlNamespace)]
    public class CombinatorBuilder : BinaryCombinatorExpressionBuilder
    {
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();

        public object Combinator { get; set; }

        public string MemberSelector { get; set; }

        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        public override Expression Build()
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var combinatorExpression = Expression.Constant(Combinator);
            var combinatorType = Combinator.GetType();
            if (Other != null)
            {
                var combinatorAttributes = combinatorType.GetCustomAttributes(typeof(BinaryCombinatorAttribute), true);
                var methodName = ((BinaryCombinatorAttribute)combinatorAttributes.Single()).MethodName;
                var processMethods = combinatorType.GetMethods(bindingAttributes)
                                                   .Where(m => m.Name == methodName && m.GetParameters().Length == 2);
                return BuildCall(combinatorExpression, processMethods, Source, Other);
            }
            else
            {
                var combinatorAttributes = combinatorType.GetCustomAttributes(typeof(CombinatorAttribute), true);
                var methodName = ((CombinatorAttribute)combinatorAttributes.Single()).MethodName;
                var processMethod = combinatorType.GetMethods(bindingAttributes)
                                                   .Single(m => m.Name == methodName && m.GetParameters().Length == 1);
                return BuildCallRemapping(
                    combinatorExpression,
                    (combinator, sourceSelect) => BuildCall(combinator, processMethod, sourceSelect),
                    Source,
                    MemberSelector,
                    propertyMappings);
            }
        }
    }
}
