using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that delays the notification of values in the
    /// sequence by the specified time interval, using the render loop scheduler.
    /// </summary>
    [Description("Delays the notification of values in the sequence by the specified time interval, using the render loop scheduler.")]
    public class Delay : Combinator
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        /// <summary>
        /// Gets or sets the time interval by which to delay the sequence.
        /// </summary>
        [XmlIgnore]
        [Description("The time interval by which to delay the sequence.")]
        public TimeSpan DueTime { get; set; }

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
        /// Delays the notification of values in an observable sequence by the
        /// specified time interval, using the render loop scheduler.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the source sequence.
        /// </typeparam>
        /// <param name="source">
        /// The source sequence to delay notifications for.
        /// </param>
        /// <returns>
        /// The time-shifted sequence, where all notifications will be raised
        /// in the render loop scheduler.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var dueTime = DueTime.TotalSeconds;
            if (dueTime == 0) return source;
            else return updateFrame.Generate().Publish(update =>
            {
                var elapsedTime = update.Scan(0.0, (elapsed, evt) => elapsed + evt.TimeStep.ElapsedTime);
                var due = elapsedTime.FirstAsync(elapsed => elapsed > dueTime);
                return source.SelectMany(input => due.Select(x => input));
            });
        }
    }
}
