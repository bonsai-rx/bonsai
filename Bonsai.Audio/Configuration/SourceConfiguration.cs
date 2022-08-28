using Bonsai.Resources;
using OpenTK;
using OpenTK.Audio.OpenAL;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Audio.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for audio sources.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class SourceConfiguration : ResourceConfiguration<AudioSource>
    {
        /// <summary>
        /// Gets or sets the optional name of a buffer to play when creating the source.
        /// </summary>
        [TypeConverter(typeof(BufferNameConverter))]
        [Description("The optional name of a buffer to play when creating the source.")]
        public string BufferName { get; set; }

        /// <summary>
        /// Gets or sets the volume amplification applied to the audio source.
        /// </summary>
        [Precision(2, 0.01)]
        [Range(0, int.MaxValue)]
        [Category("Playback")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The volume amplification applied to the audio source.")]
        public float Gain { get; set; } = 1;

        /// <summary>
        /// Gets or sets the pitch to be applied to the audio source.
        /// </summary>
        [Range(0.5f, 2.0f)]
        [Precision(2, 0.01)]
        [Category("Playback")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The pitch to be applied to the audio source.")]
        public float Pitch { get; set; } = 1;

        /// <summary>
        /// Gets or sets the direction vector of the audio source.
        /// </summary>
        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The direction vector of the audio source.")]
        public Vector3 Direction { get; set; }

        /// <summary>
        /// Gets or sets the location of the audio source in three-dimensional space.
        /// </summary>
        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The location of the audio source in three-dimensional space.")]
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the velocity of the audio source in three-dimensional space.
        /// </summary>
        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The velocity of the audio source in three-dimensional space.")]
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the audio source is looping.
        /// </summary>
        [Category("State")]
        [Description("Indicates whether the audio source is looping.")]
        public bool Looping { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the audio source uses coordinates
        /// relative to the listener.
        /// </summary>
        [Category("State")]
        [Description("Indicates whether the audio source uses coordinates relative to the listener.")]
        public bool Relative { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the state to which the source should be set after creation.
        /// </summary>
        [Category("State")]
        [Description("Specifies the state to which the source should be set after creation.")]
        public ALSourceState State { get; set; } = ALSourceState.Initial;

        /// <summary>
        /// Creates a new source of spatialized audio.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="AudioSource"/> class.
        /// </returns>
        /// <inheritdoc/>
        public override AudioSource CreateResource(ResourceManager resourceManager)
        {
            var source = new AudioSource();
            source.Gain = Gain;
            source.Pitch = Pitch;
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

            var state = State;
            if (state != ALSourceState.Initial)
            {
                source.SetState(state);
            }

            return source;
        }
    }
}
