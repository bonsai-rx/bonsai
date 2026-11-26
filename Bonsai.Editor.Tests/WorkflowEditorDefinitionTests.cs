using System.Linq;
using Bonsai.Core.Tests;
using Bonsai.Editor.GraphModel;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Editor.Tests
{
    public partial class WorkflowEditorTests
    {
        [TestMethod]
        public void GetSubjectDefinition_NestedSubscribeSubject_ReturnsClosestDefinition()
        {
            var workflowBuilder = ElementStore.LoadWorkflow("NestedSubscribeSubjectWithClosestRedefinition.bonsai");
            var deferBuilder = workflowBuilder.Workflow.FindExpressionBuilder<Defer>();
            Assert.IsNotNull(deferBuilder);
            var selectManyBuilder = deferBuilder.Workflow.FindExpressionBuilder<SelectMany>();
            Assert.IsNotNull(selectManyBuilder);
            var definition = workflowBuilder.GetSubjectDefinition(selectManyBuilder.Workflow, "Values");
            Assert.IsNotNull(definition);
            Assert.AreSame(deferBuilder.Workflow.FindExpressionBuilder<PublishSubject>(), definition.Subject);
        }

        [TestMethod]
        public void GetSubjectDefinition_IncludedDeclaration_IsReadOnly()
        {
            var workflow = new TestWorkflow()
                .Append(new IncludeWorkflowBuilder { Path = "Bonsai.Core.Tests:LookupNodesIncluded.bonsai" })
                .Workflow;
            workflow.Build();
            var definition = new WorkflowBuilder(workflow).GetSubjectDefinition(workflow, "Value");
            Assert.IsNotNull(definition);
            Assert.IsTrue(definition.IsReadOnly);
        }

        [TestMethod]
        public void GetSubjectReferences_NestedRedefinition_ExcludesShadowedReferences()
        {
            var workflowBuilder = ElementStore.LoadWorkflow("NestedSubscribeSubjectWithClosestRedefinition.bonsai");
            var outerReference = workflowBuilder.Workflow.FindExpressionBuilder<SubscribeSubject>();
            var definition = workflowBuilder.GetSubjectDefinition(workflowBuilder.Workflow, "Values");
            var references = definition.GetSubjectReferences().ToArray();
            CollectionAssert.AreEqual(new ExpressionBuilder[] { outerReference }, references);
        }

        [TestMethod]
        public void RenameSubject_DisabledReference_KeepsName()
        {
            var reference = new SubscribeSubject { Name = "Values" };
            var disabledReference = new SubscribeSubject { Name = "Values" };
            var workflow = new TestWorkflow()
                .AppendValue(0)
                .AppendSubject<BehaviorSubject>("Values")
                .ResetCursor()
                .Append(reference)
                .ResetCursor()
                .Append(new DisableBuilder(disabledReference))
                .ToInspectableGraph();
            var (editor, _) = CreateMockEditor(workflow);
            var definition = new WorkflowBuilder(workflow).GetSubjectDefinition(workflow, "Values");
            editor.RenameSubject(definition, "Renamed");
            Assert.AreEqual("Renamed", definition.Subject.Name);
            Assert.AreEqual("Renamed", reference.Name);
            Assert.AreEqual("Values", disabledReference.Name);
        }
    }
}
