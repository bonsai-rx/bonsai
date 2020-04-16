using System;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents the target type to be created from selected member variables. This type is manipulated internally
    /// by <see cref="InputMappingBuilder"/> and <see cref="MemberSelectorBuilder"/> to force a specific
    /// output type.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class TypeMapping
    {
        internal abstract Type TargetType { get; }
    }

    /// <summary>
    /// Represents the target type to be created from selected member variables. This type is manipulated internally
    /// by <see cref="InputMappingBuilder"/> and <see cref="MemberSelectorBuilder"/> to force a specific
    /// output type.
    /// </summary>
    /// <typeparam name="T">The target type to be created from selected member variables.</typeparam>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class TypeMapping<T> : TypeMapping
    {
        internal override Type TargetType
        {
            get { return typeof(T); }
        }
    }
}
