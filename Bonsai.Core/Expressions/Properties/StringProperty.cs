using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
