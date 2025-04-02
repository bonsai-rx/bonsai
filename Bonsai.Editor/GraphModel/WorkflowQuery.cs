using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Expressions;

namespace Bonsai.Editor.GraphModel
{
    static class WorkflowQuery
    {
        public static bool MatchIncludeWorkflow(this ExpressionBuilder builder, string path)
        {
            return builder is IncludeWorkflowBuilder workflowBuilder && workflowBuilder.Path == path;
        }

        public static bool MatchSubjectReference(this ExpressionBuilder builder, string name)
        {
            string referenceName;
            if (builder is SubscribeSubject subscribeSubject) referenceName = subscribeSubject.Name;
            else if (builder is MulticastSubject multicastSubject) referenceName = multicastSubject.Name;
            else return false;
            return referenceName == name;
        }

        public static bool MatchElementType(this ExpressionBuilder builder, string typeName)
        {
            var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
            return workflowElement.GetType().AssemblyQualifiedName == typeName;
        }

        public static WorkflowQueryResult Find(
            this WorkflowBuilder source,
            Func<ExpressionBuilder, bool> predicate,
            ExpressionBuilder current,
            bool findPrevious)
        {
            var matches = Query(source.Workflow, predicate);
            if (current != null)
            {
                if (findPrevious) matches = matches.TakeWhile(match => match.Builder != current);
                else matches = matches.SkipWhile(match => match.Builder != current).Skip(1);
            }
            return matches.FirstOrDefault();
        }

        public static IEnumerable<WorkflowQueryResult> FindAll(
            this WorkflowBuilder source,
            Func<ExpressionBuilder, bool> predicate)
        {
            return Query(source.Workflow, predicate);
        }

        static IEnumerable<WorkflowQueryResult> Query(
            ExpressionBuilderGraph workflow,
            Func<ExpressionBuilder, bool> predicate,
            WorkflowEditorPath parent = null)
        {
            for (int i = 0; i < workflow.Count; i++)
            {
                var node = workflow[i];
                WorkflowEditorPath path = null;
                var builder = ExpressionBuilder.Unwrap(node.Value);
                if (predicate(builder))
                    yield return new WorkflowQueryResult(node.Value, path = new WorkflowEditorPath(i, parent));

                if (builder is IWorkflowExpressionBuilder workflowBuilder)
                {
                    path ??= new WorkflowEditorPath(i, parent);
                    foreach (var result in Query(workflowBuilder.Workflow, predicate, path))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}
