using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Reflection;
using System.Reactive.Linq;

namespace Bonsai.Expressions
{
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("Source", Namespace = Constants.XmlNamespace)]
    public class SourceBuilder : CombinatorExpressionBuilder, INamedElement
    {
        public SourceBuilder()
            : base(minArguments: 0, maxArguments: 0)
        {
        }

        public string Name
        {
            get { return GetElementDisplayName(Generator); }
        }

        public object Generator { get; set; }

        public override Expression Build()
        {
            var output = BuildCombinator();
            var sourceExpression = Expression.Constant(Generator);
            return BuildMappingOutput(sourceExpression, output, PropertyMappings);
        }

        protected override Expression BuildCombinator()
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            var sourceType = Generator.GetType();
            var sourceExpression = Expression.Constant(Generator);
            var sourceAttributes = sourceType.GetCustomAttributes(typeof(SourceAttribute), true);
            var methodName = ((SourceAttribute)sourceAttributes.Single()).MethodName;
            var generateMethod = sourceType.GetMethods(bindingAttributes)
                                           .Single(m => m.Name == methodName && m.GetParameters().Length == 0);
            return HandleBuildException(Expression.Call(sourceExpression, generateMethod), this);
        }
    }
}
