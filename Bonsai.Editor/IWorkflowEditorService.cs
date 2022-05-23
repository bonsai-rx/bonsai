using System;
using System.IO;
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

        void SelectNextControl(bool forward);

        bool ValidateWorkflow();

        void RefreshEditor();
    }
}
