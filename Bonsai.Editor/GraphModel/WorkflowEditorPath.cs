using System;
using System.Collections.Generic;
using Bonsai.Expressions;

namespace Bonsai.Editor.GraphModel
{
    class WorkflowEditorPath : IEquatable<WorkflowEditorPath>
    {
        public WorkflowEditorPath()
        {
        }

        public WorkflowEditorPath(int index, WorkflowEditorPath parent)
        {
            Index = index;
            Parent = parent;
        }

        public int Index { get; }

        public WorkflowEditorPath Parent { get; }

        public IEnumerable<WorkflowEditorPath> GetPathElements()
        {
            var stack = new Stack<WorkflowEditorPath>();
            var pathElement = this;
            while (pathElement != null)
            {
                stack.Push(pathElement);
                pathElement = pathElement.Parent;
            }

            foreach (var element in stack)
            {
                yield return element;
            }
        }

        public ExpressionBuilder Resolve(WorkflowBuilder workflowBuilder)
        {
            return Resolve(workflowBuilder, out _);
        }

        public ExpressionBuilder Resolve(WorkflowBuilder workflowBuilder, out WorkflowPathFlags pathFlags)
        {
            pathFlags = WorkflowPathFlags.None;
            var builder = default(ExpressionBuilder);
            var workflow = workflowBuilder.Workflow;
            foreach (var pathElement in GetPathElements())
            {
                if (workflow == null)
                {
                    throw new ArgumentException($"Unable to resolve workflow editor path.", nameof(workflowBuilder));
                }

                builder = ExpressionBuilder.Unwrap(workflow[pathElement.Index].Value);
                if (builder is DisableBuilder disableBuilder)
                {
                    builder = disableBuilder.Builder;
                    pathFlags |= WorkflowPathFlags.Disabled;
                }

                if (builder is IWorkflowExpressionBuilder nestedWorkflowBuilder)
                {
                    workflow = nestedWorkflowBuilder.Workflow;
                    if (nestedWorkflowBuilder is IncludeWorkflowBuilder)
                        pathFlags |= WorkflowPathFlags.ReadOnly;
                }
                else workflow = null;
            }

            return builder;
        }

        public static WorkflowEditorPath GetExceptionPath(WorkflowBuilder workflowBuilder, WorkflowException ex)
        {
            return GetExceptionPath(workflowBuilder.Workflow, ex, null);
        }

        static WorkflowEditorPath GetExceptionPath(ExpressionBuilderGraph workflow, WorkflowException ex, WorkflowEditorPath parent)
        {
            for (int i = 0; i < workflow.Count; i++)
            {
                var builder = workflow[i].Value;
                if (builder == ex.Builder)
                {
                    var path = new WorkflowEditorPath(i, parent);
                    if (ex.InnerException is WorkflowException nestedEx &&
                        ExpressionBuilder.GetWorkflowElement(ex.Builder) is IWorkflowExpressionBuilder workflowBuilder)
                    {
                        return GetExceptionPath(workflowBuilder.Workflow, nestedEx, path);
                    }
                    else return path;
                }
            }

            return null;
        }

        public static WorkflowEditorPath GetBuilderPath(WorkflowBuilder workflowBuilder, ExpressionBuilder builder)
        {
            return GetBuilderPath(workflowBuilder.Workflow, ExpressionBuilder.Unwrap(builder), new List<int>());
        }

        static WorkflowEditorPath GetBuilderPath(ExpressionBuilderGraph workflow, ExpressionBuilder target, List<int> pathElements)
        {
            for (int i = 0; i < workflow.Count; i++)
            {
                var builder = ExpressionBuilder.GetWorkflowElement(workflow[i].Value);
                if (builder == target)
                {
                    pathElements.Add(i);
                    return GetBuilderPath(pathElements);
                }

                if (builder is IWorkflowExpressionBuilder workflowBuilder)
                {
                    pathElements.Add(i);
                    var path = GetBuilderPath(workflowBuilder.Workflow, target, pathElements);
                    if (path is not null)
                        return path;
                    pathElements.RemoveAt(pathElements.Count - 1);
                }
            }

            return null;
        }

        static WorkflowEditorPath GetBuilderPath(List<int> pathElements)
        {
            WorkflowEditorPath path = null;
            foreach (var index in pathElements)
            {
                path = new WorkflowEditorPath(index, path);
            }
            return path;
        }

        public override int GetHashCode()
        {
            var hash = 107;
            hash += Index.GetHashCode() * 13;
            hash += (Parent?.GetHashCode()).GetValueOrDefault() * 13;
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is WorkflowEditorPath path)
                return Equals(path);

            return false;
        }

        public bool Equals(WorkflowEditorPath other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other == null)
                return false;

            return Index == other.Index && Parent == other.Parent;
        }

        public static bool operator ==(WorkflowEditorPath left, WorkflowEditorPath right)
        {
            if (left is not null) return left.Equals(right);
            else return right is null;
        }

        public static bool operator !=(WorkflowEditorPath left, WorkflowEditorPath right)
        {
            if (left is not null) return !left.Equals(right);
            else return right is not null;
        }
    }
}
