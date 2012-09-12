using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Reflection;

namespace Bonsai
{
    public static class ReactiveWorkflowExtensions
    {
        static readonly ConstructorInfo compositeDisposableConstructor = typeof(CompositeDisposable).GetConstructor(new[] { typeof(IEnumerable<IDisposable>) });
        static readonly MethodInfo subscribeMethod = typeof(ObservableExtensions).GetMethods().First(m => m.Name == "Subscribe" && m.GetParameters().Length == 4);

        public static IObservable<Unit> Run(this ReactiveWorkflow source)
        {
            return Observable.Create<Unit>(observer =>
            {
                var loadDisposable = Disposable.Empty;
                try
                {
                    var subscribeExpression = source.BuildSubscribe(observer.OnError, observer.OnCompleted);
                    loadDisposable = source.Load();

                    var subscriber = subscribeExpression.Compile();
                    var subscribeDisposable = subscriber();
                    return new CompositeDisposable(subscribeDisposable, loadDisposable);
                }
                catch (Exception ex)
                {
                    return new CompositeDisposable(Observable.Throw<Unit>(ex).Subscribe(observer), loadDisposable);
                }
            });
        }

        static Expression<Func<IDisposable>> BuildSubscribe(this ReactiveWorkflow source, Action<Exception> onError)
        {
            return BuildSubscribe(source, onError, () => { });
        }

        static Expression<Func<IDisposable>> BuildSubscribe(this ReactiveWorkflow source, Action<Exception> onError, Action onCompleted)
        {
            var subscriptionCounter = Expression.Variable(typeof(int));
            var subscriptionInitializer = Expression.Assign(subscriptionCounter, Expression.Constant(source.Connections.Count));
            Expression<Action> onCompletedCall = () => onCompleted();

            var decrementCall = Expression.Call(typeof(Interlocked), "Decrement", null, subscriptionCounter);
            var comparison = Expression.LessThanOrEqual(decrementCall, Expression.Constant(0));
            var onCompletedCheck = Expression.IfThen(comparison, Expression.Invoke(onCompletedCall));

            var onErrorExpression = Expression.Constant(onError);
            var subscribeActions = from expression in source.Connections
                                   let observableType = expression.Type.GetGenericArguments()[0]
                                   let onNextParameter = Expression.Parameter(observableType)
                                   let onNext = Expression.Lambda(Expression.Empty(), onNextParameter)
                                   let onCompletedExpression = Expression.Lambda(onCompletedCheck)
                                   let subscribeCall = Expression.Call(subscribeMethod.MakeGenericMethod(observableType), expression, onNext, onErrorExpression, onCompletedExpression)
                                   select subscribeCall;

            var subscriptions = Expression.NewArrayInit(typeof(IDisposable), subscribeActions);
            var disposable = Expression.New(compositeDisposableConstructor, subscriptions);
            var subscribeBlock = Expression.Block(new[] { subscriptionCounter }, subscriptionInitializer, disposable);
            return Expression.Lambda<Func<IDisposable>>(subscribeBlock);
        }
    }
}
