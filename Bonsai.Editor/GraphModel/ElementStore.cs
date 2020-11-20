using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Editor.GraphModel
{
    static class ElementStore
    {
        static readonly XmlWriterSettings DefaultWriterSettings = new XmlWriterSettings
        {
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
            Indent = true
        };

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
                try
                {
                    reader.MoveToContent();
                    var serializer = new XmlSerializer(typeof(WorkflowBuilder), reader.NamespaceURI);
                    if (serializer.CanDeserialize(reader))
                    {
                        return (WorkflowBuilder)serializer.Deserialize(reader);
                    }
                }
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
