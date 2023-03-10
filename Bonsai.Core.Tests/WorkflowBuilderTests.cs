using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
        private string SerializeWorkflow(WorkflowBuilder workflow)
        {
            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true }))
            {
                WorkflowBuilder.Serializer.Serialize(writer, workflow);
            }
            return builder.ToString();
        }

        private WorkflowBuilder DeserializeWorkflow(string xml)
        {
            using (var stringReader = new StringReader(xml))
            using (var reader = XmlReader.Create(stringReader))
            {
                reader.MoveToContent();
                return (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
            }
        }

        [TestMethod]
        public void Serialize_MultipleDerivedXmlTypes_UniqueBaseXmlTypeDeclaration()
        {
            var workflow = new WorkflowBuilder();
            var derivedClass = new DerivedNamespace.DerivedClassWithProperty();
            derivedClass.BaseProperty = 10;
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = derivedClass });
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = new DerivedXmlTypeWithProperty() });
            var xml = SerializeWorkflow(workflow);
            var baseNamespaceDeclarations = Regex.Matches(xml, Regex.Escape(BaseNamespace.BaseClassWithProperty.XmlNamespace));
            Assert.AreEqual(1, baseNamespaceDeclarations.Count);
        }

        [TestMethod]
        public void Serialize_DerivedTypeWithTypeMappingProperty_RoundTripSuccessful()
        {
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new CombinatorWithMapping { TypeMapping = new TypeMapping<int>() });
            var xml = SerializeWorkflow(workflow);
            var roundTrip = DeserializeWorkflow(xml);
            var builder = roundTrip.Workflow.First().Value as CombinatorWithMapping;
            Assert.IsNotNull(builder);
            Assert.AreEqual(typeof(TypeMapping<int>), builder.TypeMapping.GetType());
        }
    }

    [XmlInclude(typeof(TypeMapping<int>))]
    public class CombinatorWithMapping : SingleArgumentExpressionBuilder
    {
        public TypeMapping TypeMapping { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return arguments.First();
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
