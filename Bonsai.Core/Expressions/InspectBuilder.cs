﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Disposables;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that replays the latest notification from all the
    /// subscriptions made to its decorated builder.
    /// </summary>
    public sealed class InspectBuilder : ExpressionBuilder, INamedElement
    {
        static readonly MethodInfo CreateSubjectMethod = typeof(InspectBuilder).GetMethod(nameof(CreateInspectorSubject), BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectBuilder"/> class with the
        /// specified expression builder.
        /// </summary>
        /// <param name="builder">
        /// The expression builder whose notifications will be replayed by this inspector.
        /// </param>
        public InspectBuilder(ExpressionBuilder builder)
            : base(builder, decorator: false)
        {
            Builder = builder;
            PublishNotifications = true;
        }

        internal InspectBuilder VisualizerElement { get; set; }

        private VisualizerMappingList MappingList { get; set; }

        internal void AddVisualizerMapping(int index, InspectBuilder source, Type visualizerType)
        {
            MappingList ??= new VisualizerMappingList();
            MappingList.Add(index, source, visualizerType);
        }

        private InspectBuilder BuildVisualizerElement(InspectBuilder builder, IReadOnlyList<VisualizerMapping> visualizerMappings)
        {
            var visualizerElement = GetVisualizerElement(builder);
            if (visualizerMappings.Count > 0)
            {
                visualizerElement.MappingList ??= new VisualizerMappingList();
                visualizerElement.MappingList.AddRange(visualizerMappings);
            }

            return visualizerElement;
        }

        internal void ResetVisualizerMappings()
        {
            MappingList?.ResetVisualizerMappings();
        }

        internal IReadOnlyList<VisualizerMapping> VisualizerMappings
        {
            get { return MappingList?.VisualizerMappings ?? Array.Empty<VisualizerMapping>(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether runtime notifications from
        /// the decorated expression builder should be multicast by this inspector.
        /// </summary>
        public bool PublishNotifications { get; set; }

        /// <summary>
        /// Gets the expression builder that is being decorated by this inspector.
        /// </summary>
        public ExpressionBuilder Builder { get; private set; }

        /// <summary>
        /// Gets the type of the elements in the output observable sequence.
        /// </summary>
        public Type ObservableType { get; private set; }

        /// <summary>
        /// Gets an observable sequence that multicasts notifications from all
        /// the subscriptions made to the output of the decorated expression builder.
        /// </summary>
        public IObservable<IObservable<object>> Output { get; private set; }

        /// <summary>
        /// Gets an observable sequence that multicasts errors and termination
        /// messages from all subscriptions made to the output of the decorated
        /// expression builder.
        /// </summary>
        [Obsolete]
        public IObservable<Unit> Error { get; private set; }

        /// <summary>
        /// Gets an observable sequence that multicasts error notifications
        /// from all subscriptions made to the output of the decorated
        /// expression builder.
        /// </summary>
        public IObservable<Exception> ErrorEx { get; private set; }

        /// <summary>
        /// Gets the range of input arguments that the decorated expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get { return Builder.ArgumentRange; }
        }

        /// <summary>
        /// Gets the display name of the decorated expression builder.
        /// </summary>
        public string Name
        {
            get { return GetElementDisplayName(Builder); }
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            ObservableType = null;
            ResetVisualizerMappings();
            var source = Builder.Build(arguments);
            if (source == EmptyExpression.Instance) return source;
            if (IsReducible(source))
            {
                ObservableType = source.Type.GetGenericArguments()[0];
            }

            // If source is already an inspect node, use it
            // for output notifications, but not for errors
            VisualizerElement = GetInspectBuilder(source);
            if (VisualizerElement != null)
            {
                Output = VisualizerElement.Output;
                ErrorEx = Observable.Empty<Exception>();
                VisualizerElement = BuildVisualizerElement(VisualizerElement, VisualizerMappings);
                return source;
            }
            else if (Builder.GetType() == typeof(Reactive.Visualizer))
            {
                var visualizerSource = GetInspectBuilder(((LambdaExpression)((MethodCallExpression)source).Arguments[1]).Body);
                if (visualizerSource != null) VisualizerElement = BuildVisualizerElement(visualizerSource, VisualizerMappings);
            }
            
            if (PublishNotifications && ObservableType != null)
            {
                source = HandleObservableCreationException(source);
                var subject = CreateSubjectMethod.MakeGenericMethod(ObservableType).Invoke(this, null);
                var subjectExpression = Expression.Constant(subject);
                return Expression.Call(Expression.Constant(this), nameof(Process), new[] { ObservableType }, source, subjectExpression);
            }
            else
            {
                Output = Observable.Empty<IObservable<object>>();
                ErrorEx = Observable.Empty<Exception>();
                if (VisualizerElement != null)
                {
                    return Expression.Call(Expression.Constant(this), nameof(Process), new[] { ObservableType }, source);
                }
                return source;
            }
        }

        internal static InspectBuilder GetInspectBuilder(Expression source)
        {
            while (source is MulticastBranchExpression multicastExpression)
            {
                source = multicastExpression.Source;
            }

            while (source is BlockExpression block)
            {
                source = block.Expressions.LastOrDefault();
            }

            while (source is MethodCallExpression methodCall)
            {
                if (methodCall.Object == null)
                {
                    if (methodCall.Method.DeclaringType == typeof(ExpressionBuilder))
                    {
                        // If merging dangling branches in a workflow, recurse on the main output source
                        if (methodCall.Method.Name == nameof(ExpressionBuilder.MergeOutput))
                        {
                            source = methodCall.Arguments[0];
                        }
                        // If merging with build dependencies in a workflow, recurse on the main output
                        else if (methodCall.Method.Name == nameof(ExpressionBuilder.MergeDependencies) &&
                                 methodCall.Arguments[0] is MethodCallExpression lazy &&
                                 lazy.Arguments[0] is LambdaExpression lambda)
                        {
                            source = lambda.Body;
                        }
                        else break;
                    }
                    // If disposing declared build context subjects, recurse on the output source
                    else if (methodCall.Method.DeclaringType == typeof(BuildContext) &&
                             methodCall.Method.Name == nameof(BuildContext.Finally))
                    {
                        source = methodCall.Arguments[0];
                    }
                    // If multicasting into a subject, recurse on the input source
                    else if ((methodCall.Method.DeclaringType == typeof(SubjectBuilder) ||
                             methodCall.Method.DeclaringType == typeof(MulticastSubjectBuilder)) &&
                             methodCall.Method.Name == nameof(Combinator.Process))
                    {
                        source = methodCall.Arguments[0];
                    }
                    else break;
                }
                else if (methodCall.Object.Type == typeof(InspectBuilder))
                {
                    return (InspectBuilder)((ConstantExpression)methodCall.Object).Value;
                }
                else if (methodCall.Object.Type.BaseType == typeof(MulticastBranchBuilder))
                {
                    // If closing multicast scope, recurse on the scope body
                    source = ((LambdaExpression)methodCall.Arguments[1]).Body;
                }
                else break;
            }

            return null;
        }

        ReplaySubject<IObservable<TSource>> CreateInspectorSubject<TSource>()
        {
            var subject = new ReplaySubject<IObservable<TSource>>(1);
            Output = subject.Select(ys => ys.Select(xs => (object)xs));
#pragma warning disable CS0612 // Type or member is obsolete
            Error = subject.Merge().IgnoreElements().Select(xs => Unit.Default);
#pragma warning restore CS0612 // Type or member is obsolete
            ErrorEx = subject.SelectMany(xs => xs
                .IgnoreElements()
                .Select(x => default(Exception))
                .Catch<Exception, Exception>(ex => Observable.Return(ex)));
            return subject;
        }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source;
        }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source, ReplaySubject<IObservable<TSource>> subject)
        {
            return Observable.Create<TSource>(observer =>
            {
                var sourceInspector = new Subject<TSource>();
                subject.OnNext(sourceInspector);
                var subscription = source.Do(sourceInspector).SubscribeSafe(observer);
                return Disposable.Create(() =>
                {
                    try { subscription.Dispose(); }
                    catch (WorkflowRuntimeException) { throw; }
                    catch (Exception ex)
                    {
                        throw new WorkflowRuntimeException(ex.Message, this, ex);
                    }
                    finally { sourceInspector.OnCompleted(); }
                });
            });
        }

        class VisualizerMappingList
        {
            readonly SortedList<int, VisualizerMapping> localMappings = new SortedList<int, VisualizerMapping>();

            public void Add(int index, InspectBuilder source, Type visualizerType)
            {
                var visualizerMapping = new VisualizerMapping(source, visualizerType);
                localMappings.Add(index, visualizerMapping);
            }

            public void AddRange(IReadOnlyList<VisualizerMapping> mappings)
            {
                VisualizerMappings ??= new List<VisualizerMapping>();
                ((List<VisualizerMapping>)VisualizerMappings).AddRange(mappings);
            }

            public void ResetVisualizerMappings()
            {
                VisualizerMappings = localMappings.Values.ToList();
                localMappings.Clear();
            }

            public IReadOnlyList<VisualizerMapping> VisualizerMappings { get; private set; }
        }
    }
}
