using System.Collections.Generic;
using System.Linq;
using Bonsai.Expressions;

namespace Bonsai.Editor.GraphModel
{
    static class WorkflowBuilderExtensions
    {
        public static bool IsGroup(this IWorkflowExpressionBuilder builder)
        {
            return builder is IncludeWorkflowBuilder || builder is GroupWorkflowBuilder;
        }

        static bool GetCallContext(ExpressionBuilderGraph source, bool readOnly, ExpressionBuilderGraph target, Stack<ContextGrouping> callContext)
        {
            var context = new ContextGrouping(source, readOnly);
            callContext.Push(context);
            if (source == target)
            {
                return true;
            }

            foreach (var element in context)
            {
                var groupBuilder = element.Builder as IWorkflowExpressionBuilder;
                if (IsGroup(groupBuilder) && groupBuilder.Workflow == target)
                {
                    return true;
                }

                if (element.Builder is WorkflowExpressionBuilder workflowBuilder)
                {
                    if (GetCallContext(workflowBuilder.Workflow, element.IsReadOnly, target, callContext))
                    {
                        return true;
                    }
                }
            }

            callContext.Pop();
            return false;
        }

        public static SubjectDefinition GetSubjectDefinition(this WorkflowBuilder source, ExpressionBuilderGraph target, string name)
        {
            var callContext = new Stack<ContextGrouping>();
            if (GetCallContext(source.Workflow, readOnly: false, target, callContext))
            {
                return (from level in callContext
                        from element in level
                        let subjectBuilder = element.Builder as SubjectExpressionBuilder
                        where subjectBuilder != null && subjectBuilder.Name == name
                        select new SubjectDefinition(level, subjectBuilder, element.IsReadOnly))
                        .FirstOrDefault();
            }

            return null;
        }

        public static IEnumerable<ContextGrouping> GetDependentExpressions(this SubjectDefinition source)
        {
            var callContext = new Stack<ContextGrouping>();
            callContext.Push(source.Root);

            while (callContext.Count > 0)
            {
                var root = callContext.Pop();

                var exclude = false;
                var snapshot = callContext.Count;
                foreach (var element in root)
                {
                    if (element.Builder is SubjectExpressionBuilder subjectDefinition &&
                        subjectDefinition != source.Subject &&
                        subjectDefinition.Name == source.Subject.Name)
                    {
                        exclude = true;
                        while (callContext.Count > snapshot) callContext.Pop();
                        break;
                    }

                    if (element.Builder is WorkflowExpressionBuilder workflowBuilder)
                    {
                        callContext.Push(new ContextGrouping(workflowBuilder.Workflow, element.IsReadOnly));
                    }
                }

                if (!exclude)
                {
                    yield return root;
                }
            }
        }
    }

    class SubjectDefinition
    {
        public SubjectDefinition(ContextGrouping root, SubjectExpressionBuilder subject, bool readOnly)
        {
            Root = root;
            Subject = subject;
            IsReadOnly = readOnly;
        }

        public ContextGrouping Root { get; }

        public SubjectExpressionBuilder Subject { get; }

        public bool IsReadOnly { get; }
    }

    struct ContextElement
    {
        public ExpressionBuilder Builder;
        public bool IsReadOnly;

        public ContextElement(ExpressionBuilder element, bool readOnly)
        {
            Builder = element;
            IsReadOnly = readOnly;
        }
    }

    class ContextGrouping : IGrouping<ExpressionBuilderGraph, ContextElement>
    {
        public ContextGrouping(ExpressionBuilderGraph key, bool readOnly)
        {
            Key = key;
            IsReadOnly = readOnly;
            Elements = SelectContextElements(key, readOnly);
        }

        public ExpressionBuilderGraph Key { get; }

        public bool IsReadOnly { get; }

        IEnumerable<ContextElement> Elements { get; }

        public IEnumerator<ContextElement> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        static IEnumerable<ContextElement> SelectContextElements(ExpressionBuilderGraph source, bool readOnly)
        {
            foreach (var node in source)
            {
                var builder = ExpressionBuilder.Unwrap(node.Value);
                yield return new ContextElement(builder, readOnly);

                if (builder is IWorkflowExpressionBuilder workflowBuilder && workflowBuilder.IsGroup())
                {
                    var workflow = workflowBuilder.Workflow;
                    if (workflow == null) continue;
                    foreach (var groupElement in SelectContextElements(workflow, readOnly || workflowBuilder is IncludeWorkflowBuilder))
                    {
                        yield return groupElement;
                    }
                }
            }
        }
    }
}
