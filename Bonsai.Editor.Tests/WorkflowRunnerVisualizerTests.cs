using System;
using System.Linq;
using Bonsai.Core.Tests;
using Bonsai.Design;
using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Editor.Tests
{
    [TestClass]
    public class WorkflowRunnerVisualizerTests
    {
        static WorkflowBuilder BuildWorkflow<T>(T value)
        {
            var workflowBuilder = new WorkflowBuilder(new TestWorkflow().AppendValue(value).ToInspectableGraph());
            workflowBuilder.Workflow.Build();
            return workflowBuilder;
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
        public void CreateVisualizerSettings_CompatibleVisualizer_AppliesLayout()
        {
            var workflowBuilder = BuildWorkflow(0);
            var typeVisualizers = new TypeVisualizerMap();
            typeVisualizers.Add(typeof(int), typeof(TestVisualizer));
            var layout = CreateLayoutForFirstNode(typeof(TestVisualizer).FullName);

            var settings = WorkflowRunner.CreateVisualizerSettings(workflowBuilder, layout, typeVisualizers);
            Assert.IsTrue(HasWindowSettings(settings, workflowBuilder));
        }

        [TestMethod]
        public void CreateVisualizerSettings_UnavailableVisualizer_SkipsLayoutWithoutThrowing()
        {
            var workflowBuilder = BuildWorkflow(0);
            var typeVisualizers = new TypeVisualizerMap();
            var layout = CreateLayoutForFirstNode("Missing.Visualizer.Type");

            var settings = WorkflowRunner.CreateVisualizerSettings(workflowBuilder, layout, typeVisualizers);
            Assert.IsFalse(HasWindowSettings(settings, workflowBuilder));
        }

        [TestMethod]
        public void CreateVisualizerSettings_IncompatibleVisualizer_SkipsLayoutWithoutThrowing()
        {
            var workflowBuilder = BuildWorkflow("text");
            var typeVisualizers = new TypeVisualizerMap();
            typeVisualizers.Add(typeof(int), typeof(TestVisualizer));
            var layout = CreateLayoutForFirstNode(typeof(TestVisualizer).FullName);

            var settings = WorkflowRunner.CreateVisualizerSettings(workflowBuilder, layout, typeVisualizers);
            Assert.IsFalse(HasWindowSettings(settings, workflowBuilder));
        }

        class TestVisualizer : DialogTypeVisualizer
        {
            public override void Load(IServiceProvider provider) { }

            public override void Show(object value) { }

            public override void Unload() { }
        }
    }
}
