using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that broadcast the values of an observable
    /// sequence to multiple subscribers using a shared subject. This is an abstract class.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    [XmlType("Subject", Namespace = Constants.XmlNamespace)]
    public abstract class SubjectBuilder : SubjectExpressionBuilder, IRequireBuildContext
    {
        IBuildContext buildContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubjectBuilder"/> class.
        /// </summary>
        protected SubjectBuilder()
            : base(minArguments: 1, maxArguments: 1)
        {
        }

        IBuildContext IRequireBuildContext.BuildContext
        {
            get { return buildContext; }
            set { buildContext = value; }
        }

        /// <summary>
        /// When overridden in a derived class, returns the expression
        /// that creates the shared subject.
        /// </summary>
        /// <param name="expression">
        /// The expression representing the observable input sequence.
        /// </param>
        /// <returns>
        /// The <see cref="Expression"/> that creates the shared subject.
        /// </returns>
        protected abstract Expression BuildSubject(Expression expression);

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
            if (buildContext == null)
            {
                throw new InvalidOperationException("No valid build context was provided.");
            }

            var source = arguments.First();
            var subjectFactory = BuildSubject(source);
            var parameterType = source.Type.GetGenericArguments()[0];
            var subjectExpression = buildContext.AddVariable(Name, subjectFactory);
            return Expression.Call(typeof(SubjectBuilder), nameof(Process), new[] { parameterType }, source, subjectExpression);
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, ISubject<TSource> subject)
        {
            return source.Do(subject);
        }
    }

    /// <summary>
    /// Provides a base class for expression builders that declare a shared subject of the specified type.
    /// This is an abstract class.
    /// </summary>
    /// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
    [WorkflowElementCategory(ElementCategory.Source)]
    public abstract class SubjectBuilder<T> : SubjectExpressionBuilder, IRequireBuildContext
    {
        IBuildContext buildContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubjectBuilder{T}"/> class.
        /// </summary>
        protected SubjectBuilder()
            : base(minArguments: 0, maxArguments: 0)
        {
        }

        IBuildContext IRequireBuildContext.BuildContext
        {
            get { return buildContext; }
            set { buildContext = value; }
        }

        /// <summary>
        /// When overridden in a derived class, creates the shared subject.
        /// </summary>
        /// <returns>A new instance of <see cref="ISubject{T}"/>.</returns>
        protected abstract ISubject<T> CreateSubject();

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
            if (buildContext == null)
            {
                throw new InvalidOperationException("No valid build context was provided.");
            }

            var builderExpression = Expression.Constant(this);
            var subjectFactory = Expression.Call(builderExpression, nameof(CreateSubject), null);
            var subjectExpression = buildContext.AddVariable(Name, subjectFactory);
            return Expression.Call(typeof(SubjectBuilder<T>), nameof(Generate), null, subjectExpression);
        }

        static IObservable<T> Generate(ISubject<T> subject)
        {
            return subject;
        }
    }
}
