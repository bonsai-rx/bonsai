using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class ExternalizedPropertyDescriptorTests
    {
        static ExpressionBuilderGraph GetExternalizedTestWorkflow(string displayName)
        {
            var workflow = new ExpressionBuilderGraph();
            var groupWorkflow = new GroupWorkflowBuilder();
            var timerNode = groupWorkflow.Workflow.Add(new CombinatorBuilder { Combinator = new FloatProperty() });
            var externalizedTimeNode = groupWorkflow.Workflow.Add(new ExternalizedMappingBuilder
            {
                ExternalizedProperties = { new ExternalizedMapping { Name = "Value", DisplayName = displayName } }
            });
            groupWorkflow.Workflow.AddEdge(externalizedTimeNode, timerNode, new ExpressionBuilderArgument());

            var groupNode = workflow.Add(groupWorkflow);
            var externalizedNode = workflow.Add(new ExternalizedMappingBuilder
            {
                ExternalizedProperties = { new ExternalizedMapping { Name = displayName } }
            });

            workflow.AddEdge(externalizedNode, groupNode, new ExpressionBuilderArgument());
            return workflow;
        }

        [TestMethod]
        public void GetProperties_NonBrowsableNameClash_NonBrowsablePropertyIgnored()
        {
            var workflow = GetExternalizedTestWorkflow("Workflow");
            var properties = TypeDescriptor.GetProperties(workflow);
            var descriptor = properties["Workflow"];
            Assert.IsNotNull(descriptor);
            Assert.AreEqual(typeof(float), descriptor.PropertyType);
        }

        [TestMethod]
        public void SetWorkflowProperty_NestedPropertyAssignmentWithString_PropertyValueChanged()
        {
            var workflow = GetExternalizedTestWorkflow("Value");
            var properties = TypeDescriptor.GetProperties(workflow);
            var descriptor = properties["Value"];
            Assert.AreEqual(default(float), descriptor.GetValue(workflow));
            workflow.SetWorkflowProperty("Value", 5f);
            Assert.AreEqual(5f, descriptor.GetValue(workflow));
        }
    }
}
