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
    /// Represents a workflow property containing a double-precision floating-point number.
    /// </summary>
    [DisplayName("Double")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class DoubleProperty : WorkflowProperty<double>
    {
    }
}
