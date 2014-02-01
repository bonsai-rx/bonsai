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
    /// Represents a workflow property containing a Boolean value.
    /// </summary>
    [DisplayName("Boolean")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class BooleanProperty : WorkflowProperty<bool>
    {
    }
}
