﻿using OpenTK;
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
    public class UpdateSource : Sink<AudioSource>
    {
        [TypeConverter(typeof(NumericAggregateConverter))]
        public Vector3? Position { get; set; }

        [TypeConverter(typeof(NumericAggregateConverter))]
        public Vector3? Velocity { get; set; }

        [TypeConverter(typeof(NumericAggregateConverter))]
        public Vector3? Direction { get; set; }

        public override IObservable<AudioSource> Process(IObservable<AudioSource> source)
        {
            return source.Do(input =>
            {
                Vector3 position;
                if (TryGetValue(Position, out position))
                {
                    input.Position = position;
                }

                Vector3 velocity;
                if (TryGetValue(Velocity, out velocity))
                {
                    input.Velocity = velocity;
                }

                Vector3 direction;
                if (TryGetValue(Direction, out direction))
                {
                    input.Direction = direction;
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
