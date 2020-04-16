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

            if (inputInspector != null && inputInspector.ObservableType == typeof(IplImage))
            {
                return inputInspector.Output.Merge();
            }
            else return null;
        }
    }
}
