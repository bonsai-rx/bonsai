using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public abstract class WorkflowElement
    {
        public event EventHandler<LoadUnloadEventArgs> Loaded;

        public event EventHandler<LoadUnloadEventArgs> Unloaded;

        public virtual void Load(WorkflowContext context)
        {
            OnLoaded(new LoadUnloadEventArgs(context));
        }

        public virtual void Unload(WorkflowContext context)
        {
            OnUnloaded(new LoadUnloadEventArgs(context));
        }

        protected virtual void OnLoaded(LoadUnloadEventArgs e)
        {
            var handler = Loaded;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnUnloaded(LoadUnloadEventArgs e)
        {
            var handler = Unloaded;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
