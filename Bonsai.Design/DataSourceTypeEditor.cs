using Bonsai.Expressions;
using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Design
{
    public abstract class DataSourceTypeEditor : UITypeEditor
    {
        protected DataSourceTypeEditor(DataSource source)
            : this(source, null)
        {
        }

        protected DataSourceTypeEditor(DataSource source, Type targetType)
        {
            Source = source;
            TargetType = targetType;
        }

        private DataSource Source { get; set; }

        private Type TargetType { get; set; }

        protected enum DataSource
        {
            Input,
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
                var workflowBuilder = ExpressionBuilder.GetWorkflowElement(node.Value) as IWorkflowExpressionBuilder;
                if (workflowBuilder != null)
                {
                    return GetDataSource(workflowBuilder.Workflow, mappingName);
                }
            }
            return dataSource;
        }

        protected InspectBuilder GetDataSource(ITypeDescriptorContext context, IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            var mappingName = context.PropertyDescriptor.Name;
            var nestedWorkflow = context.Instance as ExpressionBuilderGraph;
            if (nestedWorkflow != null) return GetDataSource(nestedWorkflow, mappingName);

            var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
            var workflowNode = (from node in workflow
                                where ExpressionBuilder.GetWorkflowElement(node.Value) == context.Instance
                                select node)
                                .FirstOrDefault();
            return GetDataSource(workflow, mappingName, workflowNode);
        }
    }
}
