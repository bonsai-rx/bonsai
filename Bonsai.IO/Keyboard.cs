using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    public class Keyboard : Source<Keys>
    {
        IDisposable running;

        protected override void Start()
        {
            var mainForm = Application.OpenForms.Cast<Form>().FirstOrDefault();
            if (mainForm != null)
            {
                mainForm.KeyPreview = true;
                var keyDown = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    handler => mainForm.KeyDown += handler,
                    handler => mainForm.KeyDown -= handler)
                    .Select(evt => evt.EventArgs);

                var onNext = from key in keyDown
                             select key.KeyData;

                running = onNext.Subscribe(val => Subject.OnNext(val));
            }
        }

        protected override void Stop()
        {
            if (running != null)
            {
                running.Dispose();
                running = null;
            }
        }
    }
}
