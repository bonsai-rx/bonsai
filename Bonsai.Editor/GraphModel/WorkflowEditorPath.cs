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
            var builder = default(ExpressionBuilder);
            var workflow = workflowBuilder.Workflow;
            foreach (var pathElement in GetPathElements())
            {
                if (workflow == null)
                {
                    throw new ArgumentException($"Unable to resolve workflow editor path.", nameof(workflowBuilder));
                }

                builder = workflow[pathElement.Index].Value;
                if (ExpressionBuilder.Unwrap(builder) is IWorkflowExpressionBuilder nestedWorkflowBuilder)
                {
                    workflow = nestedWorkflowBuilder.Workflow;
                }
                else workflow = null;
            }

            return builder;
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
