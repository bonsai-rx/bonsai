using System.Linq;
using Bonsai.Core.Tests;
using Bonsai.Design;
using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Editor.Tests
{
    [TestClass]
    public class VisualizerLayoutMapTests
    {
        static WorkflowBuilder BuildWorkflow(ExpressionBuilder builder)
        {
            return new WorkflowBuilder(new TestWorkflow().Append(builder).ToInspectableGraph());
        }

        static VisualizerLayout CreateLayoutForFirstNode(string visualizerTypeName)
        {
            var layout = new VisualizerLayout();
            layout.WindowSettings.Add(new VisualizerWindowSettings
            {
                Index = 0,
                VisualizerTypeName = visualizerTypeName
            });
            return layout;
        }

        static bool HasWindowSettings(VisualizerLayoutMap settings, WorkflowBuilder workflowBuilder)
        {
            var source = (InspectBuilder)workflowBuilder.Workflow.First().Value;
            return settings.TryGetValue(source, out _);
        }

        [TestMethod]
        public void FromVisualizerLayout_UnavailableVisualizerOnDisabledNode_SkipsWithoutThrowing()
        {
            var workflowBuilder = BuildWorkflow(new DisableBuilder(new VisualizerWindow()));
            var typeVisualizers = new TypeVisualizerMap();
            var layout = CreateLayoutForFirstNode("Missing.Visualizer.Type");

            var settings = VisualizerLayoutMap.FromVisualizerLayout(workflowBuilder, layout, typeVisualizers);
            Assert.IsFalse(HasWindowSettings(settings, workflowBuilder));
        }
    }
}
