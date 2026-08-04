using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Bonsai;
using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: XmlNamespacePrefix(Bonsai.Core.Tests.BaseNamespace.BaseClassWithProperty.XmlNamespace, "pre")]
[assembly: XmlNamespacePrefix(Bonsai.Core.Tests.NamespaceWithPrefixClash.ClassWithProperty.XmlNamespace, "pre")]

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

        [TestMethod]
        public void Serialize_SameNameTypeArgumentsFromDifferentNamespaces_RoundTripSuccessful()
        {
            // identically named types from different namespaces are qualified by their CLR
            // namespace when used as type arguments, so both operands can be serialized
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new HasFlagBuilder
            {
                Operand = new WorkflowProperty<FirstNamespace.ValueKind> { Value = FirstNamespace.ValueKind.Second }
            });
            workflow.Workflow.Add(new HasFlagBuilder
            {
                Operand = new WorkflowProperty<SecondNamespace.ValueKind> { Value = SecondNamespace.ValueKind.First }
            });
            var xml = SerializeWorkflow(workflow);
            var roundTrip = DeserializeWorkflow(xml);
            var operandTypes = roundTrip.Workflow.Select(node => ((HasFlagBuilder)node.Value).Operand.GetType());
            CollectionAssert.AreEquivalent(
                new[]
                {
                    typeof(WorkflowProperty<FirstNamespace.ValueKind>),
                    typeof(WorkflowProperty<SecondNamespace.ValueKind>)
                },
                operandTypes.ToArray());
        }

        [TestMethod]
        public void Serialize_PrimitiveTypeArgument_RoundTripSuccessful()
        {
            // type arguments mapped to XML schema built-in types cannot be assigned any XML
            // attributes, either directly or through a nullable wrapper
            var value = 10L;
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new HasFlagBuilder { Operand = new WorkflowProperty<long> { Value = value } });
            workflow.Workflow.Add(new HasFlagBuilder { Operand = new WorkflowProperty<int?>() });
            var xml = SerializeWorkflow(workflow);
            var roundTrip = DeserializeWorkflow(xml);
            var operand = (WorkflowProperty<long>)((HasFlagBuilder)roundTrip.Workflow.First().Value).Operand;
            Assert.AreEqual(value, operand.Value);
        }

        [TestMethod]
        public void Serialize_SameNameNullableTypeArguments_RoundTripSuccessful()
        {
            // a nullable argument is named from its underlying type, so it is the underlying
            // type which needs to be qualified
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new HasFlagBuilder { Operand = new WorkflowProperty<FirstNamespace.NullableKind?>() });
            workflow.Workflow.Add(new HasFlagBuilder { Operand = new WorkflowProperty<SecondNamespace.NullableKind?>() });
            var xml = SerializeWorkflow(workflow);
            var roundTrip = DeserializeWorkflow(xml);
            var operandTypes = roundTrip.Workflow.Select(node => ((HasFlagBuilder)node.Value).Operand.GetType());
            CollectionAssert.AreEquivalent(
                new[]
                {
                    typeof(WorkflowProperty<FirstNamespace.NullableKind?>),
                    typeof(WorkflowProperty<SecondNamespace.NullableKind?>)
                },
                operandTypes.ToArray());
        }

        [TestMethod]
        public void Serialize_SameNameArrayTypeArguments_RoundTripSuccessful()
        {
            // an array argument is named from its element type, which is not included in the
            // generic arguments of the declaring type
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new HasFlagBuilder { Operand = new WorkflowProperty<FirstNamespace.ArrayKind[]>() });
            workflow.Workflow.Add(new HasFlagBuilder { Operand = new WorkflowProperty<SecondNamespace.ArrayKind[]>() });
            var xml = SerializeWorkflow(workflow);
            var roundTrip = DeserializeWorkflow(xml);
            var operandTypes = roundTrip.Workflow.Select(node => ((HasFlagBuilder)node.Value).Operand.GetType());
            CollectionAssert.AreEquivalent(
                new[]
                {
                    typeof(WorkflowProperty<FirstNamespace.ArrayKind[]>),
                    typeof(WorkflowProperty<SecondNamespace.ArrayKind[]>)
                },
                operandTypes.ToArray());
        }

        [TestMethod]
        public void Serialize_SelfDescribingTypeArgument_RoundTripSuccessful()
        {
            // type arguments which provide their own schema, or which are represented directly
            // as XML nodes, reject any assigned XML attributes
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new HasFlagBuilder { Operand = new WorkflowProperty<XElement>() });
            workflow.Workflow.Add(new HasFlagBuilder { Operand = new WorkflowProperty<XmlElement>() });
            var xml = SerializeWorkflow(workflow);
            var roundTrip = DeserializeWorkflow(xml);
            var operandTypes = roundTrip.Workflow.Select(node => ((HasFlagBuilder)node.Value).Operand.GetType());
            CollectionAssert.AreEquivalent(
                new[] { typeof(WorkflowProperty<XElement>), typeof(WorkflowProperty<XmlElement>) },
                operandTypes.ToArray());
        }

        [TestMethod]
        public void Serialize_NestedTypeArgumentNamespaces_QualifiedTypeArguments()
        {
            // type arguments reachable only through a wrapper or a nested generic type still
            // need their namespace declared, so encoded type arguments can be resolved on load
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new HasFlagBuilder { Operand = new WorkflowProperty<FirstNamespace.ValueKind?>() });
            workflow.Workflow.Add(new HasFlagBuilder { Operand = new WorkflowProperty<FirstNamespace.ValueKind[]>() });
            workflow.Workflow.Add(new SubscribeSubject<List<FirstNamespace.ValueKind>>());
            var xml = SerializeWorkflow(workflow);
            var prefix = Regex.Match(xml, @"xmlns:(\w+)=""clr-namespace:Bonsai\.Core\.Tests\.FirstNamespace;").Groups[1].Value;
            Assert.IsFalse(string.IsNullOrEmpty(prefix));
            StringAssert.Contains(xml, $"TypeArguments=\"sys:Nullable({prefix}:ValueKind)\"");
            StringAssert.Contains(xml, $"TypeArguments=\"{prefix}:ValueKind[]\"");
            StringAssert.Contains(xml, $"TypeArguments=\"scg:List({prefix}:ValueKind)\"");
            DeserializeWorkflow(xml);
        }

        [TestMethod]
        public void Serialize_FailedSerializerConstruction_SubsequentSerializationSuccessful()
        {
            // a workflow that cannot be serialized should not invalidate the serializer cache
            // for any workflow serialized afterwards
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = new DuplicateXmlTypeWithProperty() });
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = new OtherDuplicateXmlTypeWithProperty() });
            Assert.ThrowsException<InvalidOperationException>(() => SerializeWorkflow(workflow));

            var value = 5;
            var validWorkflow = new WorkflowBuilder();
            validWorkflow.Workflow.Add(new CombinatorBuilder
            {
                Combinator = new UniqueXmlTypeWithProperty { Property = value }
            });
            var xml = SerializeWorkflow(validWorkflow);
            var roundTrip = DeserializeWorkflow(xml);
            var combinator = (UniqueXmlTypeWithProperty)((CombinatorBuilder)roundTrip.Workflow.First().Value).Combinator;
            Assert.AreEqual(value, combinator.Property);
        }

        [TestMethod]
        public void Serialize_NamespacePrefixClash_RoundTripSuccessful()
        {
            var value = 10;
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new NamespaceWithPrefixClash.ClassWithProperty { Property = value });
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = new BaseNamespace.BaseClassWithProperty { BaseProperty = 1 } });
            var xml = SerializeWorkflow(workflow);
            var roundTrip = DeserializeWorkflow(xml);
            var builder = roundTrip.Workflow.First().Value as NamespaceWithPrefixClash.ClassWithProperty;
            Assert.IsNotNull(builder);
            Assert.AreEqual(value, builder.Property);
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

    namespace NamespaceWithPrefixClash
    {
        [XmlType(Namespace = XmlNamespace)]
        public class ClassWithProperty : SelectBuilder
        {
            internal const string XmlNamespace = "clr-namespace:Bonsai.Core.Tests.NamespaceWithPrefixClash;assembly=Bonsai.Core.Tests";

            public int Property { get; set; }

            protected override Expression BuildSelector(Expression expression)
            {
                return expression;
            }
        }
    }

    namespace FirstNamespace
    {
        public enum ValueKind
        {
            First = 1,
            Second = 2
        }

        public enum NullableKind
        {
            First = 1,
            Second = 2
        }

        public enum ArrayKind
        {
            First = 1,
            Second = 2
        }
    }

    namespace SecondNamespace
    {
        public enum ValueKind
        {
            First = 1,
            Second = 2
        }

        public enum NullableKind
        {
            First = 1,
            Second = 2
        }

        public enum ArrayKind
        {
            First = 1,
            Second = 2
        }
    }

    [XmlType("DuplicateXmlType", Namespace = Constants.XmlNamespace)]
    public class DuplicateXmlTypeWithProperty : Combinator
    {
        public int Property { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            throw new NotImplementedException();
        }
    }

    [XmlType("DuplicateXmlType", Namespace = Constants.XmlNamespace)]
    public class OtherDuplicateXmlTypeWithProperty : Combinator
    {
        public int Property { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            throw new NotImplementedException();
        }
    }

    public class UniqueXmlTypeWithProperty : Combinator
    {
        public int Property { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            throw new NotImplementedException();
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
