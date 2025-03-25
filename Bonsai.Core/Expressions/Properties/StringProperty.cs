using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a workflow property containing Unicode text.
    /// </summary>
    [DisplayName("String")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Represents a workflow property containing Unicode text.")]
    public class StringProperty : WorkflowProperty<string>
    {
        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public new string Value
        {
            get => base.Value;
            set => base.Value = value;
        }
    }
}
