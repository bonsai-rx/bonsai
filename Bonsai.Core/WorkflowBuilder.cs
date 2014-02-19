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
using System.Xml;
using System.Diagnostics;

namespace Bonsai
{
    /// <summary>
    /// Represents an XML serializable expression builder workflow container.
    /// </summary>
    public class WorkflowBuilder : IXmlSerializable
    {
        readonly ExpressionBuilderGraph workflow;
        const string VersionAttributeName = "Version";
        const string ExtensionTypeNodeName = "ExtensionTypes";
        const string WorkflowNodeName = "Workflow";
        const string TypeNodeName = "Type";

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowBuilder"/> class.
        /// </summary>
        public WorkflowBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowBuilder"/> class with the
        /// specified workflow instance.
        /// </summary>
        /// <param name="workflow">
        /// The <see cref="ExpressionBuilderGraph"/> that will be used by this builder.
        /// </param>
        public WorkflowBuilder(ExpressionBuilderGraph workflow)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            this.workflow = workflow;
        }

        /// <summary>
        /// Gets the <see cref="ExpressionBuilderGraph"/> instance used by this builder.
        /// </summary>
        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
        }

        #region IXmlSerializable Members

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadToFollowing(WorkflowNodeName);

            var workflowMarkup = reader.ReadOuterXml();

            reader.ReadToFollowing(ExtensionTypeNodeName);
            reader.ReadStartElement();
            var types = new HashSet<Type>();
            while (reader.ReadToNextSibling(TypeNodeName))
            {
                var type = Type.GetType(reader.ReadElementString(), true);
                types.Add(type);
            }

            if (reader.NodeType == XmlNodeType.EndElement && reader.Name == ExtensionTypeNodeName)
            {
                reader.ReadEndElement();
            }

            var serializer = GetXmlSerializer(types);
            using (var workflowReader = new StringReader(workflowMarkup))
            {
                var descriptor = (ExpressionBuilderGraphDescriptor)serializer.Deserialize(workflowReader);
                workflow.AddDescriptor(descriptor);
            }
            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            writer.WriteAttributeString(VersionAttributeName, versionInfo.ProductVersion);

            var types = new HashSet<Type>(GetExtensionTypes(workflow));
            var serializer = GetXmlSerializer(types);
            serializer.Serialize(writer, workflow.ToDescriptor(), serializerNamespaces);

            writer.WriteStartElement(ExtensionTypeNodeName);
            foreach (var type in types)
            {
                writer.WriteElementString(TypeNodeName, type.AssemblyQualifiedName);
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
                    serializerNamespaces.Add("xsd", XmlSchema.Namespace);
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

                    var rootAttribute = new XmlRootAttribute(WorkflowNodeName) { Namespace = Constants.XmlNamespace };
                    serializerCache = new XmlSerializer(typeof(ExpressionBuilderGraphDescriptor), overrides, serializerTypes.ToArray(), rootAttribute, null);
                }
            }

            return serializerCache;
        }

        static IEnumerable<object> GetWorkflowElements(ExpressionBuilder builder)
        {
            yield return builder;
            var element = ExpressionBuilder.GetWorkflowElement(builder);
            if (element != builder) yield return element;

            var binaryOperator = builder as BinaryOperatorBuilder;
            if (binaryOperator != null && binaryOperator.Operand != null) yield return binaryOperator.Operand;
        }

        static IEnumerable<Type> GetWorkflowElementTypes(ExpressionBuilder builder)
        {
            var workflowExpressionBuilder = ExpressionBuilder.GetWorkflowElement(builder) as WorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null)
            {
                return GetExtensionTypes(workflowExpressionBuilder.Workflow);
            }
            else return GetWorkflowElements(builder).Select(element => element.GetType());
        }

        static IEnumerable<Type> GetExtensionTypes(ExpressionBuilderGraph workflow)
        {
            return workflow.SelectMany(node => GetWorkflowElementTypes(node.Value)
                .Concat(Enumerable.Repeat(node.Value.GetType(), 1)))
                .Except(GetDefaultSerializerTypes());
        }

        #endregion
    }
}
