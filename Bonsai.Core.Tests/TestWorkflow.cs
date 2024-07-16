using System;
using System.Linq.Expressions;
using Bonsai.Dag;
using Bonsai.Expressions;

namespace Bonsai.Core.Tests
{
    public readonly struct TestWorkflow
    {
        public TestWorkflow()
            : this(new ExpressionBuilderGraph(), null)
        {
        }

        private TestWorkflow(ExpressionBuilderGraph workflow, Node<ExpressionBuilder, ExpressionBuilderArgument> cursor)
        {
            Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
            Cursor = cursor;
        }

        public ExpressionBuilderGraph Workflow { get; }

        public Node<ExpressionBuilder, ExpressionBuilderArgument> Cursor { get; }

        public TestWorkflow ResetCursor()
        {
            if (Cursor != null)
                return new TestWorkflow(Workflow, null);
            else
                return this;
        }

        public TestWorkflow Capture(out ExpressionBuilder builder)
        {
            builder = Cursor?.Value;
            return this;
        }

        public TestWorkflow Append(ExpressionBuilder builder)
        {
            var node = Workflow.Add(builder);
            if (Cursor != null)
                Workflow.AddEdge(Cursor, node, new ExpressionBuilderArgument());
            return new TestWorkflow(Workflow, node);
        }

        public TestWorkflow AppendCombinator<TCombinator>(TCombinator combinator) where TCombinator : new()
        {
            var combinatorBuilder = new CombinatorBuilder { Combinator = combinator };
            return Append(combinatorBuilder);
        }

        public TestWorkflow AppendValue<TValue>(TValue value)
        {
            var workflowProperty = new WorkflowProperty<TValue> { Value = value };
            return AppendCombinator(workflowProperty);
        }

        public TestWorkflow AppendUnit()
        {
            return Append(new UnitBuilder());
        }

        public TestWorkflow AppendInput(int index = 0)
        {
            return ResetCursor().Append(new WorkflowInputBuilder { Index = index });
        }

        public TestWorkflow AppendOutput()
        {
            return Append(new WorkflowOutputBuilder());
        }

        public TestWorkflow AppendPropertyMapping(params string[] propertyNames)
        {
            var mappingBuilder = new PropertyMappingBuilder();
            foreach (var name in propertyNames)
            {
                mappingBuilder.PropertyMappings.Add(new PropertyMapping { Name = name });
            }
            return Append(mappingBuilder);
        }

        public TestWorkflow AppendNested<TWorkflowExpressionBuilder>(
            Func<TestWorkflow, TestWorkflow> selector,
            Func<ExpressionBuilderGraph, TWorkflowExpressionBuilder> constructor)
            where TWorkflowExpressionBuilder : ExpressionBuilder, IWorkflowExpressionBuilder
        {
            var nestedWorkflow = new TestWorkflow();
            if (Cursor != null)
                nestedWorkflow = nestedWorkflow.AppendInput();
            nestedWorkflow = selector(nestedWorkflow);
            var workflowBuilder = constructor(nestedWorkflow.Workflow);
            return Append(workflowBuilder);
        }

        public ExpressionBuilderGraph ToInspectableGraph()
        {
            return Workflow.ToInspectableGraph();
        }

        public IObservable<T> BuildObservable<T>()
        {
            var expression = Workflow.Build();
            var observableFactory = Expression.Lambda<Func<IObservable<T>>>(expression).Compile();
            return observableFactory();
        }
    }
}
