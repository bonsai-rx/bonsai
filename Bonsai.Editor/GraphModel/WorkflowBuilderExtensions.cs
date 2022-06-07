using System.Collections.Generic;
using System.Linq;
using Bonsai.Expressions;

namespace Bonsai.Editor.GraphModel
{
    static class WorkflowBuilderExtensions
    {
        static bool IsGroup(IWorkflowExpressionBuilder builder)
        {
            return builder is IncludeWorkflowBuilder || builder is GroupWorkflowBuilder;
        }

        static IEnumerable<ExpressionBuilder> SelectContextElements(ExpressionBuilderGraph source)
        {
            foreach (var node in source)
            {
                var element = ExpressionBuilder.Unwrap(node.Value);
                yield return element;

                if (element is IWorkflowExpressionBuilder workflowBuilder && IsGroup(workflowBuilder))
                {
                    var workflow = workflowBuilder.Workflow;
                    if (workflow == null) continue;
                    foreach (var groupElement in SelectContextElements(workflow))
                    {
                        yield return groupElement;
                    }
                }
            }
        }

        static bool GetCallContext(ExpressionBuilderGraph source, ExpressionBuilderGraph target, Stack<ExpressionBuilderGraph> context)
        {
            context.Push(source);
            if (source == target)
            {
                return true;
            }

            foreach (var element in SelectContextElements(source))
            {
                var groupBuilder = element as IWorkflowExpressionBuilder;
                if (IsGroup(groupBuilder) && groupBuilder.Workflow == target)
                {
                    return true;
                }

                if (element is WorkflowExpressionBuilder workflowBuilder)
                {
                    if (GetCallContext(workflowBuilder.Workflow, target, context))
                    {
                        return true;
                    }
                }
            }

            context.Pop();
            return false;
        }

        public static SubjectDeclaration GetSubjectDeclaration(this WorkflowBuilder source, ExpressionBuilderGraph target, string name)
        {
            var callContext = new Stack<ExpressionBuilderGraph>();
            if (GetCallContext(source.Workflow, target, callContext))
            {
                return (from level in callContext
                        from element in SelectContextElements(level)
                        let subjectBuilder = element as SubjectExpressionBuilder
                        where subjectBuilder != null && subjectBuilder.Name == name
                        select new SubjectDeclaration(level, subjectBuilder))
                        .LastOrDefault();
            }

            return null;
        }

        public static IEnumerable<ContextGrouping> GetDependentExpressions(this ExpressionBuilderGraph source, SubjectExpressionBuilder subject)
        {
            var callContext = new Stack<ExpressionBuilderGraph>();
            callContext.Push(source);

            while (callContext.Count > 0)
            {
                var root = callContext.Pop();

                var exclude = false;
                var snapshot = callContext.Count;
                foreach (var element in SelectContextElements(root))
                {
                    if (element is SubjectExpressionBuilder subjectDeclaration &&
                        subjectDeclaration != subject &&
                        subjectDeclaration.Name == subject.Name)
                    {
                        exclude = true;
                        while (callContext.Count > snapshot) callContext.Pop();
                        break;
                    }

                    if (element is WorkflowExpressionBuilder workflowBuilder)
                    {
                        callContext.Push(workflowBuilder.Workflow);
                    }
                }

                if (!exclude)
                {
                    yield return new ContextGrouping(root, SelectContextElements(root));
                }
            }
        }
    }

    class SubjectDeclaration
    {
        public SubjectDeclaration(ExpressionBuilderGraph root, SubjectExpressionBuilder subject)
        {
            Root = root;
            Subject = subject;
        }

        public ExpressionBuilderGraph Root { get; }

        public SubjectExpressionBuilder Subject { get; }
    }

    class ContextGrouping : IGrouping<ExpressionBuilderGraph, ExpressionBuilder>
    {
        public ContextGrouping(ExpressionBuilderGraph key, IEnumerable<ExpressionBuilder> elements)
        {
            Key = key;
            Elements = elements;
        }

        public ExpressionBuilderGraph Key { get; }

        IEnumerable<ExpressionBuilder> Elements { get; }

        public IEnumerator<ExpressionBuilder> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
