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
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            Builder = builder;
        }

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
        public IObservable<Unit> Error { get; private set; }

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
            var source = Builder.Build(arguments);
            ObservableType = source.Type.GetGenericArguments()[0];

            // If source is already an inspect node, use it
            var methodCall = source as MethodCallExpression;
            if (methodCall != null && methodCall.Object != null && methodCall.Object.Type == typeof(InspectBuilder))
            {
                var inspectBuilder = (InspectBuilder)((ConstantExpression)methodCall.Object).Value;
                Output = inspectBuilder.Output;
                Error = inspectBuilder.Error;
                return source;
            }
            else
            {
                var subject = CreateSubjectMethod.MakeGenericMethod(ObservableType).Invoke(this, null);
                var subjectExpression = Expression.Constant(subject);
                return Expression.Call(typeof(InspectBuilder), "Process", new[] { ObservableType }, source, subjectExpression);
            }
        }

        ReplaySubject<IObservable<TSource>> CreateInspectorSubject<TSource>()
        {
            var subject = new ReplaySubject<IObservable<TSource>>(1, Scheduler.Immediate);
            Output = subject.Select(ys => ys.Select(xs => (object)xs));
            Error = subject.Merge().IgnoreElements().Select(xs => Unit.Default);
            return subject;
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, ReplaySubject<IObservable<TSource>> subject)
        {
            return Observable.Defer(() =>
            {
                var sourceInspector = new Subject<TSource>();
                subject.OnNext(sourceInspector);
                return source.Do(sourceInspector);
            });
        }
    }
}
