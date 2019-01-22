using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Specifies a set of properties to be externalized from a workflow element.
    /// </summary>
    [DefaultProperty("ExternalizedProperties")]
    [WorkflowElementCategory(ElementCategory.Property)]
    [XmlType("ExternalizedMapping", Namespace = Constants.XmlNamespace)]
    [Description("Specifies a set of properties to be externalized from a workflow element.")]
    [TypeDescriptionProvider(typeof(ExternalizedMappingTypeDescriptionProvider))]
    public class ExternalizedMappingBuilder : ZeroArgumentExpressionBuilder, INamedElement, IArgumentBuilder, IExternalizedMappingBuilder
    {
        readonly ExternalizedMappingCollection externalizedProperties = new ExternalizedMappingCollection();

        /// <summary>
        /// Gets the collection of properties to be externalized from the workflow element.
        /// </summary>
        [Externalizable(false)]
        [XmlElement("Property")]
        [Description("Specifies the set of properties to be externalized.")]
        [Editor("Bonsai.Design.MappingCollectionEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public ExternalizedMappingCollection ExternalizedProperties
        {
            get { return externalizedProperties; }
        }

        string INamedElement.Name
        {
            get
            {
                if (externalizedProperties.Count > 0)
                {
                    return string.Join(
                        ExpressionHelper.ArgumentSeparator,
                        externalizedProperties.Select(property => string.IsNullOrEmpty(property.ExternalizedName)
                            ? property.Name
                            : property.ExternalizedName));
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
            return EmptyExpression.Instance;
        }

        IEnumerable<ExternalizedMapping> IExternalizedMappingBuilder.GetExternalizedProperties()
        {
            return externalizedProperties;
        }

        bool IArgumentBuilder.BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            argument = source;
            return false;
        }

        class ExternalizedMappingTypeDescriptionProvider : TypeDescriptionProvider
        {
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(ExternalizedMappingBuilder));

            public ExternalizedMappingTypeDescriptionProvider()
                : base(parentProvider)
            {
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                var builder = (ExternalizedMappingBuilder)instance;
                if (builder != null) return new ExternalizedMappingCollectionTypeDescriptor(builder.ExternalizedProperties);
                else return base.GetExtendedTypeDescriptor(instance);
            }
        }

        class ExternalizedMappingCollectionTypeDescriptor : CustomTypeDescriptor
        {
            ExternalizedMappingCollection instance;

            public ExternalizedMappingCollectionTypeDescriptor(ExternalizedMappingCollection collection)
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
                var properties = new PropertyDescriptor[instance.Count];
                for (int i = 0; i < properties.Length; i++)
                {
                    properties[i] = new ExternalizedMappingPropertyDescriptor(instance[i]);
                }
                return new PropertyDescriptorCollection(properties);
            }
        }

        class ExternalizedMappingPropertyDescriptor : PropertyDescriptor
        {
            readonly ExternalizedMapping mapping;
            static readonly Attribute[] DescriptorAttributes = new Attribute[]
            {
                new ExternalizableAttribute(false),
                new TypeConverterAttribute(typeof(ExternalizedMappingConverter))
            };

            public ExternalizedMappingPropertyDescriptor(ExternalizedMapping mapping)
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
                get { return typeof(ExternalizedMappingCollection); }
            }

            public override object GetValue(object component)
            {
                return mapping;
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return typeof(ExternalizedMapping); }
            }

            public override void ResetValue(object component)
            {
                mapping.DisplayName = null;
                mapping.Description = null;
                mapping.Category = null;
            }

            public override void SetValue(object component, object value)
            {
            }

            public override bool ShouldSerializeValue(object component)
            {
                return !string.IsNullOrEmpty(mapping.DisplayName) ||
                    !string.IsNullOrEmpty(mapping.Description) ||
                    !string.IsNullOrEmpty(mapping.Category);
            }
        }

        class ExternalizedMappingConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var mapping = value as ExternalizedMapping;
                if (destinationType == typeof(string) && mapping != null)
                {
                    var displayName = mapping.DisplayName;
                    if (string.IsNullOrEmpty(displayName)) displayName = mapping.Name;
                    return "(" + displayName + ")";
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                var baseProperties = base.GetProperties(context, value, attributes);
                var properties = new PropertyDescriptorCollection(null);
                foreach (PropertyDescriptor baseProperty in baseProperties)
                {
                    if (baseProperty.Name == "Name") continue;
                    var property = TypeDescriptor.CreateProperty(
                        baseProperty.ComponentType,
                        baseProperty,
                        RefreshPropertiesAttribute.Repaint);
                    properties.Add(property);
                }
                return properties;
            }
        }
    }
}
