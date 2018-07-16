using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Gets the target refresh rate of the shader window.")]
    public class RefreshRate : Transform<INativeWindow, double>
    {
        public override IObservable<double> Process(IObservable<INativeWindow> source)
        {
            return source.Select(input => ((ShaderWindow)input).RefreshRate);
        }

        public IObservable<double> Process<TEventArgs>(IObservable<EventPattern<INativeWindow, TEventArgs>> source)
        {
            return source.Select(input => ((ShaderWindow)input.Sender).RefreshRate);
        }
    }
}
