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
    public class UpdateListener : Sink
    {
        static Vector3 Up = Vector3.UnitY;
        static Vector3 Forward = Vector3.UnitZ;

        [TypeConverter(typeof(NumericAggregateConverter))]
        public Vector3? Position { get; set; }

        [TypeConverter(typeof(NumericAggregateConverter))]
        public Vector3? Velocity { get; set; }

        [TypeConverter(typeof(NumericAggregateConverter))]
        public Quaternion? Orientation { get; set; } 

        public float? Gain { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Do(input =>
            {
                Vector3 position;
                if (TryGetValue(Position, out position))
                {
                    AL.Listener(ALListener3f.Position, ref position);
                }

                Vector3 velocity;
                if (TryGetValue(Velocity, out velocity))
                {
                    AL.Listener(ALListener3f.Velocity, ref velocity);
                }

                Quaternion orientation;
                if (TryGetValue(Orientation, out orientation))
                {
                    Vector3 at, up;
                    Vector3.Transform(ref Forward, ref orientation, out at);
                    Vector3.Transform(ref Up, ref orientation, out up);
                    AL.Listener(ALListenerfv.Orientation, ref at, ref up);
                }

                float gain;
                if (TryGetValue(Gain, out gain))
                {
                    AL.Listener(ALListenerf.Gain, gain);
                }
            });
        }

        static bool TryGetValue<T>(T? nullable, out T value) where T : struct
        {
            value = nullable.GetValueOrDefault();
            return nullable.HasValue;
        }
    }
}
