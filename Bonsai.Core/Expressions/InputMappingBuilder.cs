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
    /// Represents an expression builder that selects inner properties of elements of the sequence
    /// and assigns their values to properties of a workflow element.
    /// </summary>
    [XmlType("InputMapping", Namespace = Constants.XmlNamespace)]
    [Description("Selects inner properties of elements of the sequence and assigns their values to properties of a workflow element.")]
    [TypeDescriptionProvider(typeof(InputMappingTypeDescriptionProvider))]
    public class InputMappingBuilder : PropertyMappingBuilder, ISerializableElement
    {
        readonly MemberSelectorBuilder selector = new MemberSelectorBuilder();

        /// <summary>
        /// Gets or sets a string used to select the input element member to project
        /// as output of the sequence.
        /// </summary>
        [Description("The inner properties that will be selected for each element of the sequence.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Selector
        {
            get { return selector.Selector; }
            set { selector.Selector = value; }
        }

        /// <summary>
        /// Gets or sets an optional type mapping specifying the data type which the selected properties
        /// will be projected into.
        /// </summary>
        [Externalizable(false)]
        [TypeConverter(typeof(TypeMappingConverter))]
        [Description("Specifies an optional output type into which the selected properties will be mapped.")]
        [Editor("Bonsai.Design.TypeMappingEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public TypeMapping TypeMapping
        {
            get { return selector.TypeMapping; }
            set { selector.TypeMapping = value; }
        }

        object ISerializableElement.Element
        {
            get { return selector.TypeMapping; }
        }

        internal override bool BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            base.BuildArgument(source, successor, out argument);
            argument = selector.Build(argument);
            return true;
        }

        class InputMappingTypeDescriptionProvider : TypeDescriptionProvider
        {
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(InputMappingBuilder));

            public InputMappingTypeDescriptionProvider()
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
    }
}
