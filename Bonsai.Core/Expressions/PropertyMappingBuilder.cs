using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that assigns values of an observable sequence
    /// to properties of a workflow element.
    /// </summary>
    [DefaultProperty("PropertyMappings")]
    [WorkflowElementCategory(ElementCategory.Property)]
    [XmlType("PropertyMapping", Namespace = Constants.XmlNamespace)]
    [Description("Assigns values of an observable sequence to properties of a workflow element.")]
    [TypeDescriptionProvider(typeof(PropertyMappingTypeDescriptionProvider))]
    public class PropertyMappingBuilder : SingleArgumentExpressionBuilder, INamedElement, IArgumentBuilder
    {
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();

        /// <summary>
        /// Gets a collection of property mappings that specify how input values are assigned
        /// to properties of the workflow element.
        /// </summary>
        [XmlArrayItem("Property")]
        [Description("Specifies how input values are assigned to properties of the workflow element.")]
        [Editor("Bonsai.Design.MappingCollectionEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        string INamedElement.Name
        {
            get
            {
                if (propertyMappings.Count > 0)
                {
                    return string.Join(
                        ExpressionHelper.ArgumentSeparator,
                        propertyMappings.Select(mapping => mapping.Name));
                }

                return GetElementDisplayName(GetType());
            }
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
            return arguments.First();
        }

        bool IArgumentBuilder.BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            return BuildArgument(source, successor, out argument);
        }

        internal virtual bool BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            argument = source;
            var workflowElement = GetWorkflowElement(successor.Target.Value);
            var instance = Expression.Constant(workflowElement);
            foreach (var mapping in propertyMappings)
            {
                argument = BuildPropertyMapping(argument, instance, mapping.Name, mapping.Selector);
            }

            return false;
        }

        class PropertyMappingTypeDescriptionProvider : TypeDescriptionProvider
        {
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(PropertyMappingBuilder));

            public PropertyMappingTypeDescriptionProvider()
                : base(parentProvider)
            {
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                var builder = (PropertyMappingBuilder)instance;
                if (builder != null) return new PropertyMappingCollectionTypeDescriptor(builder.PropertyMappings);
                else return base.GetExtendedTypeDescriptor(instance);
            }
        }

        internal class PropertyMappingCollectionTypeDescriptor : CustomTypeDescriptor
        {
            PropertyMappingCollection instance;

            public PropertyMappingCollectionTypeDescriptor(PropertyMappingCollection collection)
            {
                instance = collection;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return GetProperties(null);
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                if (instance == null) return base.GetProperties(attributes);
                return new PropertyDescriptorCollection(
                    (from mapping in instance
                     where !string.IsNullOrEmpty(mapping.Name)
                     select new MappingPropertyDescriptor(mapping))
                     .ToArray());
            }
        }

        class MappingPropertyDescriptor : PropertyDescriptor
        {
            readonly PropertyMapping mapping;
            static readonly Attribute[] DescriptorAttributes = new Attribute[]
            {
                new ExternalizableAttribute(false),
                new EditorAttribute("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)
            };

            public MappingPropertyDescriptor(PropertyMapping mapping)
                : base(mapping.Name, DescriptorAttributes)
            {
                this.mapping = mapping;
            }

            public override string Category
            {
                get { return "Properties"; }
            }

            public override bool CanResetValue(object component)
            {
                return ShouldSerializeValue(component);
            }

            public override Type ComponentType
            {
                get { return typeof(PropertyMappingCollection); }
            }

            public override object GetValue(object component)
            {
                return mapping.Selector;
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return typeof(string); }
            }

            public override void ResetValue(object component)
            {
                mapping.Selector = null;
            }

            public override void SetValue(object component, object value)
            {
                mapping.Selector = (string)value;
            }

            public override bool ShouldSerializeValue(object component)
            {
                return !string.IsNullOrEmpty(mapping.Selector);
            }
        }
    }
}
