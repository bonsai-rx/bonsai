using Bonsai.Audio.Configuration;
using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    [Description("Creates a spatialized audio source on the specified output device.")]
    public class CreateSource : Source<AudioSource>
    {
        readonly SourceConfiguration configuration = new SourceConfiguration();

        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        [Description("The name of the output device used for playback.")]
        public string DeviceName { get; set; }

        [TypeConverter(typeof(BufferNameConverter))]
        [Description("The optional name of a buffer to play when creating the source.")]
        public string BufferName
        {
            get { return configuration.BufferName; }
            set { configuration.BufferName = value; }
        }

        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The direction vector of the audio source.")]
        public Vector3 Direction
        {
            get { return configuration.Direction; }
            set { configuration.Direction = value; }
        }

        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current location of the audio source in three-dimensional space.")]
        public Vector3 Position
        {
            get { return configuration.Position; }
            set { configuration.Position = value; }
        }

        [Category("Transform")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current velocity of the audio source in three-dimensional space.")]
        public Vector3 Velocity
        {
            get { return configuration.Velocity; }
            set { configuration.Velocity = value; }
        }

        [Category("State")]
        [Description("Indicates whether the audio source is looping.")]
        public bool Looping
        {
            get { return configuration.Looping; }
            set { configuration.Looping = value; }
        }

        [Category("State")]
        [Description("Indicates whether the audio source uses coordinates relative to the listener.")]
        public bool Relative
        {
            get { return configuration.Relative; }
            set { configuration.Relative = value; }
        }

        [Category("State")]
        [Description("Indicates the state to which the source should be set after creation.")]
        public ALSourceState State
        {
            get { return configuration.State; }
            set { configuration.State = value; }
        }

        public override IObservable<AudioSource> Generate()
        {
            return Observable.Using(
                () => AudioManager.ReserveContext(DeviceName),
                resource => Observable.Return(configuration.CreateResource(resource.Context.ResourceManager)));
        }
    }
}
