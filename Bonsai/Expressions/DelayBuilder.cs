using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("Delay", Namespace = Constants.XmlNamespace)]
    public class DelayBuilder : CombinatorBuilder
    {
        static readonly MethodInfo delayMethod = typeof(Observable).GetMethods().First(m => m.Name == "Delay" &&
                                                                                       m.GetParameters().Length == 3 &&
                                                                                       m.GetParameters()[1].ParameterType == typeof(TimeSpan));

        [XmlIgnore]
        public TimeSpan DueTime { get; set; }

        [Browsable(false)]
        [XmlElement("DueTime")]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var dueTime = Expression.Constant(DueTime);
            var scheduler = Expression.Constant(HighResolutionScheduler.ThreadPool);
            return Expression.Call(delayMethod.MakeGenericMethod(sourceType), Source, dueTime, scheduler);
        }
    }
}
