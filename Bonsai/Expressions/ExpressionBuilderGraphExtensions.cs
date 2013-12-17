using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace Bonsai.Expressions
{
    public static class ExpressionBuilderGraphExtensions
    {
        static WorkflowException BuildRuntimeExceptionStack(string message, ExpressionBuilder builder, Exception innerException, IEnumerable<ExpressionBuilder> callStack)
        {
            var exception = new WorkflowRuntimeException(message, builder, innerException);
            foreach (var caller in callStack)
            {
                exception = new WorkflowRuntimeException(message, caller, exception);
            }

            return exception;
        }

        public static IObservable<Unit> InspectErrors(this ExpressionBuilderGraph source)
        {
            return InspectErrors(source, Enumerable.Empty<ExpressionBuilder>()).Merge(Scheduler.Immediate);
        }

        static IEnumerable<IObservable<Unit>> InspectErrors(this ExpressionBuilderGraph source, IEnumerable<ExpressionBuilder> callStack)
        {
            foreach (var builder in from node in source
                                    let inspectBuilder = node.Value as InspectBuilder
                                    where inspectBuilder != null
                                    select inspectBuilder)
            {
                var inspectBuilder = builder;
                yield return inspectBuilder.Output
                    .Merge()
                    .IgnoreElements()
                    .Select(xs => Unit.Default)
                    .Catch<Unit, Exception>(xs => Observable.Throw<Unit>(BuildRuntimeExceptionStack(xs.Message, inspectBuilder, xs, callStack)));

                var workflowExpression = inspectBuilder.Builder as WorkflowExpressionBuilder;
                if (workflowExpression != null)
                {
                    foreach (var error in workflowExpression.Workflow.InspectErrors(Enumerable.Repeat(inspectBuilder, 1).Concat(callStack)))
                    {
                        yield return error;
                    }
                }
            }
        }

        internal static IEnumerable<object> GetWorkflowElements(this ExpressionBuilder expressionBuilder)
        {
            yield return expressionBuilder;
            var element = ExpressionBuilder.GetWorkflowElement(expressionBuilder);
            if (element != expressionBuilder) yield return element;
        }

        internal static void ClearArguments(this ExpressionBuilderGraph source)
        {
            foreach (var node in source)
            {
                node.Value.Arguments.Clear();
            }
        }

        public static Expression Build(this ExpressionBuilderGraph source)
        {
            return Build(source, (BuildContext)null);
        }

        public static Expression Build(this ExpressionBuilderGraph source, ExpressionBuilder buildTarget)
        {
            if (buildTarget == null)
            {
                throw new ArgumentNullException("buildTarget");
            }

            var buildContext = new BuildContext(buildTarget);
            Build(source, buildContext);
            return buildContext.BuildResult;
        }

        internal static Expression Build(this ExpressionBuilderGraph source, BuildContext buildContext)
        {
            WorkflowOutputBuilder workflowOutput = null;
            var multicastMap = new List<MulticastScope>();
            var connections = new List<Expression>();

            try
            {
                foreach (var node in source.TopologicalSort())
                {
                    Expression expression;
                    var builder = node.Value;
                    var argumentRange = builder.ArgumentRange;
                    if (argumentRange == null || builder.Arguments.Count < argumentRange.LowerBound)
                    {
                        throw new WorkflowBuildException("Unsupported number of arguments. Check the number of connections into node.", builder);
                    }

                    // Propagate build target in case of a nested workflow
                    var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                    var workflowBuilder = workflowElement as WorkflowExpressionBuilder;
                    if (workflowBuilder != null)
                    {
                        workflowBuilder.BuildContext = buildContext;
                    }

                    try
                    {
                        expression = builder.Build();
                        builder.Arguments.Clear();
                    }
                    catch (Exception e)
                    {
                        throw new WorkflowBuildException(e.Message, builder, e);
                    }
                    finally
                    {
                        if (workflowBuilder != null)
                        {
                            workflowBuilder.BuildContext = null;
                        }
                    }

                    // Check if build target was reached
                    if (buildContext != null)
                    {
                        if (builder == buildContext.BuildTarget)
                        {
                            buildContext.BuildResult = expression;
                        }

                        if (buildContext.BuildResult != null)
                        {
                            source.ClearArguments();
                            return expression;
                        }
                    }

                    MulticastScope multicastScope = null;
                    if (node.Successors.Count > 1)
                    {
                        // Start a new multicast scope
                        var multicastBuilder = workflowElement as MulticastExpressionBuilder;
                        if (multicastBuilder == null)
                        {
                            multicastBuilder = new PublishBuilder();
                            multicastBuilder.Arguments.Add(ExpressionBuilderParameter.Source, expression);
                            expression = multicastBuilder.Build();
                        }

                        multicastScope = new MulticastScope(multicastBuilder);
                        multicastMap.Insert(0, multicastScope);
                    }

                    // Remove all closing scopes
                    multicastMap.RemoveAll(scope =>
                    {
                        scope.References.RemoveAll(reference => reference == builder);
                        if (scope != multicastScope && scope.References.Count == 0)
                        {
                            expression = scope.Close(expression);
                            return true;
                        }

                        if (node.Successors.Count == 0) scope.References.Add(null);
                        else scope.References.AddRange(node.Successors.Select(successor => successor.Target.Value));
                        return false;
                    });

                    foreach (var successor in node.Successors)
                    {
                        successor.Target.Value.Arguments.Add(successor.Label.Value, expression);
                    }

                    if (node.Successors.Count == 0)
                    {
                        connections.Add(expression);
                    }

                    var outputBuilder = workflowElement as WorkflowOutputBuilder;
                    if (outputBuilder != null)
                    {
                        if (workflowOutput != null)
                        {
                            throw new WorkflowBuildException("Workflows cannot have more than one output.", builder);
                        }
                        workflowOutput = outputBuilder;
                    }
                }

                var output = ExpressionBuilder.BuildWorkflowOutput(workflowOutput, connections);
                multicastMap.RemoveAll(scope =>
                {
                    output = scope.Close(output);
                    return true;
                });
                return output;
            }
            catch
            {
                source.ClearArguments();
                throw;
            }
        }

        public static IObservable<Unit> BuildObservable(this ExpressionBuilderGraph source)
        {
            var workflow = source.Build();
            var unitConversion = new UnitBuilder { Arguments = { { "Source", workflow } } }.Build();
            var observableFactory = Expression.Lambda<Func<IObservable<Unit>>>(unitConversion).Compile();
            return observableFactory();
        }

        static WorkflowExpressionBuilder Clone(this WorkflowExpressionBuilder builder, ExpressionBuilderGraph workflow)
        {
            var propertyMappings = builder.PropertyMappings;
            var workflowExpression = (WorkflowExpressionBuilder)Activator.CreateInstance(builder.GetType(), workflow);
            workflowExpression.Name = builder.Name;
            foreach (var mapping in builder.PropertyMappings)
            {
                workflowExpression.PropertyMappings.Add(mapping);
            }
            return workflowExpression;
        }

        public static ExpressionBuilderGraph ToInspectableGraph(this ExpressionBuilderGraph source)
        {
            var observableMapping = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderParameter>, Node<ExpressionBuilder, ExpressionBuilderParameter>>();
            var observableGraph = new ExpressionBuilderGraph();
            foreach (var node in source)
            {
                ExpressionBuilder nodeValue = node.Value;
                var workflowExpression = nodeValue as WorkflowExpressionBuilder;
                if (workflowExpression != null)
                {
                    nodeValue = workflowExpression.Clone(workflowExpression.Workflow.ToInspectableGraph());
                }

                var observableNode = observableGraph.Add(new InspectBuilder(nodeValue));
                observableMapping.Add(node, observableNode);
            }

            foreach (var node in source)
            {
                var observableNode = observableMapping[node];
                foreach (var successor in node.Successors)
                {
                    var successorNode = observableMapping[successor.Target];
                    var parameter = new ExpressionBuilderParameter(successor.Label.Value);
                    observableGraph.AddEdge(observableNode, successorNode, parameter);
                }
            }

            return observableGraph;
        }

        public static ExpressionBuilderGraph FromInspectableGraph(this ExpressionBuilderGraph source)
        {
            return FromInspectableGraph(source, true);
        }

        public static ExpressionBuilderGraph FromInspectableGraph(this IEnumerable<Node<ExpressionBuilder, ExpressionBuilderParameter>> source, bool recurse)
        {
            var workflow = new ExpressionBuilderGraph();
            var nodeMapping = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderParameter>, Node<ExpressionBuilder, ExpressionBuilderParameter>>();
            foreach (var node in source)
            {
                InspectBuilder inspectBuilder = (InspectBuilder)node.Value;
                ExpressionBuilder nodeValue = inspectBuilder.Builder;
                var workflowExpression = recurse ? nodeValue as WorkflowExpressionBuilder : null;
                if (workflowExpression != null)
                {
                    nodeValue = workflowExpression.Clone(workflowExpression.Workflow.FromInspectableGraph());
                }

                var builderNode = workflow.Add(nodeValue);
                nodeMapping.Add(node, builderNode);
            }

            foreach (var node in source)
            {
                var sourceNode = node;
                var builderNode = nodeMapping[sourceNode];
                foreach (var successor in sourceNode.Successors)
                {
                    Node<ExpressionBuilder, ExpressionBuilderParameter> targetNode;
                    if (nodeMapping.TryGetValue(successor.Target, out targetNode))
                    {
                        workflow.AddEdge(builderNode, targetNode, successor.Label);
                    }
                }
            }

            return workflow;
        }

        public static ExpressionBuilderGraphDescriptor ToDescriptor(this ExpressionBuilderGraph source)
        {
            var descriptor = new ExpressionBuilderGraphDescriptor();
            source.ToDescriptor(descriptor);
            return descriptor;
        }
    }
}
