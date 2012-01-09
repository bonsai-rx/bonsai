using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reflection;

namespace Bonsai.Expressions
{
    public static class ExpressionBuilderGraphExtensions
    {
        static readonly ConstructorInfo compositeDisposableConstructor = typeof(CompositeDisposable).GetConstructor(new[] { typeof(IEnumerable<IDisposable>) });
        static readonly MethodInfo subscribeMethod = typeof(ObservableExtensions).GetMethods().First(m => m.Name == "Subscribe" && m.GetParameters().Length == 1);

        public static IEnumerable<Expression> Build(this ExpressionBuilderGraph source)
        {
            foreach (var node in source.TopologicalSort())
            {
                var expression = node.Value.Build();
                if (node.Successors.Count > 1)
                {
                    // Publish workflow result to avoid repeating operations
                    var publishBuilder = new PublishBuilder { Source = expression };
                    var publish = Expression.Lambda(publishBuilder.Build()).Compile();
                    expression = Expression.Constant(publish.DynamicInvoke());
                }

                foreach (var successor in node.Successors)
                {
                    var target = successor.Node.Value.GetType().GetProperty(successor.Label);
                    target.SetValue(successor.Node.Value, expression, null);
                }

                if (node.Successors.Count == 0)
                {
                    yield return expression;
                }
            }
        }

        public static Expression<Func<IDisposable>> BuildSubscribe(this ExpressionBuilderGraph source)
        {
            var subscribeActions = from expression in source.Build()
                                   let observableType = expression.Type.GetGenericArguments()
                                   select Expression.Call(subscribeMethod.MakeGenericMethod(observableType), expression);

            var subscriptions = Expression.NewArrayInit(typeof(IDisposable), subscribeActions);
            var disposable = Expression.New(compositeDisposableConstructor, subscriptions);
            return Expression.Lambda<Func<IDisposable>>(disposable);
        }

        public static ExpressionBuilderGraph ToInspectableGraph(this ExpressionBuilderGraph source)
        {
            var observableMapping = new Dictionary<Node<ExpressionBuilder, string>, Tuple<Node<ExpressionBuilder, string>, Node<ExpressionBuilder, string>>>();
            var observableGraph = new ExpressionBuilderGraph();
            foreach (var node in source)
            {
                var observableExpression = new InspectBuilder();
                var observableNode = new Node<ExpressionBuilder, string>(observableExpression);
                var expressionNode = new Node<ExpressionBuilder, string>(node.Value);
                observableGraph.Add(expressionNode);
                observableGraph.Add(observableNode);
                observableGraph.AddEdge(expressionNode, observableNode, "Source");
                observableMapping.Add(node, Tuple.Create(expressionNode, observableNode));
            }

            foreach (var node in source)
            {
                var observableNode = observableMapping[node].Item2;
                foreach (var successor in node.Successors)
                {
                    var successorNode = observableMapping[successor.Node].Item1;
                    observableGraph.AddEdge(observableNode, successorNode, successor.Label);
                }
            }

            return observableGraph;
        }
    }
}
