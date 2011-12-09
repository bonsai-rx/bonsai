using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoAnalyzer
{
    public abstract class Source<T> : WorkflowElement
    {
        public event EventHandler<OutputChangedEventArgs<T>> OutputChanged;

        public abstract void Start();

        public abstract void Stop();

        protected virtual void OnOutput(OutputChangedEventArgs<T> e)
        {
            var handler = OutputChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }

    public class OutputChangedEventArgs<T> : EventArgs
    {
        public OutputChangedEventArgs(T output)
        {
            Output = output;
        }

        public T Output { get; private set; }
    }
}
