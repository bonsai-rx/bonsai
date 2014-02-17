using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions.Properties
{
    /// <summary>
    /// Represents a strongly typed property that has been externalized from a workflow element.
    /// This class can be used to convert class parameters of workflow elements into explicit
    /// source modules.
    /// </summary>
    /// <typeparam name="TValue">The type of the externalized property value.</typeparam>
    /// <typeparam name="TElement">
    /// The type of the workflow element to which the externalized member is bound to.
    /// </typeparam>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [TypeDescriptionProvider(typeof(ExternalizedPropertyTypeDescriptionProvider))]
    public class ExternalizedProperty<TValue, TElement> : WorkflowProperty<TValue>, IExternalizedProperty
    {
        /// <summary>
        /// Gets or sets the name of the externalized class member.
        /// </summary>
        [Browsable(false)]
        public string MemberName { get; set; }

        Type IExternalizedProperty.ElementType
        {
            get { return typeof(TElement); }
        }
    }
}
