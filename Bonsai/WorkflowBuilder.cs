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
using System.Xml.Schema;

namespace Bonsai
{
    public class WorkflowBuilder : IXmlSerializable
    {
        readonly ExpressionBuilderGraph workflow;

        public WorkflowBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        public WorkflowBuilder(ExpressionBuilderGraph workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            this.workflow = workflow;
        }

        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
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
                var type = Type.GetType(reader.ReadElementString(), true);
                types.Add(type);
            }
            reader.ReadEndElement();

            var serializer = GetXmlSerializer(types);
            using (var workflowReader = new StringReader(workflowMarkup))
            {
                var descriptor = (ExpressionBuilderGraphDescriptor)serializer.Deserialize(workflowReader);
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

        static IEnumerable<Type> GetDefaultSerializerTypes()
        {
            return Attribute.GetCustomAttributes(typeof(ExpressionBuilder), typeof(XmlIncludeAttribute), false)
                            .Select(attribute => ((XmlIncludeAttribute)attribute).Type);
        }

        static string GetXmlNamespace(Type type)
        {
            var xmlTypeAttribute = (XmlTypeAttribute)Attribute.GetCustomAttribute(type, typeof(XmlTypeAttribute), false);
            if (xmlTypeAttribute != null) return xmlTypeAttribute.Namespace;
            return GetClrNamespace(type);
        }

        static string GetClrNamespace(Type type)
        {
            return string.Format("clr-namespace:{0};assembly={1}", type.Namespace, type.Assembly.GetName().Name);
        }

        static XmlSerializer GetXmlSerializer(HashSet<Type> types)
        {
            lock (cacheLock)
            {
                if (serializerCache == null || !types.IsSubsetOf(serializerTypes))
                {
                    if (serializerTypes == null) serializerTypes = types;
                    else serializerTypes.UnionWith(types);

                    int namespaceIndex = 1;
                    serializerNamespaces = new XmlSerializerNamespaces();
                    serializerNamespaces.Add("xsi", XmlSchema.InstanceNamespace);
                    foreach (var xmlNamespace in (from type in serializerTypes
                                                  let xmlNamespace = GetXmlNamespace(type)
                                                  where xmlNamespace != Constants.XmlNamespace
                                                  select xmlNamespace)
                                                 .Distinct())
                    {
                        serializerNamespaces.Add("q" + namespaceIndex, xmlNamespace);
                        namespaceIndex++;
                    }

                    XmlAttributeOverrides overrides = new XmlAttributeOverrides();
                    foreach (var type in serializerTypes)
                    {
                        if (Attribute.IsDefined(type, typeof(XmlTypeAttribute), false)) continue;

                        var attributes = new XmlAttributes();
                        attributes.XmlType = new XmlTypeAttribute { Namespace = GetClrNamespace(type) };
                        overrides.Add(type, attributes);
                    }

                    var rootAttribute = new XmlRootAttribute("Workflow") { Namespace = Constants.XmlNamespace };
                    serializerCache = new XmlSerializer(typeof(ExpressionBuilderGraphDescriptor), overrides, serializerTypes.ToArray(), rootAttribute, null);
                }
            }

            return serializerCache;
        }

        static IEnumerable<Type> GetLoadableElementTypes(ExpressionBuilder builder)
        {
            var workflowExpressionBuilder = builder as WorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null)
            {
                return GetExtensionTypes(workflowExpressionBuilder.Workflow);
            }
            else return builder.GetLoadableElements().Select(element => element.GetType());
        }

        static IEnumerable<Type> GetExtensionTypes(ExpressionBuilderGraph workflow)
        {
            return workflow.SelectMany(node => GetLoadableElementTypes(node.Value)
                .Concat(Enumerable.Repeat(node.Value.GetType(), 1)))
                .Except(GetDefaultSerializerTypes());
        }

        #endregion
    }
}
