using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Design
{
    public static class WorkflowVisitor
    {
        public static object GetFilterInput(this WorkflowProject project, object filter)
        {
            object input = null;
            Visitor(project, (container, index, column, row) =>
            {
                var component = container.Components[index];
                if (component == filter)
                {
                    dynamic observable = container.Components[index - 1];
                    input = observable.Output;
                }
            });

            return input;
        }

        public static object GetFilterOutput(this WorkflowProject project, object filter)
        {
            object input = null;
            Visitor(project, (container, index, column, row) =>
            {
                var component = container.Components[index];
                if (component == filter)
                {
                    dynamic observable = container.Components[index + 1];
                    input = observable.Output;
                }
            });

            return input;
        }

        public static void Visitor(this WorkflowProject project, Action<IWorkflowContainer, int, int, int> visitor)
        {
            var rowOffset = 0;
            for (int i = 0; i < project.Workflows.Count; i++)
            {
                var workflow = project.Workflows[i];
                rowOffset += Visitor(workflow, visitor, 0, rowOffset);
            }
        }

        public static int Visitor(IWorkflowContainer container, Action<IWorkflowContainer, int, int, int> visitor, int column, int row)
        {
            var rowHeight = 1;
            for (int i = container.Components.Count - 1; i >= 0; i--)
            {
                var component = container.Components[i];
                visitor(container, i, column + i, row);

                var subContainer = component as IWorkflowContainer;
                if (subContainer != null)
                {
                    rowHeight += Visitor(subContainer, visitor, column + i + 1, row + rowHeight);
                }
            }

            return rowHeight;
        }
    }
}
