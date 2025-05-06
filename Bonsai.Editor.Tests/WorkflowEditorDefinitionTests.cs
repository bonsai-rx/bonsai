using Bonsai.Editor.GraphModel;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Editor.Tests
{
    public partial class WorkflowEditorTests
    {
        [TestMethod]
        public void GetSubjectDefinition_NestedSubscribeSubject_ReturnsClosestDefinition()
        {
            var workflowBuilder = EditorHelper.LoadEmbeddedWorkflow("NestedSubscribeSubjectWithClosestRedefinition.bonsai");
            var deferBuilder = workflowBuilder.Workflow.FindExpressionBuilder<Defer>();
            Assert.IsNotNull(deferBuilder);
            var selectManyBuilder = deferBuilder.Workflow.FindExpressionBuilder<SelectMany>();
            Assert.IsNotNull(selectManyBuilder);
            var definition = workflowBuilder.GetSubjectDefinition(selectManyBuilder.Workflow, "Values");
            Assert.IsNotNull(definition);
            Assert.AreSame(deferBuilder.Workflow, definition.Root.Key);
        }
    }
}
