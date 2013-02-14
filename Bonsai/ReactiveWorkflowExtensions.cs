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
        static readonly MethodInfo mergeMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Merge" &&
                                                                           m.GetParameters().Length == 1 &&
                                                                           m.GetParameters()[0].ParameterType.IsArray);

        public static IObservable<Unit> Run(this ReactiveWorkflow source, params LambdaExpression[] onNext)
        {
            return Observable.Create<Unit>(observer =>
            {
                var loadDisposable = Disposable.Empty;
                try
                {
                    var observableExpression = source.BuildObservable(onNext);
                    loadDisposable = source.Load();

                    var observableCreator = observableExpression.Compile();
                    var observable = observableCreator();
                    var subscribeDisposable = observable.Subscribe(xs => { }, observer.OnError, observer.OnCompleted);
                    return new CompositeDisposable(subscribeDisposable, loadDisposable);
                }
                catch (Exception ex)
                {
                    return new CompositeDisposable(Observable.Throw<Unit>(ex).Subscribe(observer), loadDisposable);
                }
            });
        }

        static IObservable<Unit> Connection<TSource>(IObservable<TSource> source, Action<TSource> onNext)
        {
            return source.Do(onNext).IgnoreElements().Select(xs => Unit.Default);
        }

        static Expression<Func<IObservable<Unit>>> BuildObservable(this ReactiveWorkflow source, params LambdaExpression[] onNext)
        {
            if (onNext == null)
            {
                throw new ArgumentNullException("onNext");
            }

            int connectionIndex = -1;
            var connections = from expression in source.Connections
                              let observableType = expression.Type.GetGenericArguments()[0]
                              let onNextParameter = Expression.Parameter(observableType)
                              let onNextExpression = ++connectionIndex < onNext.Length ? onNext[connectionIndex] : Expression.Lambda(Expression.Empty(), onNextParameter)
                              select Expression.Call(typeof(ReactiveWorkflowExtensions), "Connection", new[] { observableType }, expression, onNextExpression);

            var connectionArrayExpression = Expression.NewArrayInit(typeof(IObservable<Unit>), connections.ToArray());
            var observableExpression = Expression.Call(null, mergeMethod.MakeGenericMethod(typeof(Unit)), connectionArrayExpression);
            return Expression.Lambda<Func<IObservable<Unit>>>(observableExpression);
        }
    }
}
