using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that casts each element of an observable sequence
    /// into the specified type.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Casts each element of the sequence into the specified type.")]
    public class Cast : SingleArgumentExpressionBuilder, ISerializableElement, INamedElement
    {
        /// <summary>
        /// Gets or sets a type mapping specifying the type into which each element
        /// of the sequence will be cast.
        /// </summary>
        [Browsable(false)]
        public TypeMapping TypeMapping { get; set; }

        object ISerializableElement.Element
        {
            get { return TypeMapping; }
        }

        string INamedElement.Name
        {
            get
            {
                var targetType = TypeMapping?.TargetType;
                var displayName = GetElementDisplayName(GetType());
                return targetType != null
                    ? $"{displayName}({GetTypeName(targetType)})"
                    : displayName;
            }
        }

        internal static string GetTypeName(Type type)
        {
            var typeName = type.Name;
            if (type.IsGenericType)
            {
                var separatorIndex = typeName.LastIndexOf('`');
                if (separatorIndex >= 0) typeName = typeName.Substring(0, separatorIndex);
                var typeArguments = string.Join(",", type.GetGenericArguments().Select(GetTypeName));
                return $"{typeName}<{typeArguments}>";
            }

            return typeName;
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var sourceType = source.Type.GetGenericArguments()[0];
            var targetType = TypeMapping?.TargetType ?? typeof(object);
            var combinator = Expression.Constant(this);
            return Expression.Call(
                combinator,
                nameof(Process),
                new[] { sourceType, targetType },
                source);
        }

        IObservable<TResult> Process<TSource, TResult>(IObservable<TSource> source)
        {
            var objectSource = source as IObservable<object> ?? source.Select(value => (object)value);
            return objectSource.Cast<TResult>();
        }
    }
}
