using System;
using System.IO;
using System.Windows.Forms;
using Bonsai.Expressions;

namespace Bonsai.Editor
{
    interface IWorkflowEditorService
    {
        string GetProjectDisplayName();

        void OnKeyDown(KeyEventArgs e);

        void OnKeyPress(KeyPressEventArgs e);

        void OnContextMenuOpening(EventArgs e);

        void OnContextMenuClosed(EventArgs e);

        DirectoryInfo EnsureExtensionsDirectory();

        WorkflowBuilder LoadWorkflow(string fileName);

        void OpenWorkflow(string fileName);

        void SelectNextControl(bool forward);

        void SelectBuilderNode(ExpressionBuilder builder);

        bool ValidateWorkflow();

        void RefreshEditor();
    }
}
