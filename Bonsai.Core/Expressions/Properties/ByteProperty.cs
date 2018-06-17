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
    /// Represents a workflow property containing an 8-bit unsigned integer.
    /// </summary>
    [DisplayName("Byte")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Represents a workflow property containing an 8-bit unsigned integer.")]
    public class ByteProperty : WorkflowProperty<byte>
    {
    }
}
