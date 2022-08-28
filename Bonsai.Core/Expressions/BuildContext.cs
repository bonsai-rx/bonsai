using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Bonsai.Expressions
{
    class BuildContext : IBuildContext
    {
        Expression buildResult;
        VariableCollection variables;

        public BuildContext(ExpressionBuilder buildTarget)
        {
            BuildTarget = buildTarget;
        }

        public BuildContext(IBuildContext parentContext)
        {
            ParentContext = parentContext ?? throw new ArgumentNullException(nameof(parentContext));
            BuildTarget = parentContext.BuildTarget;
        }

        public ExpressionBuilder BuildTarget { get; private set; }

        public Expression BuildResult
        {
            get { return buildResult; }
            set
            {
                buildResult = value;
                if (ParentContext != null)
                {
                    ParentContext.BuildResult = buildResult;
                }
            }
        }

        public IBuildContext ParentContext { get; }

        public ParameterExpression AddVariable(string name, Expression expression)
        {
            if (variables == null)
            {
                variables = new VariableCollection();
            }

            if (!string.IsNullOrEmpty(name) && variables.Contains(name))
            {
                throw new ArgumentException(
                    string.Format("A variable with the specified name '{0}' already exists.", name),
                    nameof(name));
            }

            var variable = new Variable(name, expression);
            variables.Add(variable);
            return variable.Parameter;
        }

        public ParameterExpression GetVariable(string name)
        {
            if (variables == null || !variables.Contains(name))
            {
                if (ParentContext != null)
                {
                    return ParentContext.GetVariable(name);
                }
                else throw new ArgumentException(
                    string.Format("The specified variable '{0}' was not found in the current build context.", name),
                    nameof(name));
            }

            return variables[name].Parameter;
        }

        public Expression CloseContext(Expression source)
        {
            if (variables == null)
            {
                return source;
            }

            var sourceType = source.Type.GetGenericArguments()[0];
            var parameters = variables.Select(variable => variable.Parameter).Reverse();
            var disposableConstructor = typeof(CompositeDisposable).GetConstructor(new[] { typeof(IDisposable[]) });
            var disposableParameters = parameters.Select(parameter => Expression.Convert(parameter, typeof(IDisposable)));
            var disposableExpression = Expression.New(disposableConstructor, Expression.NewArrayInit(typeof(IDisposable), disposableParameters));
            var finallyExpression = (Expression)Expression.Call(
                typeof(BuildContext),
                nameof(Finally),
                new[] { sourceType },
                source, disposableExpression);
            return Expression.Block(
                parameters,
                variables.Select(variable => Expression.Assign(variable.Parameter, variable.Factory))
                         .Concat(Enumerable.Repeat(finallyExpression, 1)));
        }

        internal static IObservable<TSource> Finally<TSource>(IObservable<TSource> source, IDisposable disposable)
        {
            return source.Finally(disposable.Dispose);
        }

        class Variable
        {
            public Variable(string name, Expression factory)
            {
                Factory = factory;
                Parameter = Expression.Variable(factory.Type, name);
            }

            public Expression Factory { get; private set; }

            public ParameterExpression Parameter { get; private set; }
        }

        class VariableCollection : KeyedCollection<string, Variable>
        {
            protected override string GetKeyForItem(Variable item)
            {
                return item.Parameter.Name;
            }
        }
    }
}
