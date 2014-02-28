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
    /// Represents a workflow property containing a date and time of day.
    /// </summary>
    [DisplayName("DateTime")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Represents a workflow property containing a date and time of day.")]
    public class DateTimeProperty : WorkflowProperty<DateTime>
    {
    }
}
