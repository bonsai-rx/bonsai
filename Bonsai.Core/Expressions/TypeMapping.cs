using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class TypeMapping
    {
        internal abstract Type TargetType { get; }
    }

    [XmlType(Namespace = Constants.XmlNamespace)]
    public class TypeMapping<T> : TypeMapping
    {
        internal override Type TargetType
        {
            get { return typeof(T); }
        }
    }
}
