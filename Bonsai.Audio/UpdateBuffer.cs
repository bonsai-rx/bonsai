using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    public class UpdateBuffer : Sink<Mat>
    {
        public UpdateBuffer()
        {
            SampleRate = 44100;
        }

        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        [Description("The name of the output device used for playback.")]
        public string DeviceName { get; set; }

        [TypeConverter(typeof(BufferNameConverter))]
        [Description("The name of the buffer to update.")]
        public string BufferName { get; set; }

        [Description("The sample rate, in Hz, used to playback the buffer samples.")]
        public int SampleRate { get; set; }

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
