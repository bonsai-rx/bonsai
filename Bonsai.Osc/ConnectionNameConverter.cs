using Bonsai.Expressions;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Osc
{
    class ConnectionNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                if (workflowBuilder != null)
                {
                    var channelNames = (from builder in workflowBuilder.Workflow.Descendants()
                                        let createTransport = ExpressionBuilder.GetWorkflowElement(builder) as CreateTransport
                                        where createTransport != null && !string.IsNullOrEmpty(createTransport.Name)
                                        select createTransport.Name)
                                        .Distinct()
                                        .ToList();
                    return new StandardValuesCollection(channelNames);
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
