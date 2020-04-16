using System;
using System.ComponentModel;
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
