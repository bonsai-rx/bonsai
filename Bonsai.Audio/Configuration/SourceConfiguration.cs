using Bonsai.Resources;
using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Audio.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class SourceConfiguration : ResourceConfiguration<AudioSource>
    {
        public SourceConfiguration()
        {
            State = ALSourceState.Initial;
        }

        [TypeConverter(typeof(BufferNameConverter))]
        [Description("The optional name of a buffer to play when creating the source.")]
        public string BufferName { get; set; }

        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The direction vector of the audio source.")]
        public Vector3 Direction { get; set; }

        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The location of the audio source in three-dimensional space.")]
        public Vector3 Position { get; set; }

        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The velocity of the audio source in three-dimensional space.")]
        public Vector3 Velocity { get; set; }

        [Category("State")]
        [Description("Indicates whether the audio source is looping.")]
        public bool Looping { get; set; }

        [Category("State")]
        [Description("Indicates whether the audio source uses coordinates relative to the listener.")]
        public bool Relative { get; set; }

        [Category("State")]
        [Description("Indicates the state to which the source should be set after creation.")]
        public ALSourceState State { get; set; }

        public override AudioSource CreateResource(ResourceManager resourceManager)
        {
            var source = new AudioSource();
            source.Direction = Direction;
            source.Position = Position;
            source.Velocity = Velocity;
            source.Looping = Looping;
            source.Relative = Relative;

            var bufferName = BufferName;
            if (!string.IsNullOrEmpty(bufferName))
            {
                var buffer = resourceManager.Load<Buffer>(bufferName);
                AL.SourceQueueBuffer(source.Id, buffer.Id);
            }

            switch (State)
            {
                case ALSourceState.Paused:
                    AL.SourcePause(source.Id);
                    break;
                case ALSourceState.Playing:
                    AL.SourcePlay(source.Id);
                    break;
                case ALSourceState.Stopped:
                    AL.SourceStop(source.Id);
                    break;
                case ALSourceState.Initial:
                default:
                    break;
            }

            return source;
        }
    }
}
