using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents the target type to be used by a mapping operator. Instances of this type are
    /// manipulated internally by <see cref="InputMappingBuilder"/>, <see cref="MemberSelectorBuilder"/>,
    /// and instances of <see cref="VisualizerMappingExpressionBuilder"/> to specify output and visualizer types.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [TypeConverter(typeof(TypeMappingConverter))]
    public abstract class TypeMapping
    {
        internal abstract Type TargetType { get; }
    }

    /// <summary>
    /// Represents the target type to be used by a mapping operator. Instances of this type are
    /// manipulated internally by <see cref="InputMappingBuilder"/>, <see cref="MemberSelectorBuilder"/>,
    /// and instances of <see cref="VisualizerMappingExpressionBuilder"/> to specify output and visualizer types.
    /// </summary>
    /// <typeparam name="T">The target type to be used by the mapping operator.</typeparam>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public sealed class TypeMapping<T> : TypeMapping
    {
        internal override Type TargetType
        {
            get { return typeof(T); }
        }
    }
}
