using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai
{
    [XmlRoot("Workflows")]
    public class WorkflowCollection : Collection<Workflow>
    {
    }
}
