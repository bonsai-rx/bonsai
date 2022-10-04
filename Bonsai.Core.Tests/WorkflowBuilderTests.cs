using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class WorkflowBuilderTests
    {
        [TestMethod]
        public void Serialize_DerivedXmlType_BasePropertiesShouldSerializeWithBaseXmlNamespace()
        {
            var builder = new StringBuilder();
            var workflow = new WorkflowBuilder();
            var derivedClass = new DerivedClassWithProperty();
            derivedClass.DueTime = TimeSpan.FromSeconds(10);
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = derivedClass });
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = new DerivedXmlTypeWithProperty() });

            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true }))
            {
                WorkflowBuilder.Serializer.Serialize(writer, workflow);
            }

            var xml = builder.ToString();
            var document = XDocument.Parse(xml);
            var element = document.Descendants(XName.Get(nameof(Combinator), document.Root.Name.NamespaceName)).FirstOrDefault();
            var property = element.Descendants().FirstOrDefault(descendant => descendant.Name.LocalName == nameof(Timer.DueTime));
            Assert.IsNotNull(property);
            Assert.AreNotEqual(document.Root.Name.NamespaceName, property.Name.NamespaceName);
        }
    }

    public class DerivedClassWithProperty : Timer
    {
        public int NewProperty { get; set; }
    }

    [XmlType(Namespace = Constants.XmlNamespace)]
    public class DerivedXmlTypeWithProperty : Timer
    {
        public int NewProperty { get; set; }
    }
}
