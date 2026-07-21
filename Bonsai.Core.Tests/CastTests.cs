using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Xml;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class CastTests
    {
        [TestMethod]
        public void Build_ObjectSequenceWithCompatibleElements_CastsElementsToTargetType()
        {
            var result = new TestWorkflow()
                .AppendValue<object>("A")
                .Append(new Cast { TypeMapping = new TypeMapping<string>() })
                .AppendOutput()
                .BuildObservable<string>()
                .FirstAsync().Wait();
            Assert.AreEqual("A", result);
        }

        [TestMethod]
        public void Build_ObjectSequenceWithIncompatibleElements_ThrowsInvalidCastException()
        {
            var observable = new TestWorkflow()
                .AppendValue<object>(1)
                .Append(new Cast { TypeMapping = new TypeMapping<string>() })
                .AppendOutput()
                .BuildObservable<string>();
            Assert.ThrowsException<InvalidCastException>(() => observable.FirstAsync().Wait());
        }

        [TestMethod]
        public void Build_Int32SequenceCastToInt64_ThrowsInvalidCastException()
        {
            // consistent with Observable.Cast: boxed values only unbox to their exact type
            var observable = new TestWorkflow()
                .AppendValue(1)
                .Append(new Cast { TypeMapping = new TypeMapping<long>() })
                .AppendOutput()
                .BuildObservable<long>();
            Assert.ThrowsException<InvalidCastException>(() => observable.FirstAsync().Wait());
        }

        [TestMethod]
        public void Build_NoTypeMapping_CastsElementsToObject()
        {
            var result = new TestWorkflow()
                .AppendValue(1)
                .Append(new Cast())
                .AppendOutput()
                .BuildObservable<object>()
                .FirstAsync().Wait();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void Name_GenericTargetType_ReturnsTypeName()
        {
            INamedElement cast = new Cast { TypeMapping = new TypeMapping<List<int>>() };
            Assert.AreEqual("Cast(List<Int32>)", cast.Name);
        }

        [TestMethod]
        public void SerializeDeserialize_TypeMapping_RoundTripEqualsOriginal()
        {
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new Cast { TypeMapping = new TypeMapping<double>() });
            workflow.Workflow.Add(new Cast { TypeMapping = new TypeMapping<Tuple<int, string>>() });

            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true }))
            {
                WorkflowBuilder.Serializer.Serialize(writer, workflow);
            }

            var xml = builder.ToString();
            using (var reader = XmlReader.Create(new StringReader(xml)))
            {
                workflow = (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
            }

            builder.Clear();
            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true }))
            {
                WorkflowBuilder.Serializer.Serialize(writer, workflow);
            }
            Assert.AreEqual(xml, builder.ToString());
        }
    }
}
