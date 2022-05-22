using Bonsai.Audio.Configuration;
using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that creates a spatialized source on the specified audio device.
    /// </summary>
    [Description("Creates a spatialized source on the specified audio device.")]
    public class CreateSource : Source<AudioSource>
    {
        readonly SourceConfiguration configuration = new SourceConfiguration();

        /// <summary>
        /// Gets or sets the name of the audio device used for playback.
        /// </summary>
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        [Description("The name of the audio device used for playback.")]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the optional name of a buffer to play when creating the source.
        /// </summary>
        [TypeConverter(typeof(BufferNameConverter))]
        [Description("The optional name of a buffer to play when creating the source.")]
        public string BufferName
        {
            get { return configuration.BufferName; }
            set { configuration.BufferName = value; }
        }

        /// <summary>
        /// Gets or sets the volume amplification applied to the audio source.
        /// </summary>
        [Precision(2, 0.01)]
        [Range(0, int.MaxValue)]
        [Category("Playback")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The volume amplification applied to the audio source.")]
        public float Gain
        {
            get { return configuration.Gain; }
            set { configuration.Gain = value; }
        }

        /// <summary>
        /// Gets or sets the pitch to be applied to the audio source.
        /// </summary>
        [Range(0.5f, 2.0f)]
        [Precision(2, 0.01)]
        [Category("Playback")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The pitch to be applied to the audio source.")]
        public float Pitch
        {
            get { return configuration.Pitch; }
            set { configuration.Pitch = value; }
        }

        /// <summary>
        /// Gets or sets the direction vector of the audio source.
        /// </summary>
        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The direction vector of the audio source.")]
        public Vector3 Direction
        {
            get { return configuration.Direction; }
            set { configuration.Direction = value; }
        }

        /// <summary>
        /// Gets or sets the current location of the audio source in three-dimensional space.
        /// </summary>
        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current location of the audio source in three-dimensional space.")]
        public Vector3 Position
        {
            get { return configuration.Position; }
            set { configuration.Position = value; }
        }

        /// <summary>
        /// Gets or sets the current velocity of the audio source in three-dimensional space.
        /// </summary>
        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current velocity of the audio source in three-dimensional space.")]
        public Vector3 Velocity
        {
            get { return configuration.Velocity; }
            set { configuration.Velocity = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the audio source is looping.
        /// </summary>
        [Category("State")]
        [Description("Indicates whether the audio source is looping.")]
        public bool Looping
        {
            get { return configuration.Looping; }
            set { configuration.Looping = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the audio source uses coordinates relative to the listener.
        /// </summary>
        [Category("State")]
        [Description("Indicates whether the audio source uses coordinates relative to the listener.")]
        public bool Relative
        {
            get { return configuration.Relative; }
            set { configuration.Relative = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the state to which the source should be set after creation.
        /// </summary>
        [Category("State")]
        [Description("Specifies the state to which the source should be set after creation.")]
        public ALSourceState State
        {
            get { return configuration.State; }
            set { configuration.State = value; }
        }

        /// <summary>
        /// Generates an observable sequence that contains the created audio source.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="AudioSource"/> instance.
        /// </returns>
        public override IObservable<AudioSource> Generate()
        {
            return Observable.Using(
                () => AudioManager.ReserveContext(DeviceName),
                resource => Observable.Return(configuration.CreateResource(resource.Context.ResourceManager)));
        }
    }
}
