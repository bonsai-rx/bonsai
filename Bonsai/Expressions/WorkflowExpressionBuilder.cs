using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.ComponentModel;
using Bonsai.Dag;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Expressions
{
    [XmlType("Workflow", Namespace = Constants.XmlNamespace)]
    public abstract class WorkflowExpressionBuilder : CombinatorExpressionBuilder
    {
        readonly ExpressionBuilderGraph workflow;

        protected WorkflowExpressionBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        protected WorkflowExpressionBuilder(ExpressionBuilderGraph workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            this.workflow = workflow;
        }

        [Description("The name of the encapsulated workflow.")]
        public string Name { get; set; }

        [XmlIgnore]
        [Browsable(false)]
        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
        }

        [Browsable(false)]
        [XmlElement("Workflow")]
        public ExpressionBuilderGraphDescriptor WorkflowDescriptor
        {
            get { return workflow.ToDescriptor(); }
            set
            {
                workflow.Clear();
                workflow.AddDescriptor(value);
            }
        }

        static IObservable<Unit> IgnoreConnection<TSource>(IObservable<TSource> source)
        {
            return source.IgnoreElements().Select(xs => Unit.Default);
        }

        static IObservable<Unit> MergeOutput(params IObservable<Unit>[] connections)
        {
            return Observable.Merge(connections);
        }

        static IObservable<TSource> MergeOutput<TSource>(IObservable<TSource> source, params IObservable<Unit>[] connections)
        {
            return source.Merge(Observable.Merge(connections).Select(xs => default(TSource)).TakeUntil(source.TakeLast(1)));
        }

        protected Expression BuildOutput(WorkflowOutputBuilder workflowOutput, IEnumerable<Expression> connections)
        {
            var output = workflowOutput != null ? connections.SingleOrDefault(connection => connection == workflowOutput.Output) : null;
            var ignoredConnections = from connection in connections
                                     where connection != output
                                     let observableType = connection.Type.GetGenericArguments()[0]
                                     select Expression.Call(typeof(WorkflowExpressionBuilder), "IgnoreConnection", new[] { observableType }, connection);

            var connectionArrayExpression = Expression.NewArrayInit(typeof(IObservable<Unit>), ignoredConnections.ToArray());
            if (output != null)
            {
                var outputType = output.Type.GetGenericArguments()[0];
                return Expression.Call(typeof(WorkflowExpressionBuilder), "MergeOutput", new[] { outputType }, output, connectionArrayExpression);
            }
            else return Expression.Call(typeof(WorkflowExpressionBuilder), "MergeOutput", null, connectionArrayExpression);
        }
    }
}
