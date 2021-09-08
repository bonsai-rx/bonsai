using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class IncludeWorkflowTests
    {
        static Stream LoadEmbeddedResource(string name)
        {
            var qualifierType = typeof(IncludeWorkflowTests);
            var embeddedWorkflowStream = qualifierType.Namespace + "." + name;
            return qualifierType.Assembly.GetManifestResourceStream(embeddedWorkflowStream);
        }

        static WorkflowBuilder LoadEmbeddedWorkflow(string name)
        {
            using (var workflowStream = LoadEmbeddedResource(name))
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

        [TestMethod]
        public void Build_IncludedWorkflowWithPolymorphicProperties_ReuseTopLevelPrefixes()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("IncludeWorkflowPolymorphic.bonsai");
            workflowBuilder.Workflow.Build();

            using (var sw = new StringWriter())
            using (var reader = LoadEmbeddedResource("IncludeWorkflowPolymorphic.bonsai"))
            using (var writer = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, NamespaceHandling = NamespaceHandling.OmitDuplicates }))
            {
                WorkflowBuilder.Serializer.Serialize(writer, workflowBuilder);
                var text = sw.ToString();

                var input = XDocument.Load(reader);
                var output = XDocument.Parse(text);
                output.Root.SetAttributeValue("Version", input.Root.Attribute("Version").Value);
                var result = XNode.DeepEquals(input.Root, output.Root);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void Build_IncludedInnerWorkflowWithArguments_EnsureOutputType()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("IncludeWorkflowInner.bonsai");
            var unitArgument = new UnitBuilder().Build();
            var result = workflowBuilder.Workflow.Build(unitArgument);
            Assert.AreEqual(unitArgument.Type, result.Type);
        }

        [TestMethod]
        public void Build_NestedIncludeWorkflowMissing_EnsureStackTrace()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("IncludeWorkflowMissing.bonsai");
            try { workflowBuilder.Workflow.Build(); }
            catch (WorkflowBuildException ex)
            {
                Assert.IsInstanceOfType(ex.Builder, typeof(IncludeWorkflowBuilder));
                Assert.IsInstanceOfType(ex.InnerException, typeof(WorkflowBuildException));
                ex = (WorkflowBuildException)ex.InnerException;
                Assert.IsNotInstanceOfType(ex.Builder, typeof(IncludeWorkflowBuilder));
            }
        }

        [TestMethod]
        public void Serialize_WorkflowWithPolymorphicProperties_NoPrefixClash()
        {
            var builder = new WorkflowBuilder();
            var polymorphic = new PolymorphicPropertyTest();
            polymorphic.Types.Add(new PolyType());
            polymorphic.Types.Add(new MorphicType());
            polymorphic.Types.Add(new ExtraTypes.ExtraType());
            builder.Workflow.Add(new CombinatorBuilder { Combinator = polymorphic });

            using (var sw = new StringWriter())
            using (var writer = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true }))
            {
                WorkflowBuilder.Serializer.Serialize(writer, builder);
                var text = sw.ToString();
                var output = XDocument.Parse(text);
                var result = output.Root
                    .Descendants()
                    .Any(element => element.Name.LocalName == typeof(PolymorphicType).Name);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void Serialize_NonEqualExternalizedProperties_DoNotSerializeProperty()
        {
            var workflowName = "IncludeWorkflow.SharedProperties.NonEqualProperty.bonsai";
            var workflowBuilder = LoadEmbeddedWorkflow(workflowName);
            workflowBuilder.Workflow.Build();

            using (var sw = new StringWriter())
            using (var writer = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true }))
            {
                WorkflowBuilder.Serializer.Serialize(writer, workflowBuilder);
                var text = sw.ToString();
                var output = XDocument.Parse(text);
                var result = output.Root
                    .Descendants()
                    .Any(element => element.Name.LocalName == nameof(Reactive.Timer.DueTime));
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void Deserialize_SharedMixedXElementProperties_EnsureAssignment()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("IncludeWorkflow.SharedProperties.Mixed.bonsai");
            workflowBuilder.Workflow.Build();

            var dueTimeProperty = TypeDescriptor.GetProperties(workflowBuilder.Workflow)["DueTime"];
            var dueTime = dueTimeProperty.GetValue(workflowBuilder.Workflow);
            Assert.AreEqual(TimeSpan.FromSeconds(1), dueTime);
        }

        [TestMethod]
        public void Deserialize_PureMixedXElementProperties_EnsureAssignment()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("IncludeWorkflow.SharedProperties.Pure.bonsai");
            workflowBuilder.Workflow.Build();

            var dueTimeProperty = TypeDescriptor.GetProperties(workflowBuilder.Workflow)["DueTime"];
            var dueTime = dueTimeProperty.GetValue(workflowBuilder.Workflow);
            Assert.AreEqual(TimeSpan.FromSeconds(1), dueTime);
        }

        [TestMethod]
        public void ArgumentRange_MissingWorkflow_DoNotThrow()
        {
            var includeWorkflow = new IncludeWorkflowBuilder { Path = "MissingWorkflow.bonsai" };
            Assert.AreEqual(0, includeWorkflow.ArgumentRange.LowerBound);
        }

        [TestMethod]
        public void ArgumentRange_CopyConstructor_AreEqual()
        {
            var includeWorkflow = new IncludeWorkflowBuilder
            {
                Path = $"{typeof(IncludeWorkflowTests).Namespace}:IncludeWorkflow.ArgumentRange.TwoInputs.bonsai"
            };
            Assert.AreEqual(2, includeWorkflow.ArgumentRange.UpperBound);
            var inspectWorkflow = includeWorkflow.AsInspectBuilder();
            Assert.AreEqual(includeWorkflow.ArgumentRange.UpperBound, inspectWorkflow.ArgumentRange.UpperBound);
        }
    }

    public class PolymorphicPropertyTest : Sink
    {
        readonly List<PolymorphicType> types = new List<PolymorphicType>();

        public List<PolymorphicType> Types
        {
            get { return types; }
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source;
        }
    }
}
