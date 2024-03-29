﻿using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

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
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = new MappingCombinator() });
            workflow.Workflow.Add(new CombinatorBuilder { Combinator = new GenericMappingCombinator<int>() });
            workflow.Workflow.Add(new AddBuilder { Operand = new WorkflowProperty<int>() });
#pragma warning disable CS0612 // Type or member is obsolete
            workflow.Workflow.Add(new ExternalizedTimeSpan<int>());
#pragma warning restore CS0612 // Type or member is obsolete
            workflow.Workflow.Add(new PropertySource<Reactive.WindowCount, int>());
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<Tuple<int, int>>() });
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<Tuple<Tuple<int, int, int>>>() });
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<Tuple<Tuple<int, int>, int>>() });
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<List<MappingNamespace1.Vector2>>() });
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<List<MappingNamespace2.Vector2>>() });
            workflow.Workflow.Add(new InputMappingBuilder { TypeMapping = new TypeMapping<System.Diagnostics.Stopwatch>() });
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

    public class MappingCombinator : Combinator
    {
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            throw new NotImplementedException();
        }
    }

    public class GenericMappingCombinator<T> : Combinator
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
