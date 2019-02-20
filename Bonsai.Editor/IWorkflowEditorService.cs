using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Editor
{
    interface IWorkflowEditorService
    {
        void OnKeyDown(KeyEventArgs e);

        void OnKeyPress(KeyPressEventArgs e);

        void OnContextMenuOpening(EventArgs e);

        void OnContextMenuClosed(EventArgs e);

        DirectoryInfo EnsureExtensionsDirectory();

        WorkflowBuilder LoadWorkflow(string fileName);

        void OpenWorkflow(string fileName);

        string StoreWorkflowElements(WorkflowBuilder builder);

        WorkflowBuilder RetrieveWorkflowElements(string text);

        IEnumerable<Type> GetTypeVisualizers(Type targetType);

        void SelectNextControl(bool forward);

        void StartWorkflow(bool debugging);

        void StopWorkflow();

        void RestartWorkflow();

        bool ValidateWorkflow();

        void RefreshEditor();
    }
}
