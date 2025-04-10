using System;
using System.Collections.Generic;
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

        static IEnumerable<InspectBuilder> SelectContextElements(ExpressionBuilderGraph source)
        {
            foreach (var node in source)
            {
                if (!(node.Value is InspectBuilder inspectBuilder)) continue;
                yield return inspectBuilder;

                if (inspectBuilder.Builder is IGroupWorkflowBuilder groupBuilder)
                {
                    var workflow = groupBuilder.Workflow;
                    if (workflow == null) continue;
                    foreach (var groupElement in SelectContextElements(workflow))
                    {
                        yield return groupElement;
                    }
                }
            }
        }

        static bool GetCallContext(ExpressionBuilderGraph source, ExpressionBuilderGraph target, Stack<ExpressionBuilderGraph> context)
        {
            context.Push(source);
            if (source == target)
            {
                return true;
            }

            foreach (var inspectBuilder in SelectContextElements(source))
            {
                var element = inspectBuilder.Builder;
                if (element is IGroupWorkflowBuilder groupBuilder)
                {
                    if (groupBuilder.Workflow == target)
                        return true;
                }
                else if (element is WorkflowExpressionBuilder workflowBuilder)
                {
                    if (GetCallContext(workflowBuilder.Workflow, target, context))
                    {
                        return true;
                    }
                }
            }

            context.Pop();
            return false;
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

                    var callContext = new Stack<ExpressionBuilderGraph>();
                    if (GetCallContext(workflowBuilder.Workflow, workflow, callContext))
                    {
                        var names = (from level in callContext
                                     from inspectBuilder in SelectContextElements(level)
                                     let subjectBuilder = inspectBuilder.Builder as SubjectExpressionBuilder
                                     where subjectBuilder != null && !string.IsNullOrEmpty(subjectBuilder.Name) &&
                                           (targetType == null ||
                                           !multicast && targetType.IsAssignableFrom(inspectBuilder.ObservableType) ||
                                            multicast && ExpressionBuilder.HasConversion(inspectBuilder.ObservableType, targetType))
                                     select subjectBuilder.Name)
                                     .Distinct()
                                     .ToList();
                        return new StandardValuesCollection(names);
                    }
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
