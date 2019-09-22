using Bonsai.Resources;
using OpenTK;
using OpenTK.Audio;
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
    [TypeConverter(typeof(AudioContextConverter))]
    [Description("Creates an audio context using the specified device and listener properties.")]
    public class CreateAudioContext : Source<AudioContext>
    {
        public CreateAudioContext()
        {
            Up = Vector3.UnitY;
            Direction = -Vector3.UnitZ;
            Gain = 1;
        }

        [Category("Device")]
        [Description("The name of the output device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName { get; set; }

        [Category("Device")]
        [Description("The sample rate, in Hz, used by the output device. Zero represents the driver default.")]
        public int SampleRate { get; set; }

        [Category("Device")]
        [Description("The refresh frequency, in Hz, used by the output device. Zero represents the driver default.")]
        public int Refresh { get; set; }

        [Category("Listener")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The location of the listener, in the world coordinate frame.")]
        public Vector3 Position { get; set; }

        [Category("Listener")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The velocity of the listener, in the world coordinate frame.")]
        public Vector3 Velocity { get; set; }

        [Category("Listener")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The direction vector of the listener, in the world coordinate frame.")]
        public Vector3 Direction { get; set; }

        [Category("Listener")]
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The up vector of the listener, in the world coordinate frame.")]
        public Vector3 Up { get; set; }

        [Category("Listener")]
        [Description("The amount of amplification applied to the listener. Each multiplication by 2 increases gain by +6dB.")]
        public float Gain { get; set; }

        public override IObservable<AudioContext> Generate()
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
                           .Sort(new[] { "DeviceName", "Position", "Velocity", "Direction", "Up" });
            }
        }
    }
}
