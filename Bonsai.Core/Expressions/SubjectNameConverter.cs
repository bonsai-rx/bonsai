using System;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Expressions
{
    class SubjectNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                var workflow = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
                var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                if (workflow != null && workflowBuilder != null)
                {
                    Type targetType;
                    var componentType = context.PropertyDescriptor.ComponentType;
                    var multicast = componentType == typeof(MulticastSubject);
                    if (multicast)
                    {
                        var subjectTargetAttribute = (SubjectTargetAttribute)context.PropertyDescriptor.Attributes[typeof(SubjectTargetAttribute)];
                        targetType = subjectTargetAttribute.TargetType;
                    }
                    else targetType = componentType.IsGenericType ? componentType.GetGenericArguments()[0] : null;

                    var names = (from node in workflowBuilder.Workflow.LookupNodes(workflow)
                                 let inspectBuilder = node.Value as InspectBuilder
                                 let subjectBuilder = inspectBuilder?.Builder as SubjectExpressionBuilder
                                 where subjectBuilder != null && !string.IsNullOrEmpty(subjectBuilder.Name) &&
                                       (targetType == null ||
                                       !multicast && targetType.IsAssignableFrom(inspectBuilder.ObservableType) ||
                                        multicast && ExpressionBuilder.HasConversion(targetType, inspectBuilder.ObservableType))
                                 select subjectBuilder.Name)
                                 .Distinct()
                                 .ToList();
                    return new StandardValuesCollection(names);
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
