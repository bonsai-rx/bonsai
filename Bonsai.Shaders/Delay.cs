﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [Description("Delays the notification of values in the render loop by the specified time interval.")]
    public class Delay : Combinator
    {
        static readonly UpdateFrame updateFrame = new UpdateFrame();

        [XmlIgnore]
        [Description("The time interval by which to delay the sequence.")]
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
