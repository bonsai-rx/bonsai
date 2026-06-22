using System;
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

        static InvalidOperationException AssertThrowsForUnavailableVisualizer(ExpressionBuilder builder)
        {
            var workflowBuilder = BuildWorkflow(builder);
            var typeVisualizers = new TypeVisualizerMap();
            var layout = CreateLayoutForFirstNode("Missing.Visualizer.Type");
            return Assert.ThrowsException<InvalidOperationException>(
                () => VisualizerLayoutMap.FromVisualizerLayout(workflowBuilder, layout, typeVisualizers));
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

        [TestMethod]
        public void FromVisualizerLayout_UnavailableVisualizerOnVisualizerMappingBuilder_ReportsUnavailableType()
        {
            var exception = AssertThrowsForUnavailableVisualizer(new VisualizerMappingBuilder());
            StringAssert.Contains(exception.Message, "is not available");
        }

        [TestMethod]
        public void FromVisualizerLayout_UnavailableVisualizerOnRegularOperator_ReportsUnavailableType()
        {
            var exception = AssertThrowsForUnavailableVisualizer(
                new CombinatorBuilder { Combinator = new WorkflowProperty<int>() });
            StringAssert.Contains(exception.Message, "is not available");
        }

        [TestMethod]
        public void FromVisualizerLayout_UnavailableVisualizer_MessageNamesElementWithoutIndex()
        {
            var exception = AssertThrowsForUnavailableVisualizer(new VisualizerWindow());
            StringAssert.Contains(exception.Message, ExpressionBuilder.GetElementDisplayName(new VisualizerWindow()));
            StringAssert.Contains(exception.Message, "is not available");
            Assert.IsFalse(exception.Message.Contains('#'), "message should not expose a raw element index");
        }

        [TestMethod]
        public void FromVisualizerLayout_UnavailableVisualizerInGroup_MessageShowsBreadcrumbPath()
        {
            var workflowBuilder = new WorkflowBuilder(new TestWorkflow()
                .AppendNested(
                    inner => inner.Append(new VisualizerWindow()),
                    workflow => new GroupWorkflowBuilder(workflow) { Name = "MyGroup" })
                .ToInspectableGraph());
            var typeVisualizers = new TypeVisualizerMap();
            var nestedLayout = new VisualizerLayout();
            nestedLayout.WindowSettings.Add(new VisualizerWindowSettings
            {
                Index = 0,
                VisualizerTypeName = "Missing.Visualizer.Type"
            });
            var layout = new VisualizerLayout();
            layout.WindowSettings.Add(new VisualizerWindowSettings { Index = 0, NestedLayout = nestedLayout });

            var exception = Assert.ThrowsException<InvalidOperationException>(
                () => VisualizerLayoutMap.FromVisualizerLayout(workflowBuilder, layout, typeVisualizers));
            StringAssert.Contains(exception.Message, "MyGroup > ");
            StringAssert.Contains(exception.Message, "is not available");
        }

        [TestMethod]
        public void FromVisualizerLayout_LayoutIndexOutOfRange_ReportsMissingElementWithoutIndex()
        {
            var workflowBuilder = BuildWorkflow(new VisualizerWindow());
            var typeVisualizers = new TypeVisualizerMap();
            var layout = new VisualizerLayout();
            layout.WindowSettings.Add(new VisualizerWindowSettings { Index = 5 });

            var exception = Assert.ThrowsException<InvalidOperationException>(
                () => VisualizerLayoutMap.FromVisualizerLayout(workflowBuilder, layout, typeVisualizers));
            Assert.IsFalse(exception.Message.Contains('#'), "message should not expose a raw element index");
        }
    }
}
