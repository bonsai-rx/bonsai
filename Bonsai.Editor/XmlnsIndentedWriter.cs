using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bonsai.Editor
{
    class XmlnsIndentedWriter : XmlWriter
    {
        bool isRootElement;
        int indentLevel = -1;
        readonly Stream stream;
        readonly TextWriter textWriter;
        readonly XmlWriter writer;

        private XmlnsIndentedWriter(Stream output, XmlWriter baseWriter)
        {
            stream = output;
            writer = baseWriter;
        }

        private XmlnsIndentedWriter(TextWriter output, XmlWriter baseWriter)
        {
            textWriter = output;
            writer = baseWriter;
        }

        public static new XmlWriter Create(StringBuilder output, XmlWriterSettings settings)
        {
            var writer = XmlWriter.Create(output, settings);
            return new XmlnsIndentedWriter(new StringWriter(output, CultureInfo.InvariantCulture), writer);
        }

        public static new XmlWriter Create(Stream stream, XmlWriterSettings settings)
        {
            var writer = XmlWriter.Create(stream, settings);
            return new XmlnsIndentedWriter(stream, writer);
        }

        public override XmlWriterSettings Settings
        {
            get { return writer.Settings; }
        }

        public override WriteState WriteState
        {
            get { return writer.WriteState; }
        }

        public override string XmlLang
        {
            get { return writer.XmlLang; }
        }

        public override XmlSpace XmlSpace
        {
            get { return writer.XmlSpace; }
        }

        public override void Flush()
        {
            writer.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return writer.LookupPrefix(ns);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            writer.WriteBase64(buffer, index, count);
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            writer.WriteBinHex(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            writer.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            writer.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            writer.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            writer.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            writer.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            writer.WriteEndAttribute();
            if (indentLevel >= 0)
            {
                RawText(Environment.NewLine + new string(' ', indentLevel));
            }
        }

        public override void WriteEndDocument()
        {
            writer.WriteEndDocument();
        }

        public override void WriteEndElement()
        {
            writer.WriteEndElement();
        }

        public override void WriteEntityRef(string name)
        {
            writer.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            writer.WriteFullEndElement();
        }

        public override void WriteName(string name)
        {
            writer.WriteName(name);
        }

        public override void WriteNmToken(string name)
        {
            writer.WriteNmToken(name);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            writer.WriteProcessingInstruction(name, text);
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            writer.WriteQualifiedName(localName, ns);
        }

        public override void WriteRaw(string data)
        {
            writer.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            writer.WriteRaw(buffer, index, count);
        }

        private void RawText(string text)
        {
            writer.Flush();
            if (stream != null)
            {
                var buf = writer.Settings.Encoding.GetBytes(text);
                stream.Write(buf, 0, buf.Length);
            }
            else if (textWriter != null)
            {
                textWriter.Write(text);
            }
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            writer.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument(bool standalone)
        {
            isRootElement = true;
            writer.WriteStartDocument(standalone);
        }

        public override void WriteStartDocument()
        {
            isRootElement = true;
            writer.WriteStartDocument();
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (isRootElement)
            {
                if (indentLevel < 0) indentLevel = localName.Length + 1;
                else
                {
                    isRootElement = false;
                    indentLevel = -1;
                }
            }
            writer.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            writer.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            writer.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            writer.WriteWhitespace(ws);
        }
    }
}
