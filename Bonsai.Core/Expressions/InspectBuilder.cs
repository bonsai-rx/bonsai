using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Reactive;
using System.Reactive.Disposables;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that replays the latest notification from all the
    /// subscriptions made to its decorated builder.
    /// </summary>
    public class InspectBuilder : ExpressionBuilder, INamedElement
    {
        static readonly MethodInfo CreateSubjectMethod = typeof(InspectBuilder).GetMethod("CreateInspectorSubject", BindingFlags.NonPublic | BindingFlags.Instance);

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
            var source = Builder.Build(arguments);
            if (source == EmptyExpression.Instance) return source;
            if (ExpressionBuilder.IsReducible(source))
            {
                ObservableType = source.Type.GetGenericArguments()[0];
            }

            // If source is already an inspect node, use it
            // for output notifications, but not for errors
            var inspectBuilder = GetInspectBuilder(source);
            if (inspectBuilder != null)
            {
                Output = inspectBuilder.Output;
                ErrorEx = Observable.Empty<Exception>();
                return source;
            }
            else if (PublishNotifications && ObservableType != null)
            {
                source = HandleObservableCreationException(source);
                var subject = CreateSubjectMethod.MakeGenericMethod(ObservableType).Invoke(this, null);
                var subjectExpression = Expression.Constant(subject);
                return Expression.Call(Expression.Constant(this), "Process", new[] { ObservableType }, source, subjectExpression);
            }
            else
            {
                Output = Observable.Empty<IObservable<object>>();
                ErrorEx = Observable.Empty<Exception>();
                return source;
            }
        }

        static InspectBuilder GetInspectBuilder(Expression source)
        {
            MulticastBranchExpression multicastExpression;
            while ((multicastExpression = source as MulticastBranchExpression) != null)
            {
                source = multicastExpression.Source;
            }

            var methodCall = source as MethodCallExpression;
            if (methodCall != null && methodCall.Object != null && methodCall.Object.Type == typeof(InspectBuilder))
            {
                return (InspectBuilder)((ConstantExpression)methodCall.Object).Value;
            }

            return null;
        }

        ReplaySubject<IObservable<TSource>> CreateInspectorSubject<TSource>()
        {
            var subject = new ReplaySubject<IObservable<TSource>>(1);
            Output = subject.Select(ys => ys.Select(xs => (object)xs));
            Error = subject.Merge().IgnoreElements().Select(xs => Unit.Default);
            ErrorEx = subject.SelectMany(xs => xs
                .IgnoreElements()
                .Select(x => default(Exception))
                .Catch<Exception, Exception>(ex => Observable.Return(ex)));
            return subject;
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
                    catch (Exception ex)
                    {
                        throw new WorkflowRuntimeException(ex.Message, this, ex);
                    }
                    finally { sourceInspector.OnCompleted(); }
                });
            });
        }
    }
}
