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
    public class SourceBuilder : CombinatorExpressionBuilder
    {
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();

        public object Generator { get; set; }

        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        public override Expression Build()
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var sourceType = Generator.GetType();
            var sourceExpression = Expression.Constant(Generator);
            var sourceAttributes = sourceType.GetCustomAttributes(typeof(SourceAttribute), true);
            var methodName = ((SourceAttribute)sourceAttributes.Single()).MethodName;
            var generateMethod = sourceType.GetMethods(bindingAttributes)
                                           .Single(m => m.Name == methodName && m.GetParameters().Length == 0);
            return BuildCallRemapping(sourceExpression, generateMethod, Source, null, propertyMappings, true);
        }
    }
}
