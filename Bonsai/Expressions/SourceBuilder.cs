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
    [PropertyMapping]
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("Source", Namespace = Constants.XmlNamespace)]
    public class SourceBuilder : CombinatorExpressionBuilder
    {
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();

        public SourceBuilder()
            : base(0, 1)
        {
        }

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
            return BuildCallRemapping(
                    sourceExpression,
                    (combinator, sourceSelect) =>
                    {
                        var decoratedSource = HandleBuildException(Expression.Call(combinator, generateMethod), this);
                        if (sourceSelect != null)
                        {
                            var selectorType = sourceSelect.Type.GetGenericArguments()[0];
                            var decoratedSourceType = decoratedSource.Type.GetGenericArguments()[0];
                            decoratedSource = Expression.Call(typeof(SourceBuilder), "IgnoreSourceConnection", new[] { decoratedSourceType, selectorType }, decoratedSource, sourceSelect);
                        }
                        return decoratedSource;
                    },
                    Arguments.Values.SingleOrDefault(),
                    null,
                    propertyMappings,
                    hot:true);
        }

        static IObservable<TSource> IgnoreSourceConnection<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> connection)
        {
            return connection.IgnoreElements().Select(xs => default(TSource)).Merge(source);
        }
    }
}
