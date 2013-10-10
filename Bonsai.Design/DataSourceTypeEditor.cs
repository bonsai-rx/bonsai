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
        {
            Source = source;
        }

        private DataSource Source { get; set; }

        protected enum DataSource
        {
            Input,
            Output
        }

        bool NodeSelector(ExpressionBuilder builder, object instance)
        {
            var selectBuilder = builder as SelectBuilder;
            if (selectBuilder != null) return selectBuilder.Selector == instance;

            var combinatorBuilder = builder as CombinatorBuilder;
            if (combinatorBuilder != null) return combinatorBuilder.Combinator == instance;
            return false;
        }

        protected IObservable<object> GetDataSource(ITypeDescriptorContext context, IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
            var workflowNode = (from node in workflow
                                where NodeSelector(node.Value, context.Instance)
                                select node)
                                .FirstOrDefault();
            if (workflowNode == null)
            {
                throw new InvalidOperationException(string.Format("'{0}' does not support this combinator type.", GetType()));
            }

            switch (Source)
            {
                case DataSource.Input: return ((InspectBuilder)workflow.Predecessors(workflowNode).First().Value).Output.Merge();
                case DataSource.Output: return ((InspectBuilder)workflow.Successors(workflowNode).First().Value).Output.Merge();
                default: throw new InvalidOperationException("The specified data source is not supported.");
            }
        }
    }
}
