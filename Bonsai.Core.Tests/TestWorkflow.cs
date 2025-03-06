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

        private TestWorkflow(
            ExpressionBuilderGraph workflow,
            Node<ExpressionBuilder, ExpressionBuilderArgument> cursor,
            int argumentIndex = 0)
        {
            Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
            ArgumentIndex = argumentIndex;
            Cursor = cursor;
        }

        public ExpressionBuilderGraph Workflow { get; }

        public Node<ExpressionBuilder, ExpressionBuilderArgument> Cursor { get; }

        public int ArgumentIndex { get; }

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
            return new TestWorkflow(Workflow, node, argumentIndex: 1);
        }

        public TestWorkflow AddArguments(params TestWorkflow[] arguments)
        {
            var argumentIndex = ArgumentIndex;
            for (int i = 0; i < arguments.Length; i++)
            {
                Workflow.AddEdge(arguments[i].Cursor, Cursor, new ExpressionBuilderArgument(argumentIndex++));
            }
            return new TestWorkflow(Workflow, Cursor, argumentIndex);
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

        public TestWorkflow AppendNamed(string name)
        {
            return Append(new GroupWorkflowBuilder { Name = name });
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

        public TestWorkflow AppendBranch(Func<TestWorkflow, TestWorkflow> selector)
        {
            return selector(this);
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

        public TestWorkflow AppendSubject<TSubjectBuilder>(string name)
            where TSubjectBuilder : SubjectExpressionBuilder, new()
        {
            var subjectBuilder = new TSubjectBuilder { Name = name };
            return Append(subjectBuilder);
        }

        public TestWorkflow TopologicalSort()
        {
            var workflow = new ExpressionBuilderGraph();
            workflow.InsertRange(0, Workflow.TopologicalSort());
            return new TestWorkflow(workflow, Cursor);
        }

        public ExpressionBuilderGraph ToInspectableGraph()
        {
            return Workflow.ToInspectableGraph();
        }

        public IObservable<T> BuildObservable<T>()
        {
            return Workflow.BuildObservable<T>();
        }
    }
}
