using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [Description("Delays the subscription of the observable sequence in the render loop by the specified relative time duration.")]
    public class DelaySubscription : Combinator
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        [XmlIgnore]
        [Description("The time interval by which to delay the subscription to the sequence.")]
        public TimeSpan DueTime { get; set; }

        [Browsable(false)]
        [XmlElement("DueTime")]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var dueTime = DueTime.TotalSeconds;
            var elapsedTime = updateFrame.Generate().Scan(0.0, (elapsed, evt) => elapsed + evt.TimeStep.ElapsedTime);
            return elapsedTime.FirstAsync(elapsed => elapsed > dueTime).SelectMany(x => source);
        }
    }
}
