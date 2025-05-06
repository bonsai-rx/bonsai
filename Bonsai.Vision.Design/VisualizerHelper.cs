using Bonsai.Design;
using Bonsai.Expressions;
using OpenCV.Net;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision.Design
{
    static class VisualizerHelper
    {
        internal static IObservable<object> ImageInput(IServiceProvider provider)
        {
            return ObservableInput(provider, typeof(IplImage));
        }

        internal static IObservable<object> ObservableInput(IServiceProvider provider, Type targetType)
        {
            var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            if (workflow != null && context != null)
            {
                return FindObservableInput(workflow, context.Source, targetType)
                    ?? FindObservableInput(workflow, ExpressionBuilder.GetVisualizerElement(context.Source), targetType);
            }

            return null;
        }

        static IObservable<object> FindObservableInput(ExpressionBuilderGraph workflow, InspectBuilder target, Type targetType)
        {
            foreach (var node in workflow)
            {
                if (node.Value is not InspectBuilder inspectBuilder)
                    continue;

                foreach (var successor in node.Successors)
                {
                    var candidate = successor.Target.Value;
                    if (candidate == target)
                        return targetType.IsAssignableFrom(inspectBuilder.ObservableType)
                            ? inspectBuilder.Output.Merge()
                            : null;
                }

                if (inspectBuilder.Builder is IWorkflowExpressionBuilder workflowBuilder &&
                    workflowBuilder.Workflow is not null &&
                    ExpressionBuilder.GetVisualizerElement(inspectBuilder) is InspectBuilder visualizerElement &&
                    visualizerElement != inspectBuilder &&
                    (visualizerElement == target ||
                     ExpressionBuilder.GetVisualizerMappings(inspectBuilder)
                                      .Any(mapping => mapping.Source == target)))
                {
                    return FindObservableInput(workflowBuilder.Workflow, target, targetType);
                }
            }

            return null;
        }
    }
}
