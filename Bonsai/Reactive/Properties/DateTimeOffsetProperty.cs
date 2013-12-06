using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Properties
{
    [DisplayName("DateTimeOffset")]
    public class DateTimeOffsetProperty : WorkflowProperty
    {
        DateTimeOffset value;
        event Action<DateTimeOffset> ValueChanged;

        [XmlIgnore]
        public DateTimeOffset Value
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
            get { return Value.ToString("o"); }
            set { Value = DateTimeOffset.Parse(value); }
        }

        void OnValueChanged(DateTimeOffset value)
        {
            var handler = ValueChanged;
            if (handler != null)
            {
                handler(value);
            }
        }

        public virtual IObservable<DateTimeOffset> Generate()
        {
            return Observable
                .Return(value)
                .Concat(Observable.FromEvent<DateTimeOffset>(
                    handler => ValueChanged += handler,
                    handler => ValueChanged -= handler));
        }
    }
}
