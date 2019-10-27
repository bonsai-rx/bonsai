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
            Assert.IsNotNull(outerIncludeBuilder.PropertiesXml);
            Assert.AreEqual(1, outerIncludeBuilder.PropertiesXml.Length);
            var outerIncludeProperty = outerIncludeBuilder.PropertiesXml[0];
            var count = int.Parse(outerIncludeProperty.InnerText, CultureInfo.InvariantCulture);

            workflowBuilder.Workflow.Build();
            Assert.IsNotNull(outerIncludeBuilder.Workflow);
            var innerIncludeBuilder = (IncludeWorkflowBuilder)outerIncludeBuilder.Workflow
                .Single(node => node.Value is IncludeWorkflowBuilder).Value;
            Assert.IsNotNull(innerIncludeBuilder.Workflow);

            var property = TypeDescriptor.GetProperties(innerIncludeBuilder.Workflow)[outerIncludeProperty.Name];
            Assert.AreEqual(count, property.GetValue(innerIncludeBuilder.Workflow));
        }

        [TestMethod]
        public void SetWorkflowProperty_BeforeBuild_EnsureWorkflowIsLoadedAndInnerPropertyIsAssigned()
        {
            var propertyValue = 3;
            var propertyName = "Count";
            var workflowBuilder = LoadEmbeddedWorkflow("IncludeWorkflowOuter.bonsai");
            workflowBuilder.Workflow.SetWorkflowProperty(propertyName, propertyValue.ToString(CultureInfo.InvariantCulture));
            var innerIncludeBuilder = (IncludeWorkflowBuilder)workflowBuilder.Workflow
                .Single(node => node.Value is IncludeWorkflowBuilder).Value;
            Assert.IsNotNull(innerIncludeBuilder.Workflow);

            var property = TypeDescriptor.GetProperties(innerIncludeBuilder.Workflow)[propertyName];
            Assert.AreEqual(propertyValue, property.GetValue(innerIncludeBuilder.Workflow));
        }
    }
}
