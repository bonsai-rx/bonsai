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
    /// Represents a workflow property containing a 64-bit signed integer.
    /// </summary>
    [DisplayName("Int64")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Represents a workflow property containing a 64-bit signed integer.")]
    public class Int64Property : WorkflowProperty<long>
    {
    }
}
