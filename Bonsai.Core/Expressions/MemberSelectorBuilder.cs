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
    /// of an observable sequence by mapping each element to one of its member values.
    /// </summary>
    [DefaultProperty("Selector")]
    [XmlType("MemberSelector", Namespace = Constants.XmlNamespace)]
    [Description("Selects inner properties of elements of the sequence.")]
    public class MemberSelectorBuilder : SelectBuilder, INamedElement
    {
        /// <summary>
        /// Gets or sets a string used to select the input element member to project
        /// as output of the sequence.
        /// </summary>
        [Description("The inner properties that will be selected for each element of the sequence.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Selector { get; set; }

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
                        if (memberNames.Length == 1) return memberNames[0];
                        else
                        {
                            return string.Join(ExpressionHelper.ArgumentSeparator, Array.ConvertAll(memberNames, memberName =>
                                memberName != ExpressionBuilderArgument.ArgumentNamePrefix ?
                                memberName.Remove(0, ExpressionBuilderArgument.ArgumentNamePrefix.Length + 1) :
                                memberName));
                        }
                    }
                    catch (InvalidOperationException) { }
                }

                return "MemberSelector";
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
            return MemberSelector(expression, Selector);
        }
    }
}
