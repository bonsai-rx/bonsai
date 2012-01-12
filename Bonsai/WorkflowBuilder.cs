using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;
using System.Xml.Serialization;
using System.IO;
using Bonsai.Expressions;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;

namespace Bonsai
{
    public class WorkflowBuilder : ILoadable, IXmlSerializable
    {
        readonly ExpressionBuilderGraph workflow = new ExpressionBuilderGraph();

        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
        }

        public IDisposable Load()
        {
            return new CompositeDisposable(from node in workflow
                                           from element in GetLoadableElements(node.Value)
                                           select element.Load());
        }

        public IEnumerable<Source> GetSources()
        {
            foreach (var node in workflow)
            {
                var sourceBuilder = node.Value as SourceBuilder;
                if (sourceBuilder != null)
                {
                    yield return sourceBuilder.Source;
                }
            }
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadToFollowing("Workflow");

            var workflowMarkup = reader.ReadOuterXml();

            reader.ReadToFollowing("ExtensionTypes");
            reader.ReadStartElement();
            var types = new HashSet<Type>();
            while (reader.ReadToNextSibling("Type"))
            {
                var type = Type.GetType(reader.ReadElementString());
                types.Add(type);
            }
            reader.ReadEndElement();

            var serializer = GetXmlSerializer(types);
            using (var workflowReader = new StringReader(workflowMarkup))
            {
                var descriptor = (DirectedGraphDescriptor<ExpressionBuilder, ExpressionBuilderParameter>)serializer.Deserialize(workflowReader);
                workflow.AddDescriptor(descriptor);
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            var types = new HashSet<Type>(GetExtensionTypes(workflow));
            var serializer = GetXmlSerializer(types);
            serializer.Serialize(writer, workflow.ToDescriptor(), serializerNamespaces);

            writer.WriteStartElement("ExtensionTypes");
            foreach (var type in types)
            {
                writer.WriteElementString("Type", type.AssemblyQualifiedName);
            }
            writer.WriteEndElement();
        }

        #endregion

        #region XmlSerializer Cache

        static HashSet<Type> serializerTypes;
        static XmlSerializer serializerCache;
        static XmlSerializerNamespaces serializerNamespaces;
        static readonly object cacheLock = new object();

        static XmlSerializer GetXmlSerializer(HashSet<Type> types)
        {
            lock (cacheLock)
            {
                if (serializerCache == null || !types.IsSubsetOf(serializerTypes))
                {
                    if (serializerTypes == null)
                    {
                        serializerTypes = types;
                        serializerNamespaces = new XmlSerializerNamespaces();
                        serializerNamespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                    }
                    else serializerTypes.UnionWith(types);

                    serializerCache = new XmlSerializer(typeof(DirectedGraphDescriptor<ExpressionBuilder, ExpressionBuilderParameter>), new XmlAttributeOverrides(), serializerTypes.ToArray(), new XmlRootAttribute("Workflow"), null);
                }
            }

            return serializerCache;
        }

        static IEnumerable<LoadableElement> GetLoadableElements(ExpressionBuilder expressionBuilder)
        {
            foreach (var property in expressionBuilder.GetType().GetProperties())
            {
                if (typeof(LoadableElement).IsAssignableFrom(property.PropertyType))
                {
                    var value = (LoadableElement)property.GetValue(expressionBuilder, null);
                    if (value != null)
                    {
                        yield return value;
                    }
                }
            }
        }

        static IEnumerable<Type> GetExtensionTypes(ExpressionBuilderGraph workflow)
        {
            return from node in workflow
                   from element in GetLoadableElements(node.Value)
                   select element.GetType();
        }

        #endregion
    }
}
