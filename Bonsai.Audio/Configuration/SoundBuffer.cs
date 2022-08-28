using Bonsai.Resources;
using OpenTK.Audio.OpenAL;
using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Bonsai.Audio.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for WAV audio buffers.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class SoundBuffer : BufferConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the sound WAV file.
        /// </summary>
        [Description("The name of the sound WAV file.")]
        [FileNameFilter("WAV Files (*.wav;*.wave)|*.wav;*.wave|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        public string FileName { get; set; }

        /// <summary>
        /// Creates a new buffer resource by reading and storing audio data from a WAV file.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Buffer"/> class storing audio data from
        /// the loaded WAV file.
        /// </returns>
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
