using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;
using System.Collections.ObjectModel;

namespace Bonsai
{
    [XmlRoot("Project")]
    public class WorkflowProject : ProcessingElement, IXmlSerializable
    {
        WorkflowCollection workflows;
        readonly List<WorkflowContext> executionContexts;

        public WorkflowProject()
        {
            workflows = new WorkflowCollection();
            executionContexts = new List<WorkflowContext>();
        }

        public WorkflowCollection Workflows
        {
            get { return workflows; }
        }

        public override void Start()
        {
            if (Running) return;

            foreach (var workflow in workflows)
            {
                workflow.Start();
            }

            base.Start();
        }

        public override void Stop()
        {
            if (!Running) return;

            foreach (var workflow in workflows)
            {
                workflow.Stop();
            }

            base.Stop();
        }

        public override void Load(WorkflowContext context)
        {
            for (int i = 0; i < workflows.Count; i++)
            {
                var executionContext = new WorkflowContext(context);
                executionContexts.Add(executionContext);
                workflows[i].Load(executionContext);
                
                var errorHandler = workflows[i].Error.Subscribe(OnError);
                executionContext.AddService(typeof(IDisposable), errorHandler);
            }

            // Add the workflow project as a context service
            context.AddService(typeof(WorkflowProject), this);
            base.Load(context);
        }

        public override void Unload(WorkflowContext context)
        {
            // Remove the workflow as a context service
            context.RemoveService(typeof(WorkflowProject));

            for (int i = 0; i < workflows.Count; i++)
            {
                var executionContext = executionContexts[i];
                workflows[i].Unload(executionContext);
                executionContext.Dispose();
            }
            executionContexts.Clear();
            base.Unload(context);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadToFollowing("Workflows");

            var workflowMarkup = reader.ReadOuterXml();

            reader.ReadToFollowing("ElementTypes");
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
                workflows = (WorkflowCollection)serializer.Deserialize(workflowReader);
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            var types = new HashSet<Type>(GetElementTypes(workflows));
            var serializer = GetXmlSerializer(types);
            serializer.Serialize(writer, workflows, serializerNamespaces);

            writer.WriteStartElement("ElementTypes");
            foreach (var type in types)
            {
                writer.WriteElementString("Type", type.AssemblyQualifiedName);
            }
            writer.WriteEndElement();
        }

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

                    serializerCache = new XmlSerializer(typeof(WorkflowCollection), serializerTypes.ToArray());
                }
            }
            
            return serializerCache;
        }

        static IEnumerable<Type> GetElementTypes(IEnumerable<IWorkflowContainer> workflows)
        {
            var types = Enumerable.Empty<Type>();
            foreach (var workflow in workflows)
            {
                types = types.Concat(GetElementTypes(workflow));
            }

            return types;
        }

        static IEnumerable<Type> GetElementTypes(IWorkflowContainer workflow)
        {
            if (workflow != null)
            {
                foreach (var component in workflow.Components)
                {
                    yield return component.GetType();

                    var container = component as IWorkflowContainer;
                    if (container != null)
                    {
                        foreach (var type in GetElementTypes(container))
                        {
                            yield return type;
                        }
                    }
                }
            }

            yield break;
        }

        #endregion
    }
}
