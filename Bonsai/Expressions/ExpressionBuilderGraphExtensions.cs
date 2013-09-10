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
            foreach (var mapping in from node in source
                                    where !(node.Value is InspectBuilder)
                                    let inspectBuilder = node.Successors.Single().Node.Value as InspectBuilder
                                    where inspectBuilder != null
                                    select new { node.Value, inspectBuilder })
            {
                var builder = mapping.Value;
                yield return mapping.inspectBuilder.Output
                    .Merge()
                    .IgnoreElements()
                    .Select(xs => Unit.Default)
                    .Catch<Unit, Exception>(xs => Observable.Throw<Unit>(BuildRuntimeExceptionStack(xs.Message, builder, xs, callStack)));

                var workflowExpression = mapping.Value as WorkflowExpressionBuilder;
                if (workflowExpression != null)
                {
                    foreach (var error in workflowExpression.Workflow.InspectErrors(Enumerable.Repeat(workflowExpression, 1).Concat(callStack)))
                    {
                        yield return error;
                    }
                }
            }
        }

        public static Type ExpressionType(this ExpressionBuilderGraph source, Node<ExpressionBuilder, ExpressionBuilderParameter> node)
        {
            if (!source.Contains(node))
            {
                throw new ArgumentException("The specified node is not a member of the graph.", "node");
            }

            foreach (var expressionNode in source.TopologicalSort())
            {
                var expression = expressionNode.Value.Build();
                if (expressionNode == node) return expression.Type;
                else
                {
                    foreach (var successor in expressionNode.Successors)
                    {
                        var target = successor.Node.Value.GetType().GetProperty(successor.Label.Value);
                        target.SetValue(successor.Node.Value, expression, null);
                    }
                }
            }

            throw new ArgumentException("Cannot infer expression type on cyclic graphs.", "source");
        }

        internal static IEnumerable<object> GetWorkflowElements(this ExpressionBuilder expressionBuilder)
        {
            yield return expressionBuilder;
            var element = ExpressionBuilder.GetWorkflowElement(expressionBuilder);
            if (element != expressionBuilder) yield return element;
        }

        public static Expression Build(this ExpressionBuilderGraph source)
        {
            return Build(source, null);
        }

        public static Expression Build(this ExpressionBuilderGraph source, ExpressionBuilder buildTarget)
        {
            WorkflowOutputBuilder workflowOutput = null;
            var publishMap = new List<PublishScope>();
            var connections = new List<Expression>();

            foreach (var node in source.TopologicalSort())
            {
                Expression expression;
                var builder = node.Value;
                try { expression = builder.Build(); }
                catch (Exception e)
                {
                    throw new WorkflowBuildException(e.Message, builder, e);
                }

                if (builder == buildTarget) return expression;
                PublishScope publishScope = null;
                if (node.Successors.Count > 1)
                {
                    // Start a new publish scope
                    publishScope = new PublishScope(expression);
                    expression = publishScope.PublishedSource;
                    publishMap.Insert(0, publishScope);
                }

                // Remove all closing scopes
                publishMap.RemoveAll(scope =>
                {
                    scope.References.RemoveAll(reference => reference == builder);
                    if (scope != publishScope && scope.References.Count == 0)
                    {
                        expression = scope.Close(expression);
                        return true;
                    }

                    if (node.Successors.Count == 0) scope.References.Add(null);
                    else scope.References.AddRange(node.Successors.Select(successor => successor.Node.Value));
                    return false;
                });

                foreach (var successor in node.Successors)
                {
                    var target = successor.Node.Value.GetType().GetProperty(successor.Label.Value, BindingFlags.NonPublic | BindingFlags.Instance);
                    target.SetValue(successor.Node.Value, expression, null);
                }

                if (node.Successors.Count == 0)
                {
                    connections.Add(expression);
                }

                var outputBuilder = builder as WorkflowOutputBuilder;
                if (outputBuilder != null)
                {
                    if (workflowOutput != null)
                    {
                        throw new WorkflowBuildException("Workflows cannot have more than one output.", builder);
                    }
                    workflowOutput = outputBuilder;
                }
            }

            var output = ExpressionBuilder.BuildOutput(workflowOutput, connections);
            publishMap.RemoveAll(scope =>
            {
                output = scope.Close(output);
                return true;
            });
            return output;
        }

        public static IObservable<Unit> BuildObservable(this ExpressionBuilderGraph source)
        {
            var workflow = source.Build();
            var unitConversion = new UnitBuilder { Source = workflow }.Build();
            var observableFactory = Expression.Lambda<Func<IObservable<Unit>>>(unitConversion).Compile();
            return observableFactory();
        }

        public static ExpressionBuilderGraph ToInspectableGraph(this ExpressionBuilderGraph source)
        {
            var observableMapping = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderParameter>, Tuple<Node<ExpressionBuilder, ExpressionBuilderParameter>, Node<ExpressionBuilder, ExpressionBuilderParameter>>>();
            var observableGraph = new ExpressionBuilderGraph();
            foreach (var node in source)
            {
                var observableExpression = new InspectBuilder();
                var observableNode = new Node<ExpressionBuilder, ExpressionBuilderParameter>(observableExpression);

                ExpressionBuilder nodeValue = node.Value;
                var workflowExpression = nodeValue as WorkflowExpressionBuilder;
                if (workflowExpression != null)
                {
                    var observableWorkflowExpression = (WorkflowExpressionBuilder)Activator.CreateInstance(workflowExpression.GetType(), workflowExpression.Workflow.ToInspectableGraph());
                    observableWorkflowExpression.Name = workflowExpression.Name;
                    nodeValue = observableWorkflowExpression;
                }

                var expressionNode = new Node<ExpressionBuilder, ExpressionBuilderParameter>(nodeValue);
                observableGraph.Add(expressionNode);
                observableGraph.Add(observableNode);
                observableGraph.AddEdge(expressionNode, observableNode, new ExpressionBuilderParameter());
                observableMapping.Add(node, Tuple.Create(expressionNode, observableNode));
            }

            foreach (var node in source)
            {
                var observableNode = observableMapping[node].Item2;
                foreach (var successor in node.Successors)
                {
                    var successorNode = observableMapping[successor.Node].Item1;
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

        public static ExpressionBuilderGraph FromInspectableGraph(this ExpressionBuilderGraph source, bool recurse)
        {
            var workflow = new ExpressionBuilderGraph();
            var nodeMapping = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderParameter>, Node<ExpressionBuilder, ExpressionBuilderParameter>>();
            foreach (var node in source.Where(node => !(node.Value is InspectBuilder)))
            {
                var inspectNode = node.Successors.Single().Node;

                ExpressionBuilder nodeValue = node.Value;
                var workflowExpression = recurse ? nodeValue as WorkflowExpressionBuilder : null;
                if (workflowExpression != null)
                {
                    var workflowName = workflowExpression.Name;
                    workflowExpression = (WorkflowExpressionBuilder)Activator.CreateInstance(workflowExpression.GetType(), workflowExpression.Workflow.FromInspectableGraph());
                    workflowExpression.Name = workflowName;
                    nodeValue = workflowExpression;
                }

                var sourceNode = new Node<ExpressionBuilder, ExpressionBuilderParameter>(nodeValue);
                workflow.Add(sourceNode);
                nodeMapping.Add(node, sourceNode);
            }

            foreach (var node in source.Where(node => !(node.Value is InspectBuilder)))
            {
                var inspectNode = node.Successors.Single().Node;
                foreach (var successor in inspectNode.Successors)
                {
                    var sourceNode = nodeMapping[node];
                    var targetNode = nodeMapping[successor.Node];
                    var parameter = new ExpressionBuilderParameter(successor.Label.Value);
                    workflow.AddEdge(sourceNode, targetNode, parameter);
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
