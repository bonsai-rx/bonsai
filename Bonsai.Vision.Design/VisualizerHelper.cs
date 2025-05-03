using Bonsai.Dag;
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

        internal static IObservable<object> ObservableInput(IServiceProvider provider, Type observableType)
        {
            var inputInspector = default(InspectBuilder);
            var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            if (workflow != null && context != null)
            {
                inputInspector = workflow.Where(node => node.Value == context.Source)
                                         .Select(node => workflow.Predecessors(node)
                                                                 .Select(p => p.Value as InspectBuilder)
                                                                 .FirstOrDefault())
                                         .FirstOrDefault();
            }

            if (inputInspector != null && inputInspector.ObservableType == observableType)
            {
                return inputInspector.Output.Merge();
            }
            else return null;
        }
    }
}
