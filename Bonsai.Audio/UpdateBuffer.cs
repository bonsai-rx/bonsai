using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that updates the sample data of the specified audio buffer.
    /// </summary>
    [Description("Updates the sample data of the specified audio buffer.")]
    public class UpdateBuffer : Sink<Mat>
    {
        /// <summary>
        /// Gets or sets the name of the audio device used for playback.
        /// </summary>
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        [Description("The name of the audio device used for playback.")]
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets or sets the name of the buffer to update.
        /// </summary>
        [TypeConverter(typeof(BufferNameConverter))]
        [Description("The name of the buffer to update.")]
        public string BufferName { get; set; }

        /// <summary>
        /// Gets or sets the sample rate, in Hz, used to playback the buffer data.
        /// </summary>
        [Description("The sample rate, in Hz, used to playback the buffer data.")]
        public int SampleRate { get; set; } = 44100;

        /// <summary>
        /// Updates the data of the specified audio buffer using an observable sequence
        /// of buffered samples.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> objects representing the samples used to fill
        /// the buffer with audio data.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of filling the buffer with audio data.
        /// </returns>
        /// <remarks>
        /// This operator only subscribes to the <paramref name="source"/> sequence after
        /// initializing the audio context on the specified audio device.
        /// </remarks>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Using(
                () => AudioManager.ReserveContext(DeviceName),
                resource =>
                {
                    var buffer = resource.Context.ResourceManager.Load<Buffer>(BufferName);
                    return source.Do(input => BufferHelper.UpdateBuffer(buffer.Id, input, SampleRate));
                });
        }
    }
}
