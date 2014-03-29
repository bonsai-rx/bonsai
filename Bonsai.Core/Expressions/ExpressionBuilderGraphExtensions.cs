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
using System.ComponentModel;
using Bonsai.Properties;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a set of static methods for serializing, building and otherwise manipulating
    /// expression builder workflows.
    /// </summary>
    public static class ExpressionBuilderGraphExtensions
    {
        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of zero or more
        /// input arguments. The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="builder">The expression builder.</param>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public static Expression Build(this ExpressionBuilder builder, params Expression[] arguments)
        {
            return builder.Build(arguments);
        }

        /// <summary>
        /// Sets the value of a workflow property to a different value.
        /// </summary>
        /// <param name="source">The expression builder workflow for which to set the property.</param>
        /// <param name="name">The name of the workflow property.</param>
        /// <param name="value">The new value.</param>
        public static void SetWorkflowProperty(this ExpressionBuilderGraph source, string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The workflow property name cannot be null or whitespace.", "name");
            }

            var memberChain = name.Split(new[] { ExpressionHelper.MemberSeparator }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < memberChain.Length - 1; i++)
            {
                var workflowBuilder = (from node in source
                                       let builder = ExpressionBuilder.Unwrap(node.Value) as WorkflowExpressionBuilder
                                       where builder != null && builder.Name == memberChain[i]
                                       select builder)
                                       .FirstOrDefault();
                if (workflowBuilder == null)
                {
                    throw new KeyNotFoundException(string.Format(Resources.Exception_PropertyNotFound, name));
                }

                source = workflowBuilder.Workflow;
            }

            name = memberChain[memberChain.Length - 1];
            var property = (from node in source
                            let workflowProperty = ExpressionBuilder.GetWorkflowElement(node.Value) as WorkflowProperty
                            where workflowProperty != null && workflowProperty.Name == name
                            select workflowProperty)
                            .FirstOrDefault();
            if (property == null)
            {
                throw new KeyNotFoundException(string.Format(Resources.Exception_PropertyNotFound, name));
            }

            var propertyDescriptor = TypeDescriptor.GetProperties(property).Find("Value", false);
            var propertyValue = propertyDescriptor.Converter.ConvertFrom(value);
            propertyDescriptor.SetValue(property, propertyValue);
        }

        static WorkflowException BuildRuntimeExceptionStack(string message, ExpressionBuilder builder, Exception innerException, IEnumerable<ExpressionBuilder> callStack)
        {
            var exception = new WorkflowRuntimeException(message, builder, innerException);
            foreach (var caller in callStack)
            {
                exception = new WorkflowRuntimeException(message, caller, exception);
            }

            return exception;
        }

        /// <summary>
        /// Redirects any build or execution errors signaled by <see cref="InspectBuilder"/> nodes in
        /// the specified expression builder workflow into an empty observable sequence.
        /// </summary>
        /// <param name="source">The expression builder workflow for which to redirect errors.</param>
        /// <returns>
        /// An observable sequence with no elements except for error termination messages.
        /// </returns>
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
                yield return inspectBuilder.Error
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

        /// <summary>
        /// Generates an expression tree from the specified expression builder workflow.
        /// </summary>
        /// <param name="source">
        /// The expression builder workflow for which to generate the expression tree.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> tree representing the evaluation of the full
        /// expression builder workflow.
        /// </returns>
        public static Expression Build(this ExpressionBuilderGraph source)
        {
            return Build(source, (BuildContext)null);
        }

        /// <summary>
        /// Generates an expression tree from the specified expression builder workflow
        /// evaluated up to the specified build target.
        /// </summary>
        /// <param name="source">
        /// The expression builder workflow for which to generate the expression tree.
        /// </param>
        /// <param name="buildTarget">
        /// The expression builder node up to which the workflow will be evaluated.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> tree representing the evaluation of the expression
        /// builder workflow up to the specified <paramref name="buildTarget"/>.
        /// </returns>
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

        static readonly Expression[] EmptyArguments = new Expression[0];

        static void RegisterElementName(ExpressionBuilder builder, INamedElement element, ref HashSet<string> namedElements)
        {
            if (namedElements == null) namedElements = new HashSet<string>();
            if (!namedElements.Add(element.Name))
            {
                throw new WorkflowBuildException("A workflow element with the specified name already exists.", builder);
            }
        }

        internal static Expression Build(this ExpressionBuilderGraph source, BuildContext buildContext)
        {
            Expression workflowOutput = null;
            HashSet<string> namedElements = null;
            var argumentLists = new Dictionary<ExpressionBuilder, SortedList<int, Expression>>();
            var multicastMap = new List<MulticastScope>();
            var connections = new List<Expression>();

            foreach (var node in source.TopologicalSort())
            {
                Expression expression;
                IList<Expression> arguments;
                SortedList<int, Expression> argumentList;

                var builder = node.Value;
                if (argumentLists.TryGetValue(builder, out argumentList))
                {
                    arguments = argumentList.Values;
                    argumentLists.Remove(builder);
                }
                else arguments = EmptyArguments;

                var argumentRange = builder.ArgumentRange;
                if (argumentRange == null || arguments.Count < argumentRange.LowerBound)
                {
                    throw new WorkflowBuildException(
                        string.Format("Unsupported number of arguments. This node requires at least {0} input connections.", argumentRange.LowerBound),
                        builder);
                }

                // Propagate build target in case of a nested workflow
                var workflowElement = ExpressionBuilder.Unwrap(builder);
                var workflowBuilder = workflowElement as WorkflowExpressionBuilder;
                if (workflowBuilder != null)
                {
                    workflowBuilder.BuildContext = buildContext;
                    if (!string.IsNullOrEmpty(workflowBuilder.Name))
                    {
                        RegisterElementName(builder, workflowBuilder, ref namedElements);
                    }
                }

                var workflowProperty = ExpressionBuilder.GetWorkflowElement(workflowElement) as WorkflowProperty;
                if (workflowProperty != null && !string.IsNullOrEmpty(workflowProperty.Name))
                {
                    RegisterElementName(builder, workflowProperty, ref namedElements);
                }

                try
                {
                    expression = builder.Build(arguments);
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
                        return expression;
                    }
                }

                // Remove all closing scopes
                multicastMap.RemoveAll(scope =>
                {
                    scope.References.RemoveAll(reference => reference == builder);
                    if (scope.References.Count == 0)
                    {
                        expression = scope.Close(expression);
                        return true;
                    }

                    if (node.Successors.Count == 0) scope.References.Add(null);
                    else scope.References.AddRange(node.Successors.Select(successor => successor.Target.Value));
                    return false;
                });

                MulticastScope multicastScope = null;
                if (node.Successors.Count > 1)
                {
                    // Start a new multicast scope
                    var multicastBuilder = workflowElement as MulticastExpressionBuilder;
                    if (multicastBuilder == null)
                    {
                        multicastBuilder = new PublishBuilder();
                        expression = multicastBuilder.Build(expression);
                    }

                    multicastScope = new MulticastScope(multicastBuilder);
                    multicastScope.References.AddRange(node.Successors.Select(successor => successor.Target.Value));
                    multicastMap.Insert(0, multicastScope);
                }

                foreach (var successor in node.Successors)
                {
                    if (!argumentLists.TryGetValue(successor.Target.Value, out argumentList))
                    {
                        argumentList = new SortedList<int, Expression>();
                        argumentLists.Add(successor.Target.Value, argumentList);
                    }

                    argumentList.Add(successor.Label.Index, expression);
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
                    workflowOutput = expression;
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

        /// <summary>
        /// Builds and compiles an expression builder workflow into an observable that can be
        /// subscribed for its side-effects.
        /// </summary>
        /// <param name="source">The expression builder workflow to compile.</param>
        /// <returns>
        /// An observable sequence with no elements except for termination messages.
        /// </returns>
        public static IObservable<Unit> BuildObservable(this ExpressionBuilderGraph source)
        {
            var workflow = source.Build();
            var unitBuilder = new UnitBuilder();
            var unitConversion = unitBuilder.Build(workflow);
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

        /// <summary>
        /// Converts the specified expression builder workflow into an equivalent representation
        /// where all the nodes are decorated by <see cref="InspectBuilder"/> instances that allow
        /// for runtime inspection and error redirection of workflow values.
        /// </summary>
        /// <param name="source">The expression builder workflow to convert.</param>
        /// <returns>
        /// A new expression builder workflow where all nodes have been decorated by
        /// <see cref="InspectBuilder"/> instances.
        /// </returns>
        public static ExpressionBuilderGraph ToInspectableGraph(this ExpressionBuilderGraph source)
        {
            return ToInspectableGraph(source, true);
        }

        /// <summary>
        /// Converts the specified expression builder workflow into an equivalent representation
        /// where all the nodes are decorated by <see cref="InspectBuilder"/> instances that allow
        /// for runtime inspection and error redirection of workflow values.
        /// </summary>
        /// <param name="source">The expression builder workflow to convert.</param>
        /// <returns>
        /// <param name="recurse">
        /// A value indicating whether to recurse the conversion into nested workflows.
        /// </param>
        /// A new expression builder workflow where all nodes have been decorated by
        /// <see cref="InspectBuilder"/> instances.
        /// </returns>
        public static ExpressionBuilderGraph ToInspectableGraph(this ExpressionBuilderGraph source, bool recurse)
        {
            var observableMapping = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderArgument>, Node<ExpressionBuilder, ExpressionBuilderArgument>>();
            var observableGraph = new ExpressionBuilderGraph();
            foreach (var node in source)
            {
                ExpressionBuilder nodeValue = node.Value;
                var workflowExpression = recurse ? nodeValue as WorkflowExpressionBuilder : null;
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
                    var parameter = new ExpressionBuilderArgument(successor.Label.Index);
                    observableGraph.AddEdge(observableNode, successorNode, parameter);
                }
            }

            return observableGraph;
        }

        /// <summary>
        /// Converts the specified expression builder workflow into an equivalent representation
        /// where all the <see cref="InspectBuilder"/> nodes have been replaced by their decorated
        /// children.
        /// </summary>
        /// <param name="source">The expression builder workflow to convert.</param>
        /// <returns>
        /// A new expression builder workflow where all <see cref="InspectBuilder"/> nodes have
        /// been replaced by their decorated children.
        /// </returns>
        public static ExpressionBuilderGraph FromInspectableGraph(this ExpressionBuilderGraph source)
        {
            return FromInspectableGraph(source, true);
        }

        /// <summary>
        /// Converts the specified expression builder workflow into an equivalent representation
        /// where all the <see cref="InspectBuilder"/> nodes have been replaced by their decorated
        /// children.
        /// </summary>
        /// <param name="source">The expression builder workflow to convert.</param>
        /// <param name="recurse">
        /// A value indicating whether to recurse the conversion into nested workflows.
        /// </param>
        /// <returns>
        /// A new expression builder workflow where all <see cref="InspectBuilder"/> nodes have
        /// been replaced by their decorated children.
        /// </returns>
        public static ExpressionBuilderGraph FromInspectableGraph(this IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> source, bool recurse)
        {
            var workflow = new ExpressionBuilderGraph();
            var nodeMapping = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderArgument>, Node<ExpressionBuilder, ExpressionBuilderArgument>>();
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
                    Node<ExpressionBuilder, ExpressionBuilderArgument> targetNode;
                    if (nodeMapping.TryGetValue(successor.Target, out targetNode))
                    {
                        workflow.AddEdge(builderNode, targetNode, successor.Label);
                    }
                }
            }

            return workflow;
        }

        /// <summary>
        /// Converts an expression builder workflow into its serializable representation.
        /// </summary>
        /// <param name="source">The expression builder workflow to convert.</param>
        /// <returns>
        /// The serializable descriptor of the specified expression builder workflow.
        /// </returns>
        public static ExpressionBuilderGraphDescriptor ToDescriptor(this ExpressionBuilderGraph source)
        {
            var descriptor = new ExpressionBuilderGraphDescriptor();
            source.ToDescriptor(descriptor);
            return descriptor;
        }
    }
}
