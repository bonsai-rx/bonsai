using System.Xml.Serialization;
using Bonsai.Expressions;

namespace Bonsai.Core.Tests
{
    [XmlInclude(typeof(PolyType))]
    [XmlInclude(typeof(MorphicType))]
    [XmlInclude(typeof(ExtraTypes.ExtraType))]
    [XmlType(Namespace = XmlNamespace)]
    public abstract class PolymorphicType
    {
        public const string XmlNamespace = "clr-namespace:Bonsai.Core.Tests;assembly=Bonsai.Core.Tests";

        public PolymorphicType Extension { get; set; }
    }

    [XmlType(Namespace = XmlNamespace)]
    public class PolyType : PolymorphicType
    {
    }

    [XmlType(Namespace = XmlNamespace)]
    public class MorphicType : PolymorphicType
    {
    }

    namespace ExtraTypes
    {
        [XmlType(Namespace = "clr-namespace:Bonsai.Core.Tests.ExtraTypes;assembly=Bonsai.Core.Tests")]
        public class ExtraType : PolymorphicType
        {
        }
    }

    class DerivedValueProperty : WorkflowProperty<int>
    {
        [Range(0, 100)]
        public new int Value { get; set; }
    }

    class DerivedNewProperty : DerivedValueProperty
    {
        public int AnotherValue { get; set; }
    }
}
