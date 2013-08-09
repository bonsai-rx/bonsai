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
        public override IObservable<Keys> Generate()
        {
            var mainForm = Application.OpenForms.Cast<Form>().FirstOrDefault();
            if (mainForm != null)
            {
                mainForm.KeyPreview = true;
                return Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    handler => mainForm.KeyDown += handler,
                    handler => mainForm.KeyDown -= handler)
                    .Select(evt => evt.EventArgs.KeyData);
            }

            return Observable.Never<Keys>();
        }
    }
}
