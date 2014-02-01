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
    /// Represents a workflow property containing a 32-bit signed integer.
    /// </summary>
    [DisplayName("Int")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class IntProperty : WorkflowProperty<int>
    {
    }
}
