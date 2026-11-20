using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Xml;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class OfTypeTests
    {
        class MixedObjectSource : Source<object>
        {
            public override IObservable<object> Generate()
            {
                return new object[] { 1, "two", null, 3.0, "four" }.ToObservable();
            }
        }

        static IList<TResult> RunOfType<TResult>()
        {
            return new TestWorkflow()
                .AppendCombinator(new MixedObjectSource())
                .Append(new OfType { TypeMapping = new TypeMapping<TResult>() })
                .AppendOutput()
                .BuildObservable<TResult>()
                .ToList().Wait();
        }

        [TestMethod]
        public void Build_MixedObjectSequence_FiltersMatchingReferenceTypeElements()
        {
            var results = RunOfType<string>();
            CollectionAssert.AreEqual(new[] { "two", "four" }, results.ToArray());
        }

        [TestMethod]
        public void Build_MixedObjectSequence_FiltersMatchingValueTypeElements()
        {
            var results = RunOfType<int>();
            CollectionAssert.AreEqual(new[] { 1 }, results.ToArray());
        }

        [TestMethod]
        public void Build_NoTypeMapping_FiltersNullElements()
        {
            var results = new TestWorkflow()
                .AppendCombinator(new MixedObjectSource())
                .Append(new OfType())
                .AppendOutput()
                .BuildObservable<object>()
                .ToList().Wait();
            CollectionAssert.AreEqual(new object[] { 1, "two", 3.0, "four" }, results.ToArray());
        }

        [TestMethod]
        public void SerializeDeserialize_TypeMapping_RoundTripEqualsOriginal()
        {
            var workflow = new WorkflowBuilder();
            workflow.Workflow.Add(new OfType { TypeMapping = new TypeMapping<string>() });
            workflow.Workflow.Add(new OfType { TypeMapping = new TypeMapping<List<Tuple<int, int>>>() });

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
