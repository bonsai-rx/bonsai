using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Editor.GraphModel
{
    static class ElementStore
    {
        const string VersionAttributeName = "Version";
        static readonly XmlWriterSettings DefaultWriterSettings = new XmlWriterSettings
        {
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
            Indent = true
        };

        public static WorkflowBuilder LoadWorkflow(string fileName, out SemanticVersion version)
        {
            using var reader = XmlReader.Create(fileName);
            return LoadWorkflow(reader, out version);
        }

        public static WorkflowBuilder LoadWorkflow(XmlReader reader, out SemanticVersion version)
        {
            reader.MoveToContent();
            var versionName = reader.GetAttribute(VersionAttributeName);
            SemanticVersion.TryParse(versionName, out version);
            var serializer = new XmlSerializer(typeof(WorkflowBuilder), reader.NamespaceURI);
            return (WorkflowBuilder)serializer.Deserialize(reader);
        }

        public static string StoreWorkflowElements(WorkflowBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.Workflow.Count > 0)
            {
                var stringBuilder = new StringBuilder();
                using (var writer = XmlnsIndentedWriter.Create(stringBuilder, DefaultWriterSettings))
                {
                    WorkflowBuilder.Serializer.Serialize(writer, builder);
                }

                return stringBuilder.ToString();
            }

            return string.Empty;
        }

        public static WorkflowBuilder RetrieveWorkflowElements(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var stringReader = new StringReader(text);
                using var reader = XmlReader.Create(stringReader);
                try { return LoadWorkflow(reader, out _); }
                catch (XmlException) { }
            }

            return new WorkflowBuilder();
        }

        public static void SaveElement(XmlSerializer serializer, string fileName, object element)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = XmlnsIndentedWriter.Create(memoryStream, DefaultWriterSettings))
            {
                serializer.Serialize(writer, element);
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    memoryStream.WriteTo(fileStream);
                }
            }
        }
    }
}
