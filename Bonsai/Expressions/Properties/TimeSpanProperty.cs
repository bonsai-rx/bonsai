using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [DisplayName("TimeSpan")]
    public class TimeSpanProperty : WorkflowProperty
    {
        TimeSpan value;
        event Action<TimeSpan> ValueChanged;

        [XmlIgnore]
        public TimeSpan Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnValueChanged(value);
            }
        }

        [Browsable(false)]
        [XmlElement("Value")]
        public string ValueXml
        {
            get { return XmlConvert.ToString(Value); }
            set { Value = XmlConvert.ToTimeSpan(value); }
        }

        void OnValueChanged(TimeSpan value)
        {
            var handler = ValueChanged;
            if (handler != null)
            {
                handler(value);
            }
        }

        public virtual IObservable<TimeSpan> Generate()
        {
            return Observable
                .Return(value)
                .Concat(Observable.FromEvent<TimeSpan>(
                    handler => ValueChanged += handler,
                    handler => ValueChanged -= handler));
        }
    }
}
