
using System;
using System.Text;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.GraphView;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.Docking
{
    internal static class DockPanelSerializer
    {
        const char ContentTypeSeparator = ':';

        static void ThrowNotRecognizedException(string contentString)
        {
            throw new ArgumentException(
                $"The specified dock panel content was not recognized: {contentString}.",
                nameof(contentString));
        }

        public static IDockContent DeserializeContent(
            WorkflowEditorControl editorControl,
            WorkflowBuilder workflowBuilder,
            string contentString)
        {
            if (editorControl is null)
                throw new ArgumentNullException(nameof(editorControl));

            if (string.IsNullOrEmpty(contentString))
                throw new ArgumentException("The dock panel content is null or empty.", nameof(contentString));

            var contentElements = contentString.Split(new[] { ContentTypeSeparator });
            if (contentElements.Length < 1 || contentElements.Length > 2)
                ThrowNotRecognizedException(contentString);

            switch (contentElements[0])
            {
                case nameof(WorkflowDockContent):
                    var workflowPath = contentElements.Length > 1
                        ? WorkflowEditorPath.Parse(contentElements[1])
                        : null;
                    workflowPath?.Resolve(workflowBuilder);
                    return editorControl.CreateDockContent(workflowPath, DockState.Unknown);
                default:
                    if (editorControl.ToolWindows.Contains(contentString))
                        return editorControl.ToolWindows[contentString];
                    ThrowNotRecognizedException(contentString);
                    return default;
            }
        }

        public static string SerializeContent(WorkflowDockContent dockContent)
        {
            if (dockContent is null)
                throw new ArgumentNullException(nameof(dockContent));

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(nameof(WorkflowDockContent));
            if (dockContent.WorkflowGraphView.WorkflowPath != null)
            {
                stringBuilder.Append(ContentTypeSeparator);
                stringBuilder.Append(dockContent.WorkflowGraphView.WorkflowPath);
            }
            return stringBuilder.ToString();
        }
    }
}
