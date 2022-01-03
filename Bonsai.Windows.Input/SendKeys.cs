using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Windows.Input
{
    [DefaultProperty(nameof(Keys))]
    [Description("Sends one or more keystrokes to the active window as if typed at the keyboard.")]
    public class SendKeys : Sink
    {
        [Description("The string expression specifying the keystrokes to send.")]
        public string Keys { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Do(input => System.Windows.Forms.SendKeys.Send(Keys));
        }
    }
}
