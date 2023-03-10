using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class WorkflowBuilderTests
    {
        [TestMethod]
        public void Serialize_MultipleDerivedXmlTypes_UniqueBaseXmlTypeDeclaration()
        {
            var builder = new StringBuilder();
            var workflow = new WorkflowBuilder();
            var derivedClass = new DerivedNamespace.DerivedClassWithProperty();
            derivedClass.BaseProperty = 10;
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = derivedClass });
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = new DerivedXmlTypeWithProperty() });

            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true }))
            {
                WorkflowBuilder.Serializer.Serialize(writer, workflow);
            }

            var xml = builder.ToString();
            var baseNamespaceDeclarations = Regex.Matches(xml, Regex.Escape(BaseNamespace.BaseClassWithProperty.XmlNamespace));
            Assert.AreEqual(1, baseNamespaceDeclarations.Count);
        }
    }

    namespace BaseNamespace
    {
        [XmlType(Namespace = XmlNamespace)]
        public class BaseClassWithProperty : Combinator
        {
            internal const string XmlNamespace = "clr-namespace:Bonsai.Core.Tests.BaseNamespace;assembly=Bonsai.Core.Tests";

            public int BaseProperty { get; set; }

            public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
            {
                throw new NotImplementedException();
            }
        }
    }

    namespace DerivedNamespace
    {
        public class DerivedClassWithProperty : IntermediateTypeWithProperty
        {
            public int NewProperty { get; set; }
        }
    }

    [XmlType(Namespace = Constants.XmlNamespace)]
    public class DerivedXmlTypeWithProperty : BaseNamespace.BaseClassWithProperty
    {
        public int NewProperty { get; set; }
    }

    public class IntermediateTypeWithProperty : BaseNamespace.BaseClassWithProperty
    {
        public int IntermediateProperty { get; set; }
    }
}
