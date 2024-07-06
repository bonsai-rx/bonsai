using System;
using Bonsai.Dag;
using Bonsai.Expressions;

namespace Bonsai.Core.Tests
{
    static class Workflow
    {
        public static WorkflowCursor New()
        {
            var workflow = new ExpressionBuilderGraph();
            return FromGraph(workflow);
        }

        public static WorkflowCursor FromGraph(ExpressionBuilderGraph workflow)
        {
            return new WorkflowCursor(workflow, null);
        }

        public static WorkflowCursor FromInspectableGraph(ExpressionBuilderGraph workflow)
        {
            return FromGraph(workflow.FromInspectableGraph());
        }

        public static WorkflowCursor ResetCursor(this WorkflowCursor workflowCursor)
        {
            var (workflow, cursor) = workflowCursor;
            if (cursor != null)
                return new WorkflowCursor(workflow, null);
            else
                return workflowCursor;
        }

        public static WorkflowCursor Do(this WorkflowCursor workflowCursor, Action<ExpressionBuilder> action)
        {
            action(workflowCursor.Cursor?.Value);
            return workflowCursor;
        }

        public static WorkflowCursor Append(this WorkflowCursor workflowCursor, ExpressionBuilder builder)
        {
            var (workflow, cursor) = workflowCursor;
            var node = workflow.Add(builder);
            if (cursor != null)
                workflow.AddEdge(cursor, node, new ExpressionBuilderArgument());
            return new WorkflowCursor(workflow, node);
        }

        public static WorkflowCursor AppendCombinator<TCombinator>(this WorkflowCursor workflowCursor, TCombinator combinator)
        {
            var combinatorBuilder = new CombinatorBuilder { Combinator = combinator };
            return workflowCursor.Append(combinatorBuilder);
        }

        public static WorkflowCursor AppendValue<TValue>(this WorkflowCursor workflowCursor, TValue value)
        {
            var workflowProperty = new WorkflowProperty<TValue> { Value = value };
            return workflowCursor.AppendCombinator(workflowProperty);
        }

        public static WorkflowCursor AppendUnit(this WorkflowCursor workflowCursor)
        {
            return workflowCursor.Append(new UnitBuilder());
        }

        public static WorkflowCursor AppendInput(this WorkflowCursor workflowCursor, int index = 0)
        {
            return workflowCursor.ResetCursor().Append(new WorkflowInputBuilder { Index = index });
        }

        public static WorkflowCursor AppendOutput(this WorkflowCursor workflowCursor)
        {
            return workflowCursor.Append(new WorkflowOutputBuilder());
        }

        public static WorkflowCursor AppendPropertyMapping(this WorkflowCursor workflowCursor, params string[] propertyNames)
        {
            var mappingBuilder = new PropertyMappingBuilder();
            foreach (var name in propertyNames)
            {
                mappingBuilder.PropertyMappings.Add(new PropertyMapping { Name = name });
            }
            return workflowCursor.Append(mappingBuilder);
        }

        public static WorkflowCursor AppendNested<TWorkflowExpressionBuilder>(
            this WorkflowCursor workflowCursor,
            Func<WorkflowCursor, WorkflowCursor> selector,
            Func<ExpressionBuilderGraph, TWorkflowExpressionBuilder> constructor)
            where TWorkflowExpressionBuilder : ExpressionBuilder, IWorkflowExpressionBuilder
        {
            var (_, cursor) = workflowCursor;
            var nestedCursor = New();
            if (cursor != null)
                nestedCursor = nestedCursor.AppendInput();
            nestedCursor = selector(nestedCursor);
            var workflowBuilder = constructor(nestedCursor.Workflow);
            return workflowCursor.Append(workflowBuilder);
        }

        public static ExpressionBuilderGraph ToGraph(this WorkflowCursor workflowCursor)
        {
            return workflowCursor.Workflow;
        }

        public static ExpressionBuilderGraph ToInspectableGraph(this WorkflowCursor workflowCursor)
        {
            return workflowCursor.Workflow.ToInspectableGraph();
        }
    }

    class WorkflowCursor
    {
        public WorkflowCursor(ExpressionBuilderGraph workflow, Node<ExpressionBuilder, ExpressionBuilderArgument> cursor)
        {
            Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
            Cursor = cursor;
        }

        public ExpressionBuilderGraph Workflow { get; }

        public Node<ExpressionBuilder, ExpressionBuilderArgument> Cursor { get; }

        public void Deconstruct(out ExpressionBuilderGraph workflow, out Node<ExpressionBuilder, ExpressionBuilderArgument> cursor)
        {
            workflow = Workflow;
            cursor = Cursor;
        }
    }
}
