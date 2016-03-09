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
    [Description("Generates an observable sequence that periodically produces a value in the render loop after the specified initial relative due time has elapsed.")]
    public class Timer : Source<long>
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        [XmlIgnore]
        [Description("The relative time at which to produce the first value. If this value is less than or equal to zero, the timer will fire as soon as possible.")]
        public TimeSpan DueTime { get; set; }

        [XmlIgnore]
        [Description("The optional period to produce subsequent values. If this value is equal to zero the timer will recur as fast as possible.")]
        public TimeSpan? Period { get; set; }

        [Browsable(false)]
        [XmlElement("DueTime")]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        [Browsable(false)]
        [XmlElement("Period")]
        public string PeriodXml
        {
            get { return Period.HasValue ? XmlConvert.ToString(Period.Value) : null; }
            set { Period = string.IsNullOrEmpty(value) ? null : (TimeSpan?)XmlConvert.ToTimeSpan(value); }
        }

        public override IObservable<long> Generate()
        {
            return updateFrame.Generate().Publish(update =>
            {
                var period = Period;
                var elapsedTime = update.Scan(TimeSpan.Zero, (elapsed, evt) => elapsed + TimeSpan.FromSeconds(evt.EventArgs.Time));
                var due = elapsedTime.FirstAsync(elapsed => elapsed > DueTime).Select(x => 0L);
                if (period.HasValue)
                {
                    return due.Concat(
                        elapsedTime
                        .FirstAsync(elapsed => elapsed > period.Value)
                        .Repeat()
                        .TakeUntil(update.TakeLast(1))
                        .Scan(0L, (counter, x) => counter + 1));
                }
                else return due;
            });
        }
    }
}
