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
    public class CreateSource : Source<AudioSource>
    {
        [Description("The name of the output device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName { get; set; }

        [TypeConverter("OpenCV.Net.NumericAggregateConverter, OpenCV.Net")]
        public Vector3 Direction { get; set; }

        [TypeConverter("OpenCV.Net.NumericAggregateConverter, OpenCV.Net")]
        public Vector3 Position { get; set; }

        [TypeConverter("OpenCV.Net.NumericAggregateConverter, OpenCV.Net")]
        public Vector3 Velocity { get; set; }

        public bool Looping { get; set; }

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
