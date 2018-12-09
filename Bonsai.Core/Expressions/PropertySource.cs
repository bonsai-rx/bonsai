using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a data source compatible with the specified workflow element property.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [WorkflowElementCategory(ElementCategory.Source)]
    [TypeDescriptionProvider(typeof(PropertySourceTypeDescriptionProvider))]
    [Description("Represents a data source created from an operator property.")]
    public abstract class PropertySource : ExpressionBuilder, INamedElement
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 1);

        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }

        /// <summary>
        /// Gets or sets the name of the externalized class member.
        /// </summary>
        [Browsable(false)]
        public string MemberName { get; set; }

        internal abstract Type ElementType { get; }

        string INamedElement.Name
        {
            get { return MemberName; }
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
            var instance = Expression.Constant(this);
            var methods = instance.Type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(m => m.Name == "Generate");
            return BuildCall(instance, methods, arguments.ToArray());
        }
    }

    /// <summary>
    /// Represents a data source compatible with the specified workflow element property.
    /// </summary>
    /// <typeparam name="TElement">
    /// The type of the workflow element from which the property data source was constructed.
    /// </typeparam>
    /// <typeparam name="TValue">The type of the property values.</typeparam>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class PropertySource<TElement, TValue> : PropertySource, INamedElement
    {
        TValue value;
        event Action<TValue> ValueChanged;

        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        [Description("The value of the property.")]
        public TValue Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnValueChanged(value);
            }
        }

        void OnValueChanged(TValue value)
        {
            var handler = ValueChanged;
            if (handler != null)
            {
                handler(value);
            }
        }

        internal override Type ElementType
        {
            get { return typeof(TElement); }
        }

        IObservable<TValue> Generate()
        {
            return Observable
                .Defer(() => Observable.Return(value))
                .Concat(Observable.FromEvent<TValue>(
                    handler => ValueChanged += handler,
                    handler => ValueChanged -= handler));
        }

        IObservable<TValue> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => value);
        }
    }
}
