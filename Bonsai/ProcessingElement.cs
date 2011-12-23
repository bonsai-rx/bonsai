using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai
{
    public abstract class ProcessingElement : Source
    {
        readonly OutputObservable<Exception> error = new OutputObservable<Exception>();

        [XmlIgnore]
        public bool Running { get; private set; }

        public event EventHandler RunningChanged;

        public IObservable<Exception> Error
        {
            get { return error; }
        }

        protected virtual void OnRunningChanged(EventArgs e)
        {
            var handler = RunningChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnError(Exception e)
        {
            error.OnNext(e);
        }

        public override void Start()
        {
            if (Running) return;

            Running = true;
            OnRunningChanged(EventArgs.Empty);
        }

        public override void Stop()
        {
            if (!Running) return;

            Running = false;
            OnRunningChanged(EventArgs.Empty);
        }
    }
}
