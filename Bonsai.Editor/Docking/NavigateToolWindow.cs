using System;
using System.Text;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using Bonsai.Expressions;

namespace Bonsai.Editor.Docking
{
    internal class NavigateToolWindow : EditorToolWindow
    {
        static readonly object EventNavigate = new();

        protected NavigateToolWindow()
        {
        }

        protected NavigateToolWindow(IServiceProvider provider)
            : base(provider)
        {
        }

        public event WorkflowNavigateEventHandler Navigate
        {
            add { Events.AddHandler(EventNavigate, value); }
            remove { Events.RemoveHandler(EventNavigate, value); }
        }

        protected virtual void OnNavigate(WorkflowNavigateEventArgs e)
        {
            if (Events[EventNavigate] is WorkflowNavigateEventHandler handler)
            {
                handler(this, e);
            }
        }

        protected ListViewItem CreateNavigationViewItem(
            InspectBuilder inspectBuilder,
            WorkflowEditorPath workflowPath,
            WorkflowBuilder workflowBuilder)
        {
            var name = ExpressionBuilder.GetElementDisplayName(inspectBuilder);
            var elementType = ExpressionBuilder.GetWorkflowElement(inspectBuilder).GetType();
            var containerPath = GetElementDisplayPath(workflowBuilder, workflowPath.Parent);
            var item = new ListViewItem(name);
            item.Tag = workflowPath;
            item.SubItems.Add(containerPath);
            item.SubItems.Add(TypeHelper.GetTypeName(elementType));
            item.SubItems.Add(inspectBuilder.ObservableType is not null
                ? TypeHelper.GetTypeName(inspectBuilder.ObservableType)
                : string.Empty);
            return item;
        }

        static string GetElementDisplayPath(WorkflowBuilder workflowBuilder, WorkflowEditorPath path)
        {
            var sb = new StringBuilder();
            foreach (var pathElement in WorkflowEditorPath.GetPathDisplayElements(path, workflowBuilder))
            {
                if (sb.Length > 0)
                    sb.Append(" > ");
                sb.Append(pathElement.Key);
            }

            return sb.ToString();
        }
    }
}
