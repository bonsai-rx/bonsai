using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that generates an observable sequence that
    /// periodically produces a value after the specified initial relative
    /// due time has elapsed, using the render loop timing.
    /// </summary>
    [Description("Generates an observable sequence that periodically produces a value after the specified initial relative due time has elapsed, using the render loop timing.")]
    public class Timer : Source<long>
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        /// <summary>
        /// Gets or sets the relative time at which to produce the first value.
        /// If this value is less than or equal to zero, the timer will fire as
        /// soon as possible.
        /// </summary>
        [XmlIgnore]
        [Description("The relative time at which to produce the first value. If this value is less than or equal to zero, the timer will fire as soon as possible.")]
        public TimeSpan DueTime { get; set; }

        /// <summary>
        /// Gets or sets the period to produce subsequent values. If this value
        /// is undefined or equal to zero the timer will only fire once.
        /// </summary>
        [XmlIgnore]
        [Description("The period to produce subsequent values. If this value is equal to zero the timer will recur as fast as possible.")]
        public TimeSpan? Period { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the due time for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(DueTime))]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Gets or sets an XML representation of the period for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Period))]
        public string PeriodXml
        {
            get { return Period.HasValue ? XmlConvert.ToString(Period.Value) : null; }
            set { Period = string.IsNullOrEmpty(value) ? null : (TimeSpan?)XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Generates an observable sequence that periodically produces a value
        /// after the specified initial relative due time has elapsed, using the
        /// render loop timing.
        /// </summary>
        /// <returns>
        /// An observable sequence of integer values counting how many times the
        /// timer has fired.
        /// </returns>
        public override IObservable<long> Generate()
        {
            return Generate(Observable.Concat(
                Observable.Return(0.0),
                updateFrame.Generate().Select(evt => evt.TimeStep.ElapsedTime)));
        }

        /// <summary>
        /// Generates an observable sequence that periodically produces a value
        /// after the specified initial relative due time has elapsed, using the
        /// timing from the specified sequence of frame events.
        /// </summary>
        /// <param name="source">
        /// The sequence of frame events controlling the timing of the timer.
        /// </param>
        /// <returns>
        /// An observable sequence of integer values counting how many times the
        /// timer has fired.
        /// </returns>
        public IObservable<long> Generate(IObservable<FrameEvent> source)
        {
            return Generate(source.Select(evt => evt.TimeStep.ElapsedTime));
        }

        /// <summary>
        /// Generates an observable sequence that periodically produces a value
        /// after the specified initial relative due time has elapsed, using the
        /// timing from the specified sequence of time steps.
        /// </summary>
        /// <param name="source">
        /// The sequence of time steps controlling the timing of the timer.
        /// </param>
        /// <returns>
        /// An observable sequence of integer values counting how many times the
        /// timer has fired.
        /// </returns>
        public IObservable<long> Generate(IObservable<TimeStep> source)
        {
            return Generate(source.Select(timeStep => timeStep.ElapsedTime));
        }

        /// <summary>
        /// Generates an observable sequence that periodically produces a value
        /// after the specified initial relative due time has elapsed, using the
        /// timing from the specified sequence of time steps in seconds.
        /// </summary>
        /// <param name="source">
        /// The sequence of time steps, in seconds, controlling the timing of
        /// the timer.
        /// </param>
        /// <returns>
        /// An observable sequence of integer values counting how many times the
        /// timer has fired.
        /// </returns>
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
