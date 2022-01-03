using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that updates the properties of the audio listener.
    /// </summary>
    [Description("Updates the properties of the audio listener.")]
    public class UpdateListener : Sink
    {
        static Vector3 Up = Vector3.UnitY;
        static Vector3 Forward = -Vector3.UnitZ;

        /// <summary>
        /// Gets or sets the current location of the listener, in the world coordinate frame.
        /// If this property is not set, the location of the audio listener will not be updated.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current location of the listener, in the world coordinate frame.")]
        public Vector3? Position { get; set; }

        /// <summary>
        /// Gets or sets the current velocity of the listener, in the world coordinate frame.
        /// If this property is not set, the velocity of the audio listener will not be updated.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current velocity of the listener, in the world coordinate frame.")]
        public Vector3? Velocity { get; set; }

        /// <summary>
        /// Gets or sets the current orientation of the listener, in the world coordinate frame.
        /// If this property is not set, the orientation of the audio listener will not be updated.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The current orientation of the listener, in the world coordinate frame.")]
        public Quaternion? Orientation { get; set; }

        /// <summary>
        /// Gets or sets the amount of amplification applied to the listener. Each multiplication by 2 increases gain by +6dB.
        /// If this property is not set, the gain of the audio listener will not be updated.
        /// </summary>
        [Description("The amount of amplification applied to the listener. Each multiplication by 2 increases gain by +6dB.")]
        public float? Gain { get; set; }

        /// <summary>
        /// Updates the properties of the audio listener whenever the source sequence
        /// emits a notification.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to trigger the update of
        /// the audio listener.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of updating the properties of the
        /// audio listener whenever the sequence emits a notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Do(input =>
            {
                if (TryGetValue(Position, out Vector3 position))
                {
                    AL.Listener(ALListener3f.Position, ref position);
                }

                if (TryGetValue(Velocity, out Vector3 velocity))
                {
                    AL.Listener(ALListener3f.Velocity, ref velocity);
                }

                if (TryGetValue(Orientation, out Quaternion orientation))
                {
                    Vector3 at, up;
                    Vector3.Transform(ref Forward, ref orientation, out at);
                    Vector3.Transform(ref Up, ref orientation, out up);
                    AL.Listener(ALListenerfv.Orientation, ref at, ref up);
                }

                if (TryGetValue(Gain, out float gain))
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
