using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Dag;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.ComponentModel;
using Bonsai.Properties;
using System.Globalization;

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
            return ExpressionBuilder.IsBuildDependency(element as IArgumentBuilder);
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
                throw new ArgumentException("The workflow property name cannot be null or whitespace.", nameof(name));
            }

            object component = source;
            var memberChain = name.Split(new[] { ExpressionHelper.MemberSeparator }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < memberChain.Length - 1; i++)
            {
                var namedBuilders = (from node in source
                                     let builder = ExpressionBuilder.Unwrap(node.Value) as ISupportPropertyAssignment
                                     where builder != null && builder.Name == memberChain[i]
                                     select builder).ToArray();
                if (namedBuilders.Length == 0)
                {
                    throw new KeyNotFoundException(string.Format(
                        Resources.Exception_PropertyNotFound,
                        string.Join(ExpressionHelper.MemberSeparator, memberChain, 0, i + 1)));
                }
                else if (namedBuilders.Length > 1)
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.Exception_AmbiguousNamedElement,
                        string.Join(ExpressionHelper.MemberSeparator, memberChain, 0, i + 1)));
                }

                component = namedBuilders[0];
                if (component is IWorkflowExpressionBuilder workflowBuilder)
                {
                    source = workflowBuilder.Workflow;
                }
                else if (i < memberChain.Length - 2)
                {
                    throw new InvalidOperationException(string.Format(
                        Resources.Exception_UnsupportedNestedAssignment,
                        string.Join(ExpressionHelper.MemberSeparator, memberChain, 0, i + 1)));
                }
            }

            name = memberChain[memberChain.Length - 1];
            var propertyDescriptor = TypeDescriptor.GetProperties(component).Find(name, false);
            if (propertyDescriptor == null)
            {
                throw new KeyNotFoundException(string.Format(Resources.Exception_PropertyNotFound, name));
            }

            if (value is not null && value.GetType() != propertyDescriptor.PropertyType)
            {
                try
                {
                    var context = new SimpleTypeDescriptorContext(component, propertyDescriptor);
                    value = propertyDescriptor.Converter.ConvertFrom(context, CultureInfo.InvariantCulture, value);
                }
                catch (Exception ex)
                {
                    throw new FormatException(Resources.Exception_InvalidPropertyValue, ex);
                }
            }

            propertyDescriptor.SetValue(component, value);
        }

        class SimpleTypeDescriptorContext : ITypeDescriptorContext
        {
            public SimpleTypeDescriptorContext(object instance, PropertyDescriptor descriptor)
            {
                Instance = instance;
                PropertyDescriptor = descriptor;
            }

            public IContainer Container => null;

            public object Instance { get; }

            public PropertyDescriptor PropertyDescriptor { get; }

            public void OnComponentChanged() { }

            public bool OnComponentChanging() => false;

            public object GetService(Type serviceType) => null;
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

                if (inspectBuilder.Builder is IWorkflowExpressionBuilder workflowExpression && workflowExpression.Workflow != null)
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
            return Build(source, buildTarget, Enumerable.Empty<Expression>());
        }

        /// <summary>
        /// Generates an expression tree from the specified expression builder workflow and array
        /// of build arguments.
        /// </summary>
        /// <param name="source">
        /// The expression builder workflow for which to generate the expression tree.
        /// </param>
        /// <param name="buildArguments">
        /// The array of <see cref="Expression"/> objects to be assigned as workflow input arguments
        /// in the context of generating the expression tree.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> tree representing the evaluation of the full
        /// expression builder workflow.
        /// </returns>
        public static Expression Build(this ExpressionBuilderGraph source, params Expression[] buildArguments)
        {
            return Build(source, null, (IEnumerable<Expression>)buildArguments);
        }

        /// <summary>
        /// Generates an expression tree from the specified expression builder workflow and a sequence
        /// of build arguments.
        /// </summary>
        /// <param name="source">
        /// The expression builder workflow for which to generate the expression tree.
        /// </param>
        /// <param name="buildArguments">
        /// The sequence of <see cref="Expression"/> objects to be assigned as workflow input arguments
        /// in the context of generating the expression tree.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> tree representing the evaluation of the full
        /// expression builder workflow.
        /// </returns>
        public static Expression Build(this ExpressionBuilderGraph source, IEnumerable<Expression> buildArguments)
        {
            return Build(source, null, buildArguments);
        }

        /// <summary>
        /// Generates an expression tree from the specified expression builder workflow and array of
        /// build arguments, evaluated up to the specified build target.
        /// </summary>
        /// <param name="source">
        /// The expression builder workflow for which to generate the expression tree.
        /// </param>
        /// <param name="buildTarget">
        /// The expression builder node up to which the workflow will be evaluated.
        /// </param>
        /// <param name="buildArguments">
        /// The array of <see cref="Expression"/> objects to be assigned as workflow input arguments
        /// in the context of generating the expression tree.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> tree representing the evaluation of the expression
        /// builder workflow up to the specified <paramref name="buildTarget"/>.
        /// </returns>
        public static Expression Build(this ExpressionBuilderGraph source, ExpressionBuilder buildTarget, params Expression[] buildArguments)
        {
            return Build(source, buildTarget, (IEnumerable<Expression>)buildArguments);
        }

        /// <summary>
        /// Generates an expression tree from the specified expression builder workflow and a sequence of
        /// build arguments, evaluated up to the specified build target.
        /// </summary>
        /// <param name="source">
        /// The expression builder workflow for which to generate the expression tree.
        /// </param>
        /// <param name="buildTarget">
        /// The expression builder node up to which the workflow will be evaluated.
        /// </param>
        /// <param name="buildArguments">
        /// The sequence of <see cref="Expression"/> objects to be assigned as workflow input arguments
        /// in the context of generating the expression tree.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> tree representing the evaluation of the expression
        /// builder workflow up to the specified <paramref name="buildTarget"/>.
        /// </returns>
        public static Expression Build(this ExpressionBuilderGraph source, ExpressionBuilder buildTarget, IEnumerable<Expression> buildArguments)
        {
            // Add/remove build dependencies
            var buildContext = new BuildContext(buildTarget);
            using (var dependencies = AddBuildDependencies(source, buildContext))
            {
                return BuildNested(source, buildArguments, buildContext);
            }
        }

        #region Argument Lists

        class ArgumentList : IEnumerable<Expression>
        {
            readonly SortedList<int, ExpressionArgument> arguments = new SortedList<int, ExpressionArgument>();
            internal static readonly ArgumentList Empty = new ArgumentList();

            public int Count { get; private set; }

            public void Add(int key, ExpressionArgument argument)
            {
                arguments.Add(key, argument);
                if (argument.NestedArguments != null) Count += argument.NestedArguments.Length;
                else Count++;
            }

            public IEnumerator<Expression> GetEnumerator()
            {
                foreach (var argument in arguments.Values)
                {
                    if (argument.NestedArguments != null)
                    {
                        foreach (var nestedArgument in argument.NestedArguments)
                        {
                            yield return nestedArgument;
                        }
                    }
                    else yield return argument.Expression;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        struct ExpressionArgument
        {
            public Expression Expression;
            public Expression[] NestedArguments;
        }

        static ArgumentList GetArgumentList(
            Dictionary<ExpressionBuilder, ArgumentList> argumentLists,
            ExpressionBuilder builder)
        {
            ArgumentList argumentList;
            if (argumentLists.TryGetValue(builder, out argumentList))
            {
                argumentLists.Remove(builder);
                return argumentList;
            }

            return ArgumentList.Empty;
        }

        static void UpdateArgumentList(
            Dictionary<ExpressionBuilder, ArgumentList> argumentLists,
            Edge<ExpressionBuilder, ExpressionBuilderArgument> successor,
            Expression expression)
        {
            ExpressionArgument argument;
            argument.Expression = expression;
            var disable = expression as DisableExpression;
            argument.NestedArguments = disable?.Arguments;
            UpdateArgumentList(argumentLists, successor, argument);
        }

        static void UpdateArgumentList(
            Dictionary<ExpressionBuilder, ArgumentList> argumentLists,
            Edge<ExpressionBuilder, ExpressionBuilderArgument> successor,
            ExpressionArgument argument)
        {
            if (!argumentLists.TryGetValue(successor.Target.Value, out ArgumentList argumentList))
            {
                argumentList = new ArgumentList();
                argumentLists.Add(successor.Target.Value, argumentList);
            }

            try { argumentList.Add(successor.Label.Index, argument); }
            catch (ArgumentException e)
            {
                throw new WorkflowBuildException(e.Message, successor.Target.Value, e);
            }
        }

        static void RegisterPropertyName(ExpressionBuilder builder, ExternalizedMapping mapping, ref HashSet<string> externalizedProperties)
        {
            if (externalizedProperties == null) externalizedProperties = new HashSet<string>();
            var propertyName = mapping.ExternalizedName;
            if (!externalizedProperties.Add(propertyName))
            {
                throw new WorkflowBuildException(string.Format("A workflow property with the name '{0}' already exists.", propertyName), builder);
            }
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

            if (!dependencies.TryGetValue(name, out DependencyNode dependency))
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
                if (element is IGroupWorkflowBuilder groupBuilder)
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

                        var dependencyContext = groupBuilder is IncludeWorkflowBuilder includeBuilder ? new IncludeContext(buildContext, includeBuilder.Path) : buildContext;
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
            var publishNode = publish?.Node;
            var subscribeNode = subscribe?.Node;
            if (publishNode == subscribeNode && publishNode != null)
            {
                var group = (IGroupWorkflowBuilder)ExpressionBuilder.Unwrap(publishNode.Value);
                return CreateDependencyLink(name, group.Workflow, publish.InnerDependency, subscribe.InnerDependency);
            }
            else return new DependencyLink(name, workflow, publishNode, subscribeNode);
        }

        static WorkflowBuildException BuildDependencyLinkExceptionStack(Exception innerException, DependencyElement dependencyElement)
        {
            if (dependencyElement.InnerDependency != null)
            {
                innerException = BuildDependencyLinkExceptionStack(innerException, dependencyElement.InnerDependency);
            }

            return new WorkflowBuildException(innerException.Message, dependencyElement.Node.Value, innerException);
        }

        static IEnumerable<DependencyLink> FindBuildDependencies(ExpressionBuilderGraph source, IBuildContext buildContext)
        {
            Dictionary<string, DependencyNode> dependencies = null;
            foreach (var dependencyElement in SelectDependencyElements(source, buildContext))
            {
                var node = dependencyElement.Node;
                var workflowElement = dependencyElement.Element;
                if (workflowElement is SubjectExpressionBuilder subjectBuilder && !string.IsNullOrEmpty(subjectBuilder.Name))
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

                if (workflowElement is IRequireSubject requireSubject && !string.IsNullOrEmpty(requireSubject.Name))
                {
                    // Connect to publisher (if available)
                    var dependency = GetOrCreateDependency(ref dependencies, requireSubject.Name);
                    if (dependency.Publish != null)
                    {
                        yield return CreateDependencyLink(requireSubject.Name, source, dependency.Publish, dependencyElement);
                    }
                    else dependency.Subscribe.Add(dependencyElement);
                }

                if (workflowElement is WorkflowExpressionBuilder workflowBuilder)
                {
                    // Recurse through nested workflows and handle any unsatisfied dependencies
                    var enumerator = FindBuildDependencies(workflowBuilder.Workflow, buildContext).GetEnumerator();
                    while (true)
                    {
                        try { if (!enumerator.MoveNext()) break; }
                        catch (Exception e)
                        {
                            throw BuildDependencyLinkExceptionStack(e, dependencyElement);
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

        class BuildDependency
        {
            public readonly ExpressionBuilderGraph Workflow;
            public readonly Node<ExpressionBuilder, ExpressionBuilderArgument> Source;
            public readonly Edge<ExpressionBuilder, ExpressionBuilderArgument> Edge;

            public BuildDependency(DependencyLink link)
            {
                Workflow = link.Workflow;
                Source = link.Publish;
                Edge = Workflow.AddEdge(link.Publish, link.Subscribe, null);
            }
        }

        static IDisposable AddBuildDependencies(ExpressionBuilderGraph source, IBuildContext buildContext)
        {
            var dependencies = new List<BuildDependency>();
            var disposable = Disposable.Create(() =>
            {
                foreach (var dependency in dependencies)
                {
                    dependency.Workflow.RemoveEdge(dependency.Source, dependency.Edge);
                }
            });

            try
            {
                foreach (var link in FindBuildDependencies(source, buildContext).Where(link => link.Publish != null && link.Subscribe != null))
                {
                    var dependency = new BuildDependency(link);
                    dependencies.Add(dependency);
                }
                return disposable;
            }
            catch
            {
                disposable.Dispose();
                throw;
            }
        }

        #endregion

        #region Cycle Detection

        static DependencyElement FindCyclicalDependency(ExpressionBuilderGraph source, IBuildContext buildContext)
        {
            return (from dependency in SelectDependencyElements(source, buildContext)
                    where dependency.Element is SubscribeSubject
                    from successor in dependency.Node.Successors
                    where successor.Target.DepthFirstSearch().Contains(dependency.Node)
                    select dependency)
                    .FirstOrDefault();
        }

        static Exception CreateDependencyException(string message, DependencyElement element)
        {
            var stack = new Stack<DependencyElement>();
            while (element != null)
            {
                stack.Push(element);
                element = element.InnerDependency;
            }

            var exception = default(Exception);
            foreach (var dependency in stack)
            {
                exception = new WorkflowBuildException(message, dependency.Node.Value, exception);
            }

            return exception;
        }

        #endregion

        #region Branch Pruning

        static HashSet<Node<ExpressionBuilder, ExpressionBuilderArgument>> GetPrunedBranches(
            IEnumerable<DirectedGraph<ExpressionBuilder, ExpressionBuilderArgument>> buildOrder)
        {
            HashSet<Node<ExpressionBuilder, ExpressionBuilderArgument>> result = new();
            foreach (var component in buildOrder)
            {
                for (int i = component.Count - 1; i >= 0; i--)
                {
                    var node = component[i];
                    if (ExpressionBuilder.Unwrap(node.Value) is not DisableBuilder)
                        continue;

                    if (AllSuccessorsPruned(result, node.Successors))
                        result.Add(node);
                }
            }

            return result;
        }

        static bool AllSuccessorsPruned(
            HashSet<Node<ExpressionBuilder, ExpressionBuilderArgument>> result,
            EdgeCollection<ExpressionBuilder, ExpressionBuilderArgument> successors)
        {
            for (int k = successors.Count - 1; k >= 0; k--)
            {
                var successor = successors[k];
                if (successor.Label != null && !result.Contains(successor.Target))
                    return false;
            }

            return true;
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

        internal static Expression BuildNested(this ExpressionBuilderGraph source, IEnumerable<Expression> arguments, IBuildContext buildContext)
        {
            var parameters = source.GetNestedParameters();
            foreach (var assignment in parameters.Zip(arguments, (parameter, argument) => new { parameter, argument }))
            {
                assignment.parameter.Source = assignment.argument;
            }

            try { return source.Build(buildContext); }
            finally
            {
                foreach (var node in source)
                {
                    if (node.Value is InspectBuilder builder)
                    {
                        builder.ResetVisualizerMappings();
                    }
                }

                foreach (var parameter in parameters)
                {
                    parameter.Source = null;
                }
            }
        }

        #endregion

        #region Build Sequence

        internal static Expression Build(this ExpressionBuilderGraph source, IBuildContext buildContext)
        {
            Expression output = null;
            HashSet<string> externalizedProperties = null;
            var argumentLists = new Dictionary<ExpressionBuilder, ArgumentList>();
            var dependencyLists = new Dictionary<ExpressionBuilder, ArgumentList>();
            var nodeSuccessors = new List<Edge<ExpressionBuilder, ExpressionBuilderArgument>>();
            var componentConnections = new List<Expression>();
            var multicastMap = new List<MulticastScope>();
            var connections = new List<Expression>();

            if (!TopologicalSort.TrySort(source, out IEnumerable<DirectedGraph<ExpressionBuilder, ExpressionBuilderArgument>> buildOrder))
            {
                var cyclicalDependency = FindCyclicalDependency(source, buildContext);
                if (cyclicalDependency == null) throw new WorkflowBuildException("The workflow contains unspecified cyclical build dependencies.");
                var name = ((SubscribeSubject)cyclicalDependency.Element).Name;
                var message = string.Format("The specified variable '{0}' is defined in terms of itself.", name);
                throw CreateDependencyException(message, cyclicalDependency);
            }

            var prunedBranches = GetPrunedBranches(buildOrder);
            foreach (var component in buildOrder)
            {
                Expression workflowOutput = null;
                foreach (var node in component)
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
                        throw new WorkflowBuildException(string.Format(Resources.Exception_UnsupportedMaxArgumentCount, argumentRange.UpperBound), builder);
                    }

                    var externalizedBuilder = workflowElement as IExternalizedMappingBuilder;
                    if (externalizedBuilder != null)
                    {
                        foreach (var property in externalizedBuilder.GetExternalizedProperties())
                        {
                            RegisterPropertyName(builder, property, ref externalizedProperties);
                        }
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
                    var reducible = ExpressionBuilder.IsReducible(expression);
                    var buildDependencies = GetArgumentList(dependencyLists, builder);
                    if (buildDependencies.Count > 0 && reducible)
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

                    // Filter disabled successors for property mapping nodes
                    var argumentBuilder = workflowElement as IArgumentBuilder;
                    var propertyMappingBuilder = ExpressionBuilder.IsBuildDependency(argumentBuilder);

                    nodeSuccessors.Clear();
                    if (propertyMappingBuilder)
                    {
                        nodeSuccessors.AddRange(node.Successors
                            .Where(edge => edge.Label != null && ExpressionBuilder.Unwrap(edge.Target.Value) is not DisableBuilder));
                    }
                    else
                    {
                        nodeSuccessors.AddRange(node.Successors
                            .Where(edge => edge.Label != null && !prunedBranches.Contains(edge.Target)));
                    }

                    // Remove all closing scopes
                    var disable = expression as DisableExpression;
                    var successorCount = nodeSuccessors.Count;
                    multicastMap.RemoveAll(scope =>
                    {
                        var referencesRemoved = scope.References.RemoveAll(reference => reference == builder);
                        if (scope.References.Count == 0 && disable == null)
                        {
                            try
                            {
                                expression = scope.Close(expression);
                                return true;
                            }
                            catch (Exception e)
                            {
                                throw new WorkflowBuildException(e.Message, builder, e);
                            }
                        }

                        if (referencesRemoved > 0)
                        {
                            var expandedArguments = disable?.Arguments.Skip(1).GetEnumerator();
                            do
                            {
                                // If there are no successors, or the expression is a disabled build dependency, this scope should never close
                                if (successorCount == 0 || expression == DisconnectExpression.Instance)
                                    scope.References.Add(null);
                                else
                                    scope.References.AddRange(nodeSuccessors
                                        .Select(successor => successor.Target.Value));
                            }
                            while (expandedArguments != null && expandedArguments.MoveNext());
                        }
                        return false;
                    });

                    // Evaluate irreducible extension expressions
                    if (!reducible)
                    {
                        // Disconnect disabled build dependencies from their immediate successors
                        if (expression == DisconnectExpression.Instance)
                        {
                            componentConnections.AddRange(arguments);
                            continue;
                        }

                        // Validate externalized properties
                        if (externalizedBuilder != null)
                        {
                            var argument = expression;
                            foreach (var successor in nodeSuccessors)
                            {
                                var successorElement = ExpressionBuilder.GetWorkflowElement(successor.Target.Value);
                                var successorInstance = Expression.Constant(successorElement);
                                foreach (var property in externalizedBuilder.GetExternalizedProperties())
                                {
                                    try { argument = ExpressionBuilder.BuildPropertyMapping(argument, successorInstance, property.Name); }
                                    catch (Exception e)
                                    {
                                        throw new WorkflowBuildException(e.Message, builder, e);
                                    }
                                }
                            }
                            continue;
                        }

                        // Do not generate output sequences if the result expression is empty
                        if (expression == EmptyExpression.Instance)
                        {
                            continue;
                        }

                        // Do not generate output or successor sequences if the result expression type is void
                        if (successorCount > 0 && disable == null)
                        {
                            try { if (expression.Type == typeof(void)) continue; }
                            catch (Exception e)
                            {
                                throw new WorkflowBuildException(e.Message, builder, e);
                            }
                        }
                    }

                    if (workflowElement is WorkflowOutputBuilder outputBuilder)
                    {
                        if (successorCount > 0)
                        {
                            throw new WorkflowBuildException("The workflow output must be a terminal node.", builder);
                        }

                        if (workflowOutput != null || output != null)
                        {
                            throw new WorkflowBuildException("Workflows cannot have more than one output.", builder);
                        }

                        workflowOutput = expression;
                        continue;
                    }

                    if (successorCount > 1 && reducible)
                    {
                        // Start a new multicast scope
                        MulticastBranchBuilder multicastBuilder;
                        if (propertyMappingBuilder)
                        {
                            // Property mappings get replayed across subscriptions
                            multicastBuilder = new ReplayLatestBranchBuilder();
                        }
                        else multicastBuilder = new PublishBranchBuilder();
                        expression = multicastBuilder.Build(expression);

                        // Ensure publish/subscribe subject dependencies are not multicast
                        var multicastScope = new MulticastScope(multicastBuilder);
                        multicastScope.References.AddRange(nodeSuccessors
                            .Select(successor => successor.Target.Value));
                        multicastMap.Insert(0, multicastScope);
                    }

                    foreach (var successor in nodeSuccessors)
                    {
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
                        if (disable != null) componentConnections.AddRange(disable.Arguments);
                        else componentConnections.Add(expression);
                    }
                }

                var componentOutput = ExpressionBuilder.BuildOutput(workflowOutput, componentConnections);
                multicastMap.RemoveAll(scope =>
                {
                    componentOutput = scope.Close(componentOutput);
                    return true;
                });

                if (workflowOutput != null) output = componentOutput;
                else connections.Add(componentOutput);
                multicastMap.Clear();
                componentConnections.Clear();
            }

            output = ExpressionBuilder.MergeOutput(output, connections);
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

        /// <summary>
        /// Builds and compiles an expression builder workflow into an observable sequence
        /// with the specified element type.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the observable sequence.</typeparam>
        /// <param name="source">The expression builder workflow to compile.</param>
        /// <returns>
        /// An observable sequence with the specified element type.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// The specified expression builder workflow does not compile into an observable sequence
        /// with the expected element type.
        /// </exception>
        public static IObservable<TResult> BuildObservable<TResult>(this ExpressionBuilderGraph source)
        {
            var workflow = source.Build();
            if (!typeof(IObservable<TResult>).IsAssignableFrom(workflow.Type))
            {
                workflow = ExpressionBuilder.ConvertExpression(workflow, typeof(IObservable<TResult>));
            }

            var observableFactory = Expression.Lambda<Func<IObservable<TResult>>>(workflow).Compile();
            return observableFactory();
        }

        static WorkflowExpressionBuilder Clone(this WorkflowExpressionBuilder builder, ExpressionBuilderGraph workflow)
        {
            var workflowExpression = (WorkflowExpressionBuilder)Activator.CreateInstance(builder.GetType(), workflow);
            workflowExpression.Name = builder.Name;
            workflowExpression.Description = builder.Description;
            workflowExpression.InstanceNumber = builder.InstanceNumber;
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
                var instanceNumber = builder.InstanceNumber;
                if (recurse)
                {
                    builder = UnwrapConvert(builder, selector);
                }

                builder = selector(builder);
                builder.InstanceNumber = instanceNumber;
                var builderNode = workflow.Add(builder);
                nodeMapping.Add(node, builderNode);
            }

            foreach (var node in source)
            {
                var builderNode = nodeMapping[node];
                foreach (var successor in node.Successors)
                {
                    if (nodeMapping.TryGetValue(successor.Target, out Node<ExpressionBuilder, ExpressionBuilderArgument> targetNode))
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
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder is InspectBuilder inspectBuilder)
            {
                var result = UnwrapConvert(inspectBuilder.Builder, selector);
                return new InspectBuilder(result);
            }

            if (builder is DisableBuilder disableBuilder)
            {
                var result = UnwrapConvert(disableBuilder.Builder, selector);
                return new DisableBuilder(result);
            }

            if (builder is WorkflowExpressionBuilder workflowExpression)
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
            if (builder is IncludeWorkflowBuilder includeWorkflow && recurse)
            {
                builder = new IncludeWorkflowBuilder(includeWorkflow, true);
                builder.InstanceNumber = includeWorkflow.InstanceNumber;
            }
            return new InspectBuilder(builder);
        }

        static ExpressionBuilder RemoveInspectBuilder(InspectBuilder inspectBuilder, bool recurse)
        {
            var builder = inspectBuilder.Builder;
            if (builder is IncludeWorkflowBuilder includeWorkflow && recurse)
            {
                builder = new IncludeWorkflowBuilder(includeWorkflow, false);
                builder.InstanceNumber = includeWorkflow.InstanceNumber;
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
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!TopologicalSort.TrySort(source, out IEnumerable<DirectedGraph<ExpressionBuilder, ExpressionBuilderArgument>> buildOrder))
            {
                throw new ArgumentException("Cannot serialize a workflow with cyclical dependencies.", nameof(source));
            }

            int index = 0;
            var nodeMap = buildOrder
                .SelectMany(component => component)
                .ToDictionary(node => node, node => index++);

            var nodes = new List<ExpressionBuilder>(nodeMap.Count);
            nodes.AddRange(nodeMap.Keys.Select(node => node.Value));

            var edges = new List<ExpressionBuilderArgumentDescriptor>();
            foreach (var entry in nodeMap)
            {
                var from = entry.Value;
                foreach (var successor in entry.Key.Successors)
                {
                    var to = nodeMap[successor.Target];
                    edges.Add(new ExpressionBuilderArgumentDescriptor(from, to, successor.Label.Name));
                }
            }

            edges.Sort((x, y) =>
            {
                var from = x.From.CompareTo(y.From);
                if (from != 0) return from;
                else return x.To.CompareTo(y.To);
            });
            return new ExpressionBuilderGraphDescriptor(nodes, edges);
        }

        /// <summary>
        /// Adds the contents of the specified graph descriptor to the <see cref="ExpressionBuilderGraph"/>.
        /// </summary>
        /// <param name="source">
        /// The directed graph on which to add the contents of <paramref name="descriptor"/>.
        /// </param>
        /// <param name="descriptor">
        /// The serializable descriptor whose contents should be added to the <see cref="ExpressionBuilderGraph"/>.
        /// </param>
        public static void AddDescriptor(this ExpressionBuilderGraph source, ExpressionBuilderGraphDescriptor descriptor)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            var nodes = descriptor.Nodes
                .Select(value => new Node<ExpressionBuilder, ExpressionBuilderArgument>(value))
                .ToArray();
            foreach (var edge in descriptor.Edges)
            {
                var label = new ExpressionBuilderArgument(edge.Label);
                nodes[edge.From].Successors.Add(Edge.Create(nodes[edge.To], label));
            }

            if (!TopologicalSort.TrySort(nodes, out IEnumerable<DirectedGraph<ExpressionBuilder, ExpressionBuilderArgument>> buildOrder))
            {
                throw new ArgumentException("Cannot deserialize a workflow with cyclical dependencies.", nameof(source));
            }

            source.AddRange(buildOrder.SelectMany(component => component));
        }

        #endregion

        #region Workflow Traversal

        /// <summary>
        /// Returns a filtered collection of the child elements for this workflow.
        /// </summary>
        /// <param name="source">The expression builder workflow to search.</param>
        /// <returns>An enumerable sequence of all the elements in this workflow.</returns>
        public static IEnumerable<ExpressionBuilder> Elements(this ExpressionBuilderGraph source)
        {
            foreach (var node in source)
            {
                yield return ExpressionBuilder.Unwrap(node.Value);
            }
        }

        /// <summary>
        /// Returns a filtered collection of the descendant elements for this workflow, including elements
        /// nested inside grouped workflows. Any descendants of disabled groups will not be included in the result.
        /// </summary>
        /// <param name="source">The expression builder workflow to search.</param>
        /// <returns>An enumerable sequence of all the descendant elements in this workflow.</returns>
        public static IEnumerable<ExpressionBuilder> Descendants(this ExpressionBuilderGraph source)
        {
            return Descendants(source, unwrap: true);
        }

        /// <summary>
        /// Returns a filtered collection of the descendant elements for this workflow, including elements
        /// nested inside grouped workflows. Any descendants of disabled groups will not be included in the result.
        /// </summary>
        /// <param name="source">The expression builder workflow to search.</param>
        /// <param name="unwrap">A value indicating whether to unwrap descendant elements.</param>
        /// <returns>An enumerable sequence of all the descendant elements in this workflow.</returns>
        public static IEnumerable<ExpressionBuilder> Descendants(this ExpressionBuilderGraph source, bool unwrap)
        {
            var stack = new Stack<IEnumerator<Node<ExpressionBuilder, ExpressionBuilderArgument>>>();
            stack.Push(source.GetEnumerator());

            while (stack.Count > 0)
            {
                var nodeEnumerator = stack.Peek();
                while (true)
                {
                    if (!nodeEnumerator.MoveNext())
                    {
                        stack.Pop();
                        break;
                    }

                    var nodeValue = nodeEnumerator.Current.Value;
                    var builder = ExpressionBuilder.Unwrap(nodeValue);
                    yield return unwrap ? builder : nodeValue;

                    if (builder is IWorkflowExpressionBuilder workflowBuilder && workflowBuilder.Workflow != null)
                    {
                        stack.Push(workflowBuilder.Workflow.GetEnumerator());
                        break;
                    }
                }
            }
        }

        #endregion
    }
}
