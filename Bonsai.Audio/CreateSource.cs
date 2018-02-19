using OpenTK;
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
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        [Description("The name of the output device used for playback.")]
        public string DeviceName { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The direction vector of the audio source.")]
        public Vector3 Direction { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current location of the audio source in three-dimensional space.")]
        public Vector3 Position { get; set; }

        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current velocity of the audio source in three-dimensional space.")]
        public Vector3 Velocity { get; set; }

        [Description("Indicates whether the audio source is looping.")]
        public bool Looping { get; set; }

        [Description("Indicates whether the audio source uses coordinates relative to the listener.")]
        public bool Relative { get; set; }

        public override IObservable<AudioSource> Generate()
        {
            return Observable.Using(
                () => AudioManager.ReserveContext(DeviceName),
                context =>
                {
                    var source = new AudioSource();
                    source.Direction = Direction;
                    source.Position = Position;
                    source.Velocity = Velocity;
                    source.Looping = Looping;
                    source.Relative = Relative;
                    return Observable.Return(source)
                                     .Concat(Observable.Never(source))
                                     .Finally(source.Dispose);
                });
        }
    }
}
