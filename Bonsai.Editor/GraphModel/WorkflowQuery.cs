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
            return builder switch
            {
                SubscribeSubject subscribeSubject => subscribeSubject.Name == name,
                MulticastSubject multicastSubject => multicastSubject.Name == name,
                _ => false
            };
        }

        public static bool MatchElementType(this ExpressionBuilder builder, string typeName)
        {
            var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
            return workflowElement.GetType().AssemblyQualifiedName == typeName;
        }

        static Func<ExpressionBuilder, bool> GetSubjectMatch(
            this WorkflowBuilder workflowBuilder,
            ExpressionBuilderGraph targetWorkflow,
            string subjectName)
        {
            targetWorkflow ??= workflowBuilder.Workflow;
            var definition = workflowBuilder.GetSubjectDefinition(targetWorkflow, subjectName);
            if (definition is null)
                return _ => false;

            var references = definition
                .GetDependentExpressions()
                .SelectMany(context => context)
                .Where(element => element.Builder.MatchSubjectReference(definition.Subject.Name))
                .Select(element => ExpressionBuilder.Unwrap(element.Builder))
                .Prepend(definition.Subject)
                .ToHashSet();
            return references.Contains;
        }

        static Func<ExpressionBuilder, bool> GetElementTypeMatch(
            this WorkflowBuilder workflowBuilder,
            ExpressionBuilderGraph targetWorkflow,
            ElementCategory elementCategory,
            string key)
        {
            return elementCategory switch
            {
                ~ElementCategory.Workflow => builder => builder.MatchIncludeWorkflow(key),
                ~ElementCategory.Source => GetSubjectMatch(workflowBuilder, targetWorkflow, key),
                _ => builder => builder.MatchElementType(key),
            };
        }

        static Func<ExpressionBuilder, bool> GetElementMatch(
            this WorkflowBuilder workflowBuilder,
            ExpressionBuilderGraph targetWorkflow,
            ExpressionBuilder builder)
        {
            var matchTarget = ExpressionBuilder.Unwrap(builder);
            if (matchTarget is SubjectExpressionBuilder ||
                matchTarget is SubscribeSubject ||
                matchTarget is MulticastSubject)
            {
                var subjectName = ((INamedElement)matchTarget).Name;
                return GetSubjectMatch(workflowBuilder, targetWorkflow, subjectName);
            }
            else if (matchTarget is IncludeWorkflowBuilder includeBuilder)
            {
                return builder => builder.MatchIncludeWorkflow(includeBuilder.Path);
            }
            else
            {
                var workflowElement = ExpressionBuilder.GetWorkflowElement(matchTarget);
                var typeName = workflowElement.GetType().AssemblyQualifiedName;
                return builder => builder.MatchElementType(typeName);
            }
        }

        public static WorkflowQueryResult Find(
            this WorkflowBuilder source,
            Func<ExpressionBuilder, bool> predicate,
            ExpressionBuilder current,
            bool findPrevious)
        {
            var matches = FindAll(source, predicate);
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

        public static WorkflowQueryResult FindReference(
            this WorkflowBuilder source,
            ExpressionBuilderGraph targetWorkflow,
            ElementCategory elementCategory,
            string key,
            ExpressionBuilder current,
            bool findPrevious)
        {
            var predicate = GetElementTypeMatch(source, targetWorkflow, elementCategory, key);
            return Find(source, predicate, current, findPrevious);
        }

        public static WorkflowQueryResult FindReference(
            this WorkflowBuilder source,
            ExpressionBuilderGraph targetWorkflow,
            ExpressionBuilder current,
            bool findPrevious)
        {
            var predicate = GetElementMatch(source, targetWorkflow, current);
            return Find(source, predicate, current, findPrevious);
        }

        public static IEnumerable<WorkflowQueryResult> FindAllReferences(
            this WorkflowBuilder source,
            ExpressionBuilderGraph targetWorkflow,
            ExpressionBuilder builder)
        {
            return Query(source.Workflow, () => GetElementMatch(source, targetWorkflow, builder));
        }

        public static IEnumerable<WorkflowQueryResult> FindAllReferences(
            this WorkflowBuilder source,
            ExpressionBuilderGraph targetWorkflow,
            ElementCategory elementCategory,
            string key)
        {
            return Query(source.Workflow, () => GetElementTypeMatch(source, targetWorkflow, elementCategory, key));
        }

        static IEnumerable<WorkflowQueryResult> Query(
            ExpressionBuilderGraph workflow,
            Func<Func<ExpressionBuilder, bool>> predicateFactory)
        {
            var predicate = predicateFactory();
            foreach (var match in Query(workflow, predicate))
            {
                yield return match;
            }
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
