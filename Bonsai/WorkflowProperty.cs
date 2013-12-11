using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    [Source]
    [WorkflowElementCategory(ElementCategory.Property)]
    public abstract class WorkflowProperty : INamedElement
    {
        internal WorkflowProperty()
        {
        }

        public string Name { get; set; }
    }

    [DefaultProperty("Value")]
    public class WorkflowProperty<TValue> : WorkflowProperty
    {
        TValue value;
        event Action<TValue> ValueChanged;

        public TValue Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnValueChanged(value);
            }
        }

        void OnValueChanged(TValue value)
        {
            var handler = ValueChanged;
            if (handler != null)
            {
                handler(value);
            }
        }

        public virtual IObservable<TValue> Generate()
        {
            return Observable
                .Return(value)
                .Concat(Observable.FromEvent<TValue>(
                    handler => ValueChanged += handler,
                    handler => ValueChanged -= handler));
        }
    }
}
