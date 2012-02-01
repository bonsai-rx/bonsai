using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Expressions
{
    [XmlType("Throttle", Namespace = Constants.XmlNamespace)]
    public class ThrottleBuilder : CombinatorBuilder
    {
        static readonly MethodInfo throttleMethod = typeof(Observable).GetMethods().First(m => m.Name == "Throttle" &&
                                                                                          m.GetParameters().Length == 2);

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
            return Expression.Call(throttleMethod.MakeGenericMethod(sourceType), Source, dueTime);
        }
    }
}
