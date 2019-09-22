using Bonsai.Resources;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Audio.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class SoundBuffer : BufferConfiguration
    {
        [Description("The name of the sound WAV file.")]
        [FileNameFilter("WAV Files (*.wav;*.wave)|*.wav;*.wave|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        public string FileName { get; set; }

        public override Buffer CreateResource(ResourceManager resourceManager)
        {
            var buffer = base.CreateResource(resourceManager);
            using (var reader = new BinaryReader(OpenResource(FileName)))
            {
                RiffHeader header;
                RiffReader.ReadHeader(reader, out header);

                var format = header.GetFormat();
                var sampleData = new byte[header.DataLength];
                var bytesRead = reader.Read(sampleData, 0, sampleData.Length);
                if (bytesRead < sampleData.Length)
                {
                    throw new InvalidOperationException("Unable to read audio data. Sound WAV file may be corrupted or truncated.");
                }

                AL.BufferData(buffer.Id, format, sampleData, sampleData.Length, (int)header.SampleRate);
            }

            return buffer;
        }

        public override string ToString()
        {
            var name = Name;
            var fileName = FileName;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else if (string.IsNullOrEmpty(fileName)) return name;
            else return string.Format("{0} [{1}]", name, fileName);
        }
    }
}
