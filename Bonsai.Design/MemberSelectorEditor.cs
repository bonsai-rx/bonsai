using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using Bonsai.Expressions;
using Bonsai.Dag;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Bonsai.Design
{
    public class MemberSelectorEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var selector = value as string ?? string.Empty;
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
                if (workflowBuilder == null) return base.EditValue(context, provider, value);

                var nodeBuilderGraph = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
                if (nodeBuilderGraph == null) return base.EditValue(context, provider, value);

                var workflow = workflowBuilder.Workflow;
                var builderNode = (from node in nodeBuilderGraph
                                   let builder = node.Value
                                   where ExpressionBuilder.GetWorkflowElement(builder) == context.Instance
                                   select node).SingleOrDefault();

                if (builderNode == null) return base.EditValue(context, provider, value);
                var predecessorNode = nodeBuilderGraph.Predecessors(builderNode).SingleOrDefault();

                if (predecessorNode == null) return base.EditValue(context, provider, value);
                var expression = workflow.Build(predecessorNode.Value);
                var expressionType = expression.Type.GetGenericArguments()[0];

                var instanceAttributes = TypeDescriptor.GetAttributes(context.Instance);
                var sourceMappingAttribute = (SourceMappingAttribute)instanceAttributes[typeof(SourceMappingAttribute)];
                if (sourceMappingAttribute != null && context.PropertyDescriptor.Name != sourceMappingAttribute.PropertyName)
                {
                    var memberSelectorProperty = builderNode.Value.GetType().GetProperty(sourceMappingAttribute.PropertyName);
                    if (memberSelectorProperty != null && memberSelectorProperty.PropertyType == typeof(string))
                    {
                        var memberPath = (string)memberSelectorProperty.GetValue(builderNode.Value, null);
                        var parameter = Expression.Parameter(expressionType);
                        expressionType = ExpressionHelper.MemberAccess(parameter, memberPath).Type;
                    }
                }

                var editorDialog = new MemberSelectorEditorDialog(expressionType, selector);
                if (editorService.ShowDialog(editorDialog) == DialogResult.OK)
                {
                    return string.Join(ExpressionHelper.MemberSeparator, editorDialog.GetMemberChain().ToArray());
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
