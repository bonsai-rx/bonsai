using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that creates an audio context using the specified device
    /// and listener properties.
    /// </summary>
    [TypeConverter(typeof(AudioContextConverter))]
    [Description("Creates an audio context using the specified device and listener properties.")]
    public class CreateAudioContext : Source<AudioContextManager>
    {
        /// <summary>
        /// Gets or sets the name of the audio device used for playback.
        /// </summary>
        [Category("Device")]
        [Description("The name of the audio device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the sample rate, in Hz, used by the audio device.
        /// Zero represents the driver default.
        /// </summary>
        [Category("Device")]
        [Description("The sample rate, in Hz, used by the audio device. Zero represents the driver default.")]
        public int SampleRate { get; set; }

        /// <summary>
        /// Gets or sets the refresh frequency, in Hz, used by the audio device.
        /// Zero represents the driver default.
        /// </summary>
        [Category("Device")]
        [Description("The refresh frequency, in Hz, used by the audio device. Zero represents the driver default.")]
        public int Refresh { get; set; }

        /// <summary>
        /// Gets or sets the location of the listener, in the world coordinate frame.
        /// </summary>
        [Category("Listener")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The location of the listener, in the world coordinate frame.")]
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the velocity of the listener, in the world coordinate frame.
        /// </summary>
        [Category("Listener")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The velocity of the listener, in the world coordinate frame.")]
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Gets or sets the direction vector of the listener, in the world coordinate frame.
        /// </summary>
        [Category("Listener")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The direction vector of the listener, in the world coordinate frame.")]
        public Vector3 Direction { get; set; } = -Vector3.UnitZ;

        /// <summary>
        /// Gets or sets the up vector of the listener, in the world coordinate frame.
        /// </summary>
        [Category("Listener")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The up vector of the listener, in the world coordinate frame.")]
        public Vector3 Up { get; set; } = Vector3.UnitY;

        /// <summary>
        /// Gets or sets the amount of amplification applied to the listener.
        /// Each multiplication by 2 increases gain by +6dB.
        /// </summary>
        [Category("Listener")]
        [Description("The amount of amplification applied to the listener. Each multiplication by 2 increases gain by +6dB.")]
        public float Gain { get; set; } = 1;

        /// <summary>
        /// Generates an observable sequence that contains the audio context manager object.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="AudioContextManager"/>
        /// class which will manage the lifetime of the audio context.
        /// </returns>
        public override IObservable<AudioContextManager> Generate()
        {
            return Observable.Using(
                () => AudioManager.ReserveContext(DeviceName, SampleRate, Refresh),
                resource =>
                {
                    var up = Up;
                    var at = Direction;
                    var position = Position;
                    var velocity = Velocity;
                    AL.Listener(ALListener3f.Position, ref position);
                    AL.Listener(ALListener3f.Velocity, ref velocity);
                    AL.Listener(ALListenerfv.Orientation, ref at, ref up);
                    AL.Listener(ALListenerf.Gain, Gain);
                    return Observable.Return(resource.Context)
                                     .Concat(Observable.Never(resource.Context));
                });
        }

        class AudioContextConverter : ExpandableObjectConverter
        {
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return base.GetProperties(context, value, attributes)
                           .Sort(new[] { nameof(DeviceName), nameof(Position), nameof(Velocity), nameof(Direction), nameof(Up) });
            }
        }
    }
}
