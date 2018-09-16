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
        /// Determines whether the specified <see cref="ExpressionBuilder"/> represents a build dependency.
        /// </summary>
        /// <param name="builder">The <see cref="ExpressionBuilder"/> to test.</param>
        /// <returns>
        /// true if the specified <see cref="ExpressionBuilder"/> represents a
        /// build dependency; otherwise, false.
        /// </returns>
        public static bool IsBuildDependency(this ExpressionBuilder builder)
        {
            var element = ExpressionBuilder.GetWorkflowElement(builder);
            return !(element is InputMappingBuilder) && element is PropertyMappingBuilder ||
                   element is ExternalizedProperty;
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
            var propertyDescriptor = TypeDescriptor.GetProperties(source).Find(name, false);
            if (propertyDescriptor == null)
            {
                throw new KeyNotFoundException(string.Format(Resources.Exception_PropertyNotFound, name));
            }

            if (value != null && value.GetType() != propertyDescriptor.PropertyType)
            {
                value = propertyDescriptor.Converter.ConvertFrom(value);
            }

            propertyDescriptor.SetValue(source, value);
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
                                    where inspectBuilder != null && inspectBuilder.ObservableType != null
                                    select inspectBuilder)
            {
                var inspectBuilder = builder;
                yield return inspectBuilder.ErrorEx.Select(xs => BuildRuntimeExceptionStack(xs.Message, inspectBuilder, xs, callStack));

                var workflowExpression = inspectBuilder.Builder as IWorkflowExpressionBuilder;
                if (workflowExpression != null && workflowExpression.Workflow != null)
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
            var dependencies = (from link in FindBuildDependencies(source, buildContext)
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
            public DependencyElement Publish;
            public List<DependencyElement> Subscribe = new List<DependencyElement>();
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

        class DependencyElement
        {
            public ExpressionBuilder Element;
            public Node<ExpressionBuilder, ExpressionBuilderArgument> Node;
            public DependencyElement InnerDependency;

            public DependencyElement(ExpressionBuilder element, Node<ExpressionBuilder, ExpressionBuilderArgument> node)
                : this(element, node, null)
            {
            }

            public DependencyElement(ExpressionBuilder element, Node<ExpressionBuilder, ExpressionBuilderArgument> node, DependencyElement innerDependency)
            {
                Element = element;
                Node = node;
                InnerDependency = innerDependency;
            }
        }

        static IEnumerable<DependencyElement> SelectDependencyElements(ExpressionBuilderGraph source, IBuildContext buildContext)
        {
            foreach (var node in source)
            {
                var builder = node.Value;
                var element = ExpressionBuilder.Unwrap(builder);
                var groupBuilder = element as IGroupWorkflowBuilder;
                if (groupBuilder != null)
                {
                    try { groupBuilder.BuildContext = buildContext; }
                    catch (Exception e)
                    {
                        throw new WorkflowBuildException(e.Message, builder, e);
                    }

                    try
                    {
                        var workflow = groupBuilder.Workflow;
                        if (workflow == null) continue;

                        var includeBuilder = groupBuilder as IncludeWorkflowBuilder;
                        var dependencyContext = includeBuilder != null ? new IncludeContext(buildContext, includeBuilder.Path) : buildContext;

                        var enumerator = SelectDependencyElements(workflow, dependencyContext).GetEnumerator();
                        while (true)
                        {
                            try { if (!enumerator.MoveNext()) break; }
                            catch (Exception e)
                            {
                                throw new WorkflowBuildException(e.Message, builder, e);
                            }

                            var dependencyElement = enumerator.Current;
                            yield return new DependencyElement(dependencyElement.Element, node, dependencyElement);
                        }
                    }
                    finally { groupBuilder.BuildContext = null; }
                }
                else yield return new DependencyElement(element, node);
            }
        }

        static DependencyLink CreateDependencyLink(string name, ExpressionBuilderGraph workflow, DependencyElement publish, DependencyElement subscribe)
        {
            var publishNode = publish != null ? publish.Node : null;
            var subscribeNode = subscribe != null ? subscribe.Node : null;
            if (publishNode == subscribeNode && publishNode != null)
            {
                var group = (IGroupWorkflowBuilder)ExpressionBuilder.Unwrap(publishNode.Value);
                return CreateDependencyLink(name, group.Workflow, publish.InnerDependency, subscribe.InnerDependency);
            }
            else return new DependencyLink(name, workflow, publishNode, subscribeNode);
        }

        static IEnumerable<DependencyLink> FindBuildDependencies(ExpressionBuilderGraph source, IBuildContext buildContext)
        {
            Dictionary<string, DependencyNode> dependencies = null;
            foreach (var dependencyElement in SelectDependencyElements(source, buildContext))
            {
                var node = dependencyElement.Node;
                var workflowElement = dependencyElement.Element;
                var subjectBuilder = workflowElement as SubjectBuilder;
                if (subjectBuilder != null && !string.IsNullOrEmpty(subjectBuilder.Name))
                {
                    // Connect to any existing subscribers
                    var dependency = GetOrCreateDependency(ref dependencies, subjectBuilder.Name);
                    if (dependency.Publish == null)
                    {
                        dependency.Publish = dependencyElement;
                        foreach (var subscriber in dependency.Subscribe)
                        {
                            yield return CreateDependencyLink(subjectBuilder.Name, source, dependencyElement, subscriber);
                        }
                    }
                }

                var requireSubject = workflowElement as IRequireSubject;
                if (requireSubject != null && !string.IsNullOrEmpty(requireSubject.Name))
                {
                    // Connect to publisher (if available)
                    var dependency = GetOrCreateDependency(ref dependencies, requireSubject.Name);
                    if (dependency.Publish != null)
                    {
                        yield return CreateDependencyLink(requireSubject.Name, source, dependency.Publish, dependencyElement);
                    }
                    else dependency.Subscribe.Add(dependencyElement);
                }

                var workflowBuilder = workflowElement as WorkflowExpressionBuilder;
                if (workflowBuilder != null)
                {
                    // Recurse through nested workflows and handle any unsatisfied dependencies
                    var enumerator = FindBuildDependencies(workflowBuilder.Workflow, buildContext).GetEnumerator();
                    while (true)
                    {
                        try { if (!enumerator.MoveNext()) break; }
                        catch (Exception e)
                        {
                            throw new WorkflowBuildException(e.Message, node.Value, e);
                        }

                        var link = enumerator.Current;
                        if (link.Publish == null)
                        {
                            var dependency = GetOrCreateDependency(ref dependencies, link.Name);
                            if (dependency.Publish != null)
                            {
                                yield return CreateDependencyLink(link.Name, source, dependency.Publish, dependencyElement);
                            }
                            else dependency.Subscribe.Add(dependencyElement);
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
                        yield return CreateDependencyLink(dependency.Key, source, null, subscriber);
                    }
                }
            }
        }

        #endregion

        #region Nested Workflows

        internal static IEnumerable<WorkflowInputBuilder> GetNestedParameters(this ExpressionBuilderGraph source)
        {
            return from node in source
                   let inputBuilder = ExpressionBuilder.Unwrap(node.Value) as WorkflowInputBuilder
                   where inputBuilder != null
                   orderby inputBuilder.Index ascending
                   select inputBuilder;
        }

        internal static IEnumerable<ExternalizedProperty> GetExternalizedProperties(this ExpressionBuilderGraph source)
        {
            return from node in source
                   let externalizedProperty = ExpressionBuilder.Unwrap(node.Value) as ExternalizedProperty
                   where externalizedProperty != null
                   select externalizedProperty;
        }

        internal static Expression BuildNested(this ExpressionBuilderGraph source, IEnumerable<Expression> arguments, IBuildContext buildContext)
        {
            var parameters = source.GetNestedParameters();
            foreach (var assignment in parameters.Zip(arguments, (parameter, argument) => new { parameter, argument }))
            {
                assignment.parameter.Source = assignment.argument;
            }

            return source.Build(buildContext);
        }

        #endregion

        #region Build Sequence

        internal static Expression Build(this ExpressionBuilderGraph source, IBuildContext buildContext)
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

                // Propagate build target in case of a nested workflow
                var workflowElement = ExpressionBuilder.Unwrap(builder);
                var requireBuildContext = workflowElement as IRequireBuildContext;
                if (requireBuildContext != null)
                {
                    try { requireBuildContext.BuildContext = buildContext; }
                    catch (Exception e)
                    {
                        throw new WorkflowBuildException(e.Message, builder, e);
                    }
                }

                var argumentRange = builder.ArgumentRange;
                if (argumentRange == null)
                {
                    throw new WorkflowBuildException("Argument range not set in expression builder node.", builder);
                }

                if (arguments.Count < argumentRange.LowerBound)
                {
                    throw new WorkflowBuildException(string.Format(Resources.Exception_UnsupportedMinArgumentCount, argumentRange.LowerBound), builder);
                }

                if (arguments.Count > argumentRange.UpperBound)
                {
                    throw new WorkflowBuildException(string.Format(Resources.Exception_UnsupportedMaxArgumentCount, argumentRange.LowerBound), builder);
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
                if (buildDependencies.Count > 0 && expression.NodeType != ExpressionType.Extension)
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

                // Do not generate output sequences if the result expression is empty
                if (expression.NodeType == ExpressionType.Extension)
                {
                    // Validate externalized properties
                    var externalizedProperty = workflowElement as ExternalizedProperty;
                    if (externalizedProperty != null)
                    {
                        var argument = expression;
                        foreach (var successor in node.Successors)
                        {
                            try { externalizedProperty.BuildArgument(argument, successor, out argument, string.Empty); }
                            catch (Exception e)
                            {
                                throw new WorkflowBuildException(e.Message, builder, e);
                            }
                        }
                        continue;
                    }
                }

                var outputBuilder = workflowElement as WorkflowOutputBuilder;
                if (outputBuilder != null)
                {
                    if (successorCount > 0)
                    {
                        throw new WorkflowBuildException("The workflow output must be a terminal node.", builder);
                    }

                    if (workflowOutput != null)
                    {
                        throw new WorkflowBuildException("Workflows cannot have more than one output.", builder);
                    }

                    workflowOutput = expression;
                    continue;
                }

                var argumentBuilder = workflowElement as IArgumentBuilder;
                if (successorCount > 1 && expression.NodeType != ExpressionType.Extension)
                {
                    // Start a new multicast scope
                    MulticastBranchBuilder multicastBuilder;
                    if (argumentBuilder != null)
                    {
                        // Property mappings get replayed across subscriptions
                        multicastBuilder = new ReplayLatestBranchBuilder();
                    }
                    else multicastBuilder = new PublishBranchBuilder();
                    expression = multicastBuilder.Build(expression);

                    var multicastScope = new MulticastScope(multicastBuilder);
                    multicastScope.References.AddRange(node.Successors.Select(successor => successor.Target.Value));
                    multicastMap.Insert(0, multicastScope);
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
            }

            var output = ExpressionBuilder.BuildOutput(workflowOutput, connections);
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
                if (recurse)
                {
                    builder = UnwrapConvert(builder, selector);
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
                        var edge = new ExpressionBuilderArgument(successor.Label.Index);
                        workflow.AddEdge(builderNode, targetNode, edge);
                    }
                }
            }

            return workflow;
        }

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

            var disableBuilder = builder as DisableBuilder;
            if (disableBuilder != null)
            {
                var result = UnwrapConvert(disableBuilder.Builder, selector);
                return new DisableBuilder(result);
            }

            var workflowExpression = builder as WorkflowExpressionBuilder;
            if (workflowExpression != null)
            {
                return workflowExpression.Clone(workflowExpression.Workflow.Convert(selector, true));
            }

            return builder;
        }

        /// <summary>
        /// Decorates the specified expression builder with an <see cref="InspectBuilder"/>
        /// instance allowing for runtime inspection and error redirection.
        /// </summary>
        /// <param name="builder">The expression builder instance to decorate.</param>
        /// <returns>
        /// An <see cref="InspectBuilder"/> instance decorating the
        /// specified expression builder.
        /// </returns>
        public static InspectBuilder AsInspectBuilder(this ExpressionBuilder builder)
        {
            return AsInspectBuilder(builder, true);
        }

        static InspectBuilder AsInspectBuilder(this ExpressionBuilder builder, bool recurse)
        {
            var includeWorkflow = builder as IncludeWorkflowBuilder;
            if (includeWorkflow != null && recurse)
            {
                builder = new IncludeWorkflowBuilder(includeWorkflow, true);
            }
            return new InspectBuilder(builder);
        }

        static ExpressionBuilder RemoveInspectBuilder(InspectBuilder inspectBuilder, bool recurse)
        {
            var builder = inspectBuilder.Builder;
            var includeWorkflow = builder as IncludeWorkflowBuilder;
            if (includeWorkflow != null && recurse)
            {
                builder = new IncludeWorkflowBuilder(includeWorkflow, false);
            }
            return builder;
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
            return Convert(source, builder => AsInspectBuilder(builder, recurse), recurse);
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
            return Convert(source, builder => RemoveInspectBuilder((InspectBuilder)builder, recurse), recurse);
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
