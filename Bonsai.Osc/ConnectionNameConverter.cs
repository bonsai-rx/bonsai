using Bonsai.Expressions;
using Bonsai.Osc.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Osc
{
    class ConnectionNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
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
                                        .Concat(TransportManager.LoadConfiguration().Select(configuration => configuration.Name))
                                        .Distinct()
                                        .ToList();
                    return new StandardValuesCollection(channelNames);
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
