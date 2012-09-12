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

namespace Bonsai.Expressions
{
    public static class ExpressionBuilderGraphExtensions
    {
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

        internal static IEnumerable<LoadableElement> GetLoadableElements(this ExpressionBuilder expressionBuilder)
        {
            foreach (var property in expressionBuilder.GetType().GetProperties())
            {
                if (typeof(LoadableElement).IsAssignableFrom(property.PropertyType))
                {
                    var value = (LoadableElement)property.GetValue(expressionBuilder, null);
                    if (value != null)
                    {
                        yield return value;
                    }
                }
            }
        }

        public static ReactiveWorkflow Build(this ExpressionBuilderGraph source)
        {
            List<LoadableElement> loadableElements = new List<LoadableElement>();
            List<Expression> connections = new List<Expression>();

            foreach (var node in source.TopologicalSort())
            {
                var expression = node.Value.Build();
                loadableElements.AddRange(node.Value.GetLoadableElements());

                if (node.Successors.Count > 1)
                {
                    // Publish workflow result to avoid repeating operations
                    var publishBuilder = new PublishBuilder { Source = expression };
                    loadableElements.Add(publishBuilder.PublishHandle);
                    expression = publishBuilder.Build();
                }

                foreach (var successor in node.Successors)
                {
                    var target = successor.Node.Value.GetType().GetProperty(successor.Label.Value);
                    target.SetValue(successor.Node.Value, expression, null);
                }

                if (node.Successors.Count == 0)
                {
                    connections.Add(expression);
                }
            }

            return new ReactiveWorkflow(loadableElements, connections);
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
                observableGraph.AddEdge(expressionNode, observableNode, new ExpressionBuilderParameter("Source"));
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
