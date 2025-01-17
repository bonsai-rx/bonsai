using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Dag;
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

        public static ExpressionBuilder Find(
            this WorkflowBuilder source,
            Func<ExpressionBuilder, bool> predicate,
            ExpressionBuilder current,
            bool findPrevious)
        {
            var matches = TopologicalOrder(source.Workflow);
            if (current != null)
            {
                if (findPrevious) matches = matches.TakeWhile(builder => builder != current);
                else matches = matches.SkipWhile(builder => builder != current).Skip(1);
            }
            return matches.FirstOrDefault(predicate);
        }

        public static IEnumerable<ExpressionBuilder> FindAll(
            this WorkflowBuilder source,
            Func<ExpressionBuilder, bool> predicate)
        {
            return TopologicalOrder(source.Workflow).Where(predicate);
        }

        static IEnumerable<ExpressionBuilder> TopologicalOrder(ExpressionBuilderGraph workflow)
        {
            foreach (var node in workflow.TopologicalSort())
            {
                var builder = ExpressionBuilder.Unwrap(node.Value);
                yield return builder;

                if (builder is IWorkflowExpressionBuilder workflowBuilder)
                {
                    foreach (var result in TopologicalOrder(workflowBuilder.Workflow))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}
