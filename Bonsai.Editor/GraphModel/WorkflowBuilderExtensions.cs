using System.Collections.Generic;
using System.Linq;
using Bonsai.Expressions;

namespace Bonsai.Editor.GraphModel
{
    static class WorkflowBuilderExtensions
    {
        public static HashSet<ExpressionBuilder> GetReadOnlyElements(this WorkflowBuilder source)
        {
            var readOnlyElements = new HashSet<ExpressionBuilder>();
            var stack = new Stack<ExpressionBuilderGraph>();
            stack.Push(source.Workflow);

            while (stack.Count > 0)
            {
                var workflow = stack.Pop();
                foreach (var node in workflow)
                {
                    var builder = ExpressionBuilder.Unwrap(node.Value);
                    if (builder is IncludeWorkflowBuilder includeBuilder)
                    {
                        if (includeBuilder.Workflow is not null)
                            readOnlyElements.UnionWith(includeBuilder.Workflow.Descendants());
                    }
                    else if (builder is IWorkflowExpressionBuilder workflowBuilder && workflowBuilder.Workflow is not null)
                    {
                        stack.Push(workflowBuilder.Workflow);
                    }
                }
            }

            return readOnlyElements;
        }

        public static SubjectDefinition GetSubjectDefinition(this WorkflowBuilder source, ExpressionBuilderGraph target, string name)
        {
            var subject = source.Workflow
                .LookupNodes(target)
                .Select(node => ExpressionBuilder.Unwrap(node.Value))
                .OfType<SubjectExpressionBuilder>()
                .FirstOrDefault(declaration => declaration.Name == name);
            if (subject is null)
                return null;

            var readOnly = source.GetReadOnlyElements().Contains(subject);
            return new SubjectDefinition(source, subject, readOnly);
        }

        public static IEnumerable<ExpressionBuilder> GetSubjectReferences(this SubjectDefinition source)
        {
            var name = source.Subject.Name;
            var stack = new Stack<(ExpressionBuilderGraph Workflow, SubjectExpressionBuilder Declaration)>();
            stack.Push((source.WorkflowBuilder.Workflow, null));

            while (stack.Count > 0)
            {
                var (workflow, outerDeclaration) = stack.Pop();
                var localElements = workflow
                    .LocalNodes()
                    .Select(node => ExpressionBuilder.Unwrap(node.Value))
                    .ToList();
                var declaration = localElements
                    .OfType<SubjectExpressionBuilder>()
                    .FirstOrDefault(subject => subject.Name == name) ?? outerDeclaration;

                foreach (var element in localElements)
                {
                    if (declaration == source.Subject && element.MatchSubjectReference(name))
                        yield return element;

                    if (element is WorkflowExpressionBuilder workflowBuilder && workflowBuilder.Workflow is not null)
                        stack.Push((workflowBuilder.Workflow, declaration));
                }
            }
        }
    }

    class SubjectDefinition
    {
        public SubjectDefinition(WorkflowBuilder workflowBuilder, SubjectExpressionBuilder subject, bool readOnly)
        {
            WorkflowBuilder = workflowBuilder;
            Subject = subject;
            IsReadOnly = readOnly;
        }

        public WorkflowBuilder WorkflowBuilder { get; }

        public SubjectExpressionBuilder Subject { get; }

        public bool IsReadOnly { get; }
    }
}
