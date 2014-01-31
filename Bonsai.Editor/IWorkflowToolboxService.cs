using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    interface IWorkflowToolboxService
    {
        string GetPackageDisplayName(string packageKey);

        IEnumerable<WorkflowElementDescriptor> GetToolboxElements();
    }
}
