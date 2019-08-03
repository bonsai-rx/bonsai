using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that defines a simple selector on the elements
    /// of an observable sequence by mapping specified member values into the output data type.
    /// </summary>
    [DefaultProperty("Selector")]
    [XmlType("MemberSelector", Namespace = Constants.XmlNamespace)]
    [Description("Selects inner properties of elements of the sequence and optionally maps them into the specified output type.")]
    public class MemberSelectorBuilder : SelectBuilder, INamedElement, ISerializableElement
    {
        /// <summary>
        /// Gets or sets a string used to select the input element members that will
        /// be projected as output of the sequence.
        /// </summary>
        [Description("The inner properties that will be selected for each element of the sequence.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Selector { get; set; }

        /// <summary>
        /// Gets or sets an optional type mapping specifying the data type which the selected properties
        /// will be projected into.
        /// </summary>
        [Externalizable(false)]
        [TypeConverter(typeof(TypeMappingConverter))]
        [Description("Specifies an optional output type into which the selected properties will be mapped.")]
        [Editor("Bonsai.Design.TypeMappingEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public TypeMapping TypeMapping { get; set; }

        object ISerializableElement.Element
        {
            get { return TypeMapping; }
        }

        string INamedElement.Name
        {
            get
            {
                var selector = Selector;
                if (!string.IsNullOrEmpty(selector))
                {
                    string[] memberNames;
                    try
                    {
                        memberNames = ExpressionHelper.SelectMemberNames(selector).ToArray();
                        return string.Join(ExpressionHelper.ArgumentSeparator, memberNames);
                    }
                    catch (InvalidOperationException) { }
                }

                return GetElementDisplayName(GetType());
            }
        }

        /// <summary>
        /// Returns the expression that maps the specified input parameter
        /// to the selector result.
        /// </summary>
        /// <param name="expression">The input parameter to the selector.</param>
        /// <returns>
        /// The <see cref="Expression"/> that maps the input parameter to the
        /// selector result.
        /// </returns>
        protected override Expression BuildSelector(Expression expression)
        {
            var targetType = TypeMapping != null ? TypeMapping.TargetType : null;
            return BuildTypeMapping(expression, targetType, Selector);
        }
    }
}
