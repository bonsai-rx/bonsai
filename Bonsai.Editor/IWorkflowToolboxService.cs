using System.Collections.Generic;

namespace Bonsai.Editor
{
    interface IWorkflowToolboxService
    {
        string GetPackageDisplayName(string packageKey);

        IEnumerable<WorkflowElementDescriptor> GetToolboxElements();
    }
}
