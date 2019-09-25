using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class TypeMappingTests
    {
        [TestMethod]
        public void Serialize_Nameclash_DifferentNamespaces()
        {
            var workflow = new WorkflowBuilder();
            var builder = new StringBuilder();
            workflow.Workflow.Add(new CombinatorBuilder() { Combinator = new MappingCombinator() });
            workflow.Workflow.Add(new AddBuilder { Operand = new WorkflowProperty<int>() });
            workflow.Workflow.Add(new ExternalizedTimeSpan<int>());
            workflow.Workflow.Add(new PropertySource<Bonsai.Reactive.ElementCountWindow, int>());
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<Tuple<int, int>>() });
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<Tuple<Tuple<int, int, int>>>() });
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<Tuple<Tuple<int, int>, int>>() });
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<List<Bonsai.Core.Tests.MappingNamespace1.Vector2>>() });
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<List<Bonsai.Core.Tests.MappingNamespace2.Vector2>>() });
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<System.Diagnostics.Stopwatch>() });
            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true }))
            {
                WorkflowBuilder.Serializer.Serialize(writer, workflow);
            }

            var xml = builder.ToString();
            using (var reader = XmlReader.Create(new StringReader(xml)))
            {
                WorkflowBuilder.Serializer.Deserialize(reader);
            }

            builder.Clear();
            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { Indent = true }))
            {
                WorkflowBuilder.Serializer.Serialize(writer, workflow);
            }
            Assert.AreEqual(xml, builder.ToString());
        }
    }

    public class MappingCombinator : Combinator
    {
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            throw new NotImplementedException();
        }
    }

    namespace MappingNamespace1
    {
        public class Vector2
        {
            public float X;
            public float Y;
        }
    }

    namespace MappingNamespace2
    {
        public class Vector2
        {
            public float X;
            public float Y;
        }
    }
}
