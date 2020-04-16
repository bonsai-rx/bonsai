using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a workflow property containing a Boolean value.
    /// </summary>
    [DisplayName("Boolean")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Represents a workflow property containing a logical Boolean value.")]
    public class BooleanProperty : WorkflowProperty<bool>
    {
    }
}
