using Bonsai.Expressions;
using Bonsai.Dag;
using System;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides an abstract base class for visual property editors that require inspecting
    /// the runtime notifications of an operator to provide their functionality.
    /// </summary>
    public abstract class DataSourceTypeEditor : UITypeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceTypeEditor"/> class
        /// using the specified data source.
        /// </summary>
        /// <param name="source">
        /// Specifies the source of runtime notifications to the visual property editor.
        /// </param>
        protected DataSourceTypeEditor(DataSource source)
            : this(source, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceTypeEditor"/> class
        /// using the specified data source and target data type.
        /// </summary>
        /// <param name="source">
        /// Specifies the source of runtime notifications to the visual property editor.
        /// </param>
        /// <param name="targetType">
        /// The type of values emitted by the data source.
        /// </param>
        protected DataSourceTypeEditor(DataSource source, Type targetType)
        {
            Source = source;
            TargetType = targetType;
        }

        private DataSource Source { get; set; }

        private Type TargetType { get; set; }

        /// <summary>
        /// Specifies the source of runtime notifications to the visual property editor.
        /// </summary>
        protected enum DataSource
        {
            /// <summary>
            /// Runtime notifications will come from the first input sequence to the operator.
            /// </summary>
            Input,

            /// <summary>
            /// Runtime notifications will come from the observable output of the operator.
            /// </summary>
            Output
        }

        InspectBuilder GetDataSource(ExpressionBuilderGraph workflow, string mappingName)
        {
            var input = Source == DataSource.Input;
            var target = (from node in workflow
                          let externalizedMapping = ExpressionBuilder.GetWorkflowElement(node.Value) as ExternalizedMappingBuilder
                          where externalizedMapping != null
                          let mapping = externalizedMapping.ExternalizedProperties.FirstOrDefault(mapping =>
                              mappingName == (string.IsNullOrEmpty(mapping.DisplayName) ? mapping.Name : mapping.DisplayName))
                          where mapping != null
                          from successor in node.Successors
                          select new { successor.Target, mapping.Name })
                          .FirstOrDefault();
            return GetDataSource(workflow, target.Name, target.Target);
        }

        InspectBuilder GetDataSource(ExpressionBuilderGraph workflow, string mappingName, Node<ExpressionBuilder, ExpressionBuilderArgument> node)
        {
            if (node == null)
            {
                throw new InvalidOperationException(string.Format("'{0}' does not support this combinator type.", GetType()));
            }

            InspectBuilder dataSource;
            switch (Source)
            {
                case DataSource.Input:
                    dataSource = (InspectBuilder)workflow.Predecessors(node).First(p => !p.Value.IsBuildDependency()).Value;
                    break;
                case DataSource.Output: dataSource = (InspectBuilder)node.Value; break;
                default: throw new InvalidOperationException("The specified data source is not supported.");
            }

            if (TargetType != null && dataSource.ObservableType != TargetType)
            {
                if (ExpressionBuilder.GetWorkflowElement(node.Value) is IWorkflowExpressionBuilder workflowBuilder)
                {
                    return GetDataSource(workflowBuilder.Workflow, mappingName);
                }
            }
            return dataSource;
        }

        /// <summary>
        /// Gets the source of runtime notifications arriving to or from the operator.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> object that can be used to obtain
        /// additional context information.
        /// </param>
        /// <param name="provider">
        /// An <see cref="IServiceProvider"/> object that this editor can use to obtain services.
        /// </param>
        /// <returns>
        /// An <see cref="InspectBuilder"/> object that can be used to subscribe to runtime
        /// notifications arriving to or from the operator.
        /// </returns>
        protected InspectBuilder GetDataSource(ITypeDescriptorContext context, IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var mappingName = context.PropertyDescriptor.Name;
            if (context.Instance is ExpressionBuilderGraph nestedWorkflow)
            {
                return GetDataSource(nestedWorkflow, mappingName);
            }

            var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
            var workflowNode = (from node in workflow
                                where ExpressionBuilder.GetWorkflowElement(node.Value) == context.Instance
                                select node)
                                .FirstOrDefault();
            return GetDataSource(workflow, mappingName, workflowNode);
        }
    }
}
