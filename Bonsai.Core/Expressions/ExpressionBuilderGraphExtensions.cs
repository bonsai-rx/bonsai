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
                var workflowBuilders = (from node in source
                                        let builder = ExpressionBuilder.Unwrap(node.Value) as WorkflowExpressionBuilder
                                        where builder != null && builder.Name == memberChain[i]
                                        select builder).ToArray();
                if (workflowBuilders.Length == 0)
                {
                    throw new KeyNotFoundException(string.Format(Resources.Exception_PropertyNotFound, name));
                }
                else if (workflowBuilders.Length > 1)
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.Exception_AmbiguousNamedElement,
                        string.Join(ExpressionHelper.MemberSeparator, memberChain, 0, i + 1)));
                }

                source = workflowBuilders[0].Workflow;
            }

            name = memberChain[memberChain.Length - 1];
            var property = (from node in source
                            let workflowProperty = ExpressionBuilder.Unwrap(node.Value) as ExternalizedProperty
                            where workflowProperty != null && workflowProperty.Name == name
                            select workflowProperty)
                            .FirstOrDefault();
            if (property == null)
            {
                throw new KeyNotFoundException(string.Format(Resources.Exception_PropertyNotFound, name));
            }

            var propertyDescriptor = TypeDescriptor.GetProperties(property).Find("Value", false);
            if (value != null && value.GetType() != propertyDescriptor.PropertyType)
            {
                value = propertyDescriptor.Converter.ConvertFrom(value);
            }

            propertyDescriptor.SetValue(property, value);
        }

        #region Error Handling

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
        [Obsolete]
        public static IObservable<Unit> InspectErrors(this ExpressionBuilderGraph source)
        {
            return InspectErrors(source, Enumerable.Empty<ExpressionBuilder>())
                .Merge(Scheduler.Immediate)
                .SelectMany(xs => Observable.Throw(xs, Unit.Default));
        }

        /// <summary>
        /// Redirects any build or execution errors signaled by <see cref="InspectBuilder"/> nodes in
        /// the specified expression builder workflow into a single observable sequence.
        /// </summary>
        /// <param name="source">The expression builder workflow for which to redirect errors.</param>
        /// <returns>
        /// An observable sequence where all elements are errors raised by
        /// <see cref="InspectBuilder"/> nodes.
        /// </returns>
        public static IObservable<Exception> InspectErrorsEx(this ExpressionBuilderGraph source)
        {
            return InspectErrors(source, Enumerable.Empty<ExpressionBuilder>()).Merge(Scheduler.Immediate);
        }

        static IEnumerable<IObservable<Exception>> InspectErrors(this ExpressionBuilderGraph source, IEnumerable<ExpressionBuilder> callStack)
        {
            foreach (var builder in from node in source
                                    let inspectBuilder = node.Value as InspectBuilder
                                    where inspectBuilder != null
                                    select inspectBuilder)
            {
                var inspectBuilder = builder;
                yield return inspectBuilder.ErrorEx.Select(xs => BuildRuntimeExceptionStack(xs.Message, inspectBuilder, xs, callStack));

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

        #endregion

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
            return Build(source, (ExpressionBuilder)null);
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
            // Add/remove build dependencies
            var buildContext = new BuildContext(buildTarget);
            var dependencies = (from link in FindBuildDependencies(source)
                                where link.Publish != null && link.Subscribe != null
                                select new { link, edge = link.Workflow.AddEdge(link.Publish, link.Subscribe, null) })
                                .ToList();
            try { return Build(source, buildContext); }
            finally
            {
                foreach (var dependency in dependencies)
                {
                    dependency.link.Workflow.RemoveEdge(dependency.link.Publish, dependency.edge);
                }
            }
        }

        #region Argument Lists

        static readonly Expression[] EmptyArguments = new Expression[0];

        static void RegisterPropertyName(ExpressionBuilder builder, INamedElement element, ref HashSet<string> namedElements)
        {
            if (namedElements == null) namedElements = new HashSet<string>();
            if (!namedElements.Add(element.Name))
            {
                throw new WorkflowBuildException("A workflow property with the specified name already exists.", builder);
            }
        }

        static IList<Expression> GetArgumentList(
            Dictionary<ExpressionBuilder, SortedList<int, Expression>> argumentLists,
            ExpressionBuilder builder)
        {
            IList<Expression> arguments;
            SortedList<int, Expression> argumentList;

            if (argumentLists.TryGetValue(builder, out argumentList))
            {
                arguments = argumentList.Values;
                argumentLists.Remove(builder);
            }
            else arguments = EmptyArguments;
            return arguments;
        }

        static void UpdateArgumentList(
            Dictionary<ExpressionBuilder, SortedList<int, Expression>> argumentLists,
            Edge<ExpressionBuilder, ExpressionBuilderArgument> successor,
            Expression expression)
        {
            SortedList<int, Expression> argumentList;
            if (!argumentLists.TryGetValue(successor.Target.Value, out argumentList))
            {
                argumentList = new SortedList<int, Expression>();
                argumentLists.Add(successor.Target.Value, argumentList);
            }

            argumentList.Add(successor.Label.Index, expression);
        }

        #endregion

        #region Dependency Preprocessor

        class DependencyNode
        {
            public Node<ExpressionBuilder, ExpressionBuilderArgument> Publish;
            public List<Node<ExpressionBuilder, ExpressionBuilderArgument>> Subscribe = new List<Node<ExpressionBuilder, ExpressionBuilderArgument>>();
        }

        class DependencyLink
        {
            public string Name;
            public ExpressionBuilderGraph Workflow;
            public Node<ExpressionBuilder, ExpressionBuilderArgument> Publish;
            public Node<ExpressionBuilder, ExpressionBuilderArgument> Subscribe;

            public DependencyLink(
                string name,
                ExpressionBuilderGraph workflow,
                Node<ExpressionBuilder, ExpressionBuilderArgument> publish,
                Node<ExpressionBuilder, ExpressionBuilderArgument> subscribe)
            {
                Name = name;
                Workflow = workflow;
                Publish = publish;
                Subscribe = subscribe;
            }
        }

        static DependencyNode GetOrCreateDependency(ref Dictionary<string, DependencyNode> dependencies, string name)
        {
            if (dependencies == null)
            {
                dependencies = new Dictionary<string, DependencyNode>();
            }

            DependencyNode dependency;
            if (!dependencies.TryGetValue(name, out dependency))
            {
                dependency = new DependencyNode();
                dependencies.Add(name, dependency);
            }

            return dependency;
        }

        static IEnumerable<DependencyLink> FindBuildDependencies(ExpressionBuilderGraph source)
        {
            Dictionary<string, DependencyNode> dependencies = null;
            foreach (var node in source)
            {
                var workflowElement = ExpressionBuilder.Unwrap(node.Value);
                var subjectBuilder = workflowElement as SubjectBuilder;
                if (subjectBuilder != null && !string.IsNullOrEmpty(subjectBuilder.Name))
                {
                    // Connect to any existing subscribers
                    var dependency = GetOrCreateDependency(ref dependencies, subjectBuilder.Name);
                    if (dependency.Publish == null)
                    {
                        dependency.Publish = node;
                        foreach (var subscriber in dependency.Subscribe)
                        {
                            yield return new DependencyLink(subjectBuilder.Name, source, node, subscriber);
                        }
                    }
                }

                var subscribeSubject = workflowElement as SubscribeSubjectBuilder;
                if (subscribeSubject != null && !string.IsNullOrEmpty(subscribeSubject.Name))
                {
                    // Connect to publisher (if available)
                    var dependency = GetOrCreateDependency(ref dependencies, subscribeSubject.Name);
                    if (dependency.Publish != null)
                    {
                        yield return new DependencyLink(subscribeSubject.Name, source, dependency.Publish, node);
                    }
                    else dependency.Subscribe.Add(node);
                }

                var workflowBuilder = workflowElement as WorkflowExpressionBuilder;
                if (workflowBuilder != null)
                {
                    // Recurse through nested workflows and handle any unsatisfied dependencies
                    foreach (var link in FindBuildDependencies(workflowBuilder.Workflow))
                    {
                        if (link.Publish == null)
                        {
                            var dependency = GetOrCreateDependency(ref dependencies, link.Name);
                            if (dependency.Publish != null)
                            {
                                yield return new DependencyLink(link.Name, source, dependency.Publish, node);
                            }
                            else dependency.Subscribe.Add(node);
                        }
                        else yield return link;
                    }
                }
            }

            if (dependencies != null)
            {
                // Emit unsatisfied link dependencies
                foreach (var dependency in dependencies)
                {
                    if (dependency.Value.Publish != null) continue;
                    foreach (var subscriber in dependency.Value.Subscribe)
                    {
                        yield return new DependencyLink(dependency.Key, source, null, subscriber);
                    }
                }
            }
        }

        #endregion

        #region Build Sequence

        internal static Expression Build(this ExpressionBuilderGraph source, BuildContext buildContext)
        {
            Expression workflowOutput = null;
            HashSet<string> namedElements = null;
            var argumentLists = new Dictionary<ExpressionBuilder, SortedList<int, Expression>>();
            var dependencyLists = new Dictionary<ExpressionBuilder, SortedList<int, Expression>>();
            var multicastMap = new List<MulticastScope>();
            var connections = new List<Expression>();

            foreach (var node in source.TopologicalSort())
            {
                Expression expression;
                var builder = node.Value;
                var arguments = GetArgumentList(argumentLists, builder);

                var argumentRange = builder.ArgumentRange;
                if (argumentRange == null)
                {
                    throw new WorkflowBuildException("Argument range not set in expression builder node.", builder);
                }

                if (arguments.Count < argumentRange.LowerBound)
                {
                    throw new WorkflowBuildException(
                        string.Format("Unsupported number of arguments. This node requires at least {0} input connection(s).", argumentRange.LowerBound),
                        builder);
                }

                if (arguments.Count > argumentRange.UpperBound)
                {
                    throw new WorkflowBuildException(
                        string.Format("Unsupported number of arguments. This node supports at most {0} input connection(s).", argumentRange.LowerBound),
                        builder);
                }

                // Propagate build target in case of a nested workflow
                var workflowElement = ExpressionBuilder.Unwrap(builder);
                var requireBuildContext = workflowElement as IRequireBuildContext;
                if (requireBuildContext != null)
                {
                    requireBuildContext.BuildContext = buildContext;
                }

                var workflowProperty = workflowElement as ExternalizedProperty;
                if (workflowProperty != null && !string.IsNullOrEmpty(workflowProperty.Name))
                {
                    RegisterPropertyName(builder, workflowProperty, ref namedElements);
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
                    if (requireBuildContext != null)
                    {
                        requireBuildContext.BuildContext = null;
                    }
                }

                // Merge build dependencies
                var buildDependencies = GetArgumentList(dependencyLists, builder);
                if (buildDependencies.Count > 0)
                {
                    expression = ExpressionBuilder.MergeBuildDependencies(expression, buildDependencies);
                }

                // Check if build target was reached
                if (builder == buildContext.BuildTarget)
                {
                    buildContext.BuildResult = expression;
                }

                if (buildContext.BuildResult != null)
                {
                    return expression;
                }

                // Remove all closing scopes
                var successorCount = requireBuildContext != null
                    ? node.Successors.Count(edge => edge.Label != null)
                    : node.Successors.Count;
                multicastMap.RemoveAll(scope =>
                {
                    var referencesRemoved = scope.References.RemoveAll(reference => reference == builder);
                    if (scope.References.Count == 0)
                    {
                        expression = scope.Close(expression);
                        return true;
                    }

                    if (referencesRemoved > 0)
                    {
                        if (successorCount == 0) scope.References.Add(null);
                        else scope.References.AddRange(node.Successors.Select(successor => successor.Target.Value));
                    }
                    return false;
                });

                MulticastScope multicastScope = null;
                var argumentBuilder = workflowElement as IArgumentBuilder;
                var multicastBuilder = workflowElement as MulticastExpressionBuilder;
                if (successorCount > 1 || multicastBuilder != null)
                {
                    // Start a new multicast scope
                    if (multicastBuilder == null)
                    {
                        // Property mappings get replayed across subscriptions
                        if (argumentBuilder != null)
                        {
                            multicastBuilder = new ReplayLatestBuilder();
                        }
                        else multicastBuilder = new PublishBuilder();
                        expression = multicastBuilder.Build(expression);
                    }

                    multicastScope = new MulticastScope(multicastBuilder);
                    if (successorCount > 1)
                    {
                        multicastScope.References.AddRange(node.Successors.Select(successor => successor.Target.Value));
                        multicastMap.Insert(0, multicastScope);
                    }
                    else expression = multicastScope.Close(expression);
                }

                foreach (var successor in node.Successors)
                {
                    if (successor.Label == null) continue;
                    var argument = expression;
                    var buildDependency = false;
                    if (argumentBuilder != null)
                    {
                        try { buildDependency = !argumentBuilder.BuildArgument(argument, successor, out argument); }
                        catch (Exception e)
                        {
                            throw new WorkflowBuildException(e.Message, builder, e);
                        }
                    }

                    if (buildDependency)
                    {
                        UpdateArgumentList(dependencyLists, successor, argument);
                    }
                    else UpdateArgumentList(argumentLists, successor, argument);
                }

                if (successorCount == 0)
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
            return buildContext.CloseContext(output);
        }

        #endregion

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
            workflowExpression.Description = builder.Description;
            foreach (var mapping in builder.PropertyMappings)
            {
                workflowExpression.PropertyMappings.Add(mapping);
            }
            return workflowExpression;
        }

        #region Workflow Conversion

        /// <summary>
        /// Converts the specified expression builder workflow into an equivalent representation
        /// where each node has been replaced by its projection as specified by a selector function.
        /// </summary>
        /// <param name="source">The expression builder workflow to convert.</param>
        /// <param name="selector">A transform function to apply to each node.</param>
        /// <returns>
        /// A new expression builder workflow where all nodes have been replaced by their
        /// projections as specified by the selector function.
        /// </returns>
        public static ExpressionBuilderGraph Convert(
            this IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> source,
            Func<ExpressionBuilder, ExpressionBuilder> selector)
        {
            return Convert(source, selector, true);
        }

        /// <summary>
        /// Converts the specified expression builder workflow into an equivalent representation
        /// where each node has been replaced by its projection as specified by a selector function.
        /// </summary>
        /// <param name="source">The expression builder workflow to convert.</param>
        /// <param name="selector">A transform function to apply to each node.</param>
        /// <param name="recurse">
        /// A value indicating whether to recurse the conversion into nested workflows.
        /// </param>
        /// <returns>
        /// A new expression builder workflow where all nodes have been replaced by their
        /// projections as specified by the selector function.
        /// </returns>
        public static ExpressionBuilderGraph Convert(
            this IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> source,
            Func<ExpressionBuilder, ExpressionBuilder> selector,
            bool recurse)
        {
            var workflow = new ExpressionBuilderGraph();
            var nodeMapping = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderArgument>, Node<ExpressionBuilder, ExpressionBuilderArgument>>();
            foreach (var node in source)
            {
                var builder = node.Value;
                var workflowExpression = recurse ? ExpressionBuilder.Unwrap(builder) as WorkflowExpressionBuilder : null;
                if (workflowExpression != null)
                {
                    workflowExpression = workflowExpression.Clone(workflowExpression.Workflow.Convert(selector, recurse));
                    builder = UnwrapConvert(builder, x => workflowExpression);
                }

                builder = selector(builder);
                var builderNode = workflow.Add(builder);
                nodeMapping.Add(node, builderNode);
            }

            foreach (var node in source)
            {
                var builderNode = nodeMapping[node];
                foreach (var successor in node.Successors)
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

        // This method needs to be kept in sync with the behavior of Unwrap
        static ExpressionBuilder UnwrapConvert(ExpressionBuilder builder, Func<ExpressionBuilder, ExpressionBuilder> selector)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            var inspectBuilder = builder as InspectBuilder;
            if (inspectBuilder != null)
            {
                var result = UnwrapConvert(inspectBuilder.Builder, selector);
                return new InspectBuilder(result);
            }

            return selector(builder);
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
            return Convert(source, builder => new InspectBuilder(builder), recurse);
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
            return Convert(source, builder => ((InspectBuilder)builder).Builder, recurse);
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

        #endregion
    }
}
