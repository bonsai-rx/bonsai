using Bonsai.Resources;
using OpenTK.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    [Description("Creates an audio context using the specified device, sample rate, and refresh frequency.")]
    public class CreateAudioContext : Source<AudioContext>
    {
        [Description("The name of the output device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName { get; set; }

        [Description("The sample rate, in Hz, used by the output device. Zero represents the driver default.")]
        public int SampleRate { get; set; }

        [Description("The refresh frequency, in Hz, used by the output device. Zero represents the driver default.")]
        public int Refresh { get; set; }

        public override IObservable<AudioContext> Generate()
        {
            return Observable.Using(
                () => AudioManager.ReserveContext(DeviceName, SampleRate, Refresh),
                resource => Observable.Return(resource.Context).Concat(Observable.Never(resource.Context)));
        }
    }
}
