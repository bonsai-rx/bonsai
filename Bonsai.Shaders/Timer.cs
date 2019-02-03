using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
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
            return Generate(updateFrame.Generate());
        }

        public IObservable<long> Generate(IObservable<FrameEvent> source)
        {
            return Generate(source.Select(evt => evt.TimeStep.ElapsedTime));
        }

        public IObservable<long> Generate(IObservable<TimeStep> source)
        {
            return Generate(source.Select(timeStep => timeStep.ElapsedTime));
        }

        public IObservable<long> Generate(IObservable<double> source)
        {
            var dueTime = DueTime.TotalSeconds;
            var period = Period.GetValueOrDefault().TotalSeconds;
            return Observable.Create<long>(observer =>
            {
                var counter = 0L;
                var elapsedTime = 0.0;
                var timeObserver = Observer.Create<double>(
                    time =>
                    {
                        elapsedTime += time;
                        while (elapsedTime >= dueTime)
                        {
                            observer.OnNext(counter++);
                            elapsedTime -= dueTime;
                            if (period == 0)
                            {
                                observer.OnCompleted();
                                break;
                            }
                            else dueTime = period;
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(timeObserver);
            });
        }
    }
}
