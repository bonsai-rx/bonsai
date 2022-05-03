using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that delays subscription to the observable sequence
    /// by the specified time interval, using the render loop scheduler.
    /// </summary>
    [Description("Delays subscription to the observable sequence by the specified time interval, using the render loop scheduler.")]
    public class DelaySubscription : Combinator
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        /// <summary>
        /// Gets or sets the time interval by which to delay subscription to the
        /// sequence.
        /// </summary>
        [XmlIgnore]
        [Description("The time interval by which to delay subscription to the sequence.")]
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
        /// Delays subscription to an observable sequence by the specified time
        /// interval, using the render loop scheduler.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The source sequence to delay subscription for.
        /// </param>
        /// <returns>
        /// The time-shifted sequence, where subscription is delayed by the
        /// specified time interval, using the render loop scheduler.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var dueTime = DueTime.TotalSeconds;
            var elapsedTime = updateFrame.Generate().Scan(0.0, (elapsed, evt) => elapsed + evt.TimeStep.ElapsedTime);
            return elapsedTime.FirstAsync(elapsed => elapsed > dueTime).SelectMany(x => source);
        }
    }
}
