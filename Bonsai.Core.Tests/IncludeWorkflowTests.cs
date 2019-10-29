using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class IncludeWorkflowTests
    {
        static WorkflowBuilder LoadEmbeddedWorkflow(string name)
        {
            var qualifierType = typeof(IncludeWorkflowTests);
            var embeddedWorkflowStream = qualifierType.Namespace + "." + name;
            using (var workflowStream = qualifierType.Assembly.GetManifestResourceStream(embeddedWorkflowStream))
            using (var reader = XmlReader.Create(workflowStream))
            {
                reader.MoveToContent();
                return (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
            }
        }

        [TestMethod]
        public void Build_NestedIncludeWorkflows_EnsureInnerPropertyIsAssigned()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("IncludeWorkflow.bonsai");
            var outerIncludeBuilder = (IncludeWorkflowBuilder)workflowBuilder.Workflow.Single().Value;
            var outerProperties = outerIncludeBuilder.PropertiesXml;
            Assert.IsNotNull(outerProperties);
            Assert.AreEqual(1, outerProperties.Length);
            var outerIncludeProperty = outerProperties[0];
            var count = int.Parse(outerIncludeProperty.Value, CultureInfo.InvariantCulture);

            workflowBuilder.Workflow.Build();
            Assert.IsNotNull(outerIncludeBuilder.Workflow);
            var innerIncludeBuilder = (IncludeWorkflowBuilder)outerIncludeBuilder.Workflow
                .Single(node => node.Value is IncludeWorkflowBuilder).Value;
            Assert.IsNotNull(innerIncludeBuilder.Workflow);
            var innerProperties = innerIncludeBuilder.PropertiesXml;
            Assert.IsNotNull(innerProperties);
            Assert.AreEqual(1, innerProperties.Length);
            var innerIncludeProperty = innerProperties[0];

            var property = TypeDescriptor.GetProperties(innerIncludeBuilder.Workflow)[innerIncludeProperty.Name.LocalName];
            Assert.AreEqual(count, property.GetValue(innerIncludeBuilder.Workflow));
        }

        [TestMethod]
        public void SetWorkflowProperty_BeforeBuild_EnsureWorkflowIsLoadedWithInnerPropertyAssigned()
        {
            var propertyValue = 3;
            var workflowBuilder = LoadEmbeddedWorkflow("IncludeWorkflowOuter.bonsai");
            var outerExternalizedMapping = (ExternalizedMappingBuilder)workflowBuilder.Workflow
                .Single(node => node.Value is ExternalizedMappingBuilder).Value;
            var outerPropertyName = outerExternalizedMapping.ExternalizedProperties.Single().DisplayName;
            workflowBuilder.Workflow.SetWorkflowProperty(outerPropertyName, propertyValue.ToString(CultureInfo.InvariantCulture));
            workflowBuilder.Workflow.Build();

            var innerIncludeBuilder = (IncludeWorkflowBuilder)workflowBuilder.Workflow
                .Single(node => node.Value is IncludeWorkflowBuilder).Value;
            Assert.IsNotNull(innerIncludeBuilder.Workflow);
            var innerExternalizedMapping = (ExternalizedMappingBuilder)innerIncludeBuilder.Workflow
                .Single(node => node.Value is ExternalizedMappingBuilder).Value;
            var innerPropertyName = innerExternalizedMapping.ExternalizedProperties.Single().Name;

            var property = TypeDescriptor.GetProperties(innerIncludeBuilder.Workflow)[innerPropertyName];
            Assert.AreEqual(propertyValue, property.GetValue(innerIncludeBuilder.Workflow));
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_SelfIncludingWorkflows_WorkflowBuildException()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("IncludeWorkflowSelfOuter.bonsai");
            workflowBuilder.Workflow.Build();
        }
    }
}
