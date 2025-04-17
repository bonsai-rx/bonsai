using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents the target type to be used by a mapping operator. Instances of this type are
    /// manipulated internally by <see cref="InputMappingBuilder"/>, <see cref="MemberSelectorBuilder"/>,
    /// <see cref="VisualizerMappingBuilder"/> and other instances of <see cref="IVisualizerMappingBuilder"/>
    /// to specify output and visualizer types.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [TypeConverter(typeof(TypeMappingConverter))]
    public abstract class TypeMapping
    {
        internal abstract Type TargetType { get; }

        /// <summary>
        /// Converts the <see cref="TypeMapping"/> object to a <see cref="Type"/> object.
        /// </summary>
        /// <param name="mapping">A <see cref="TypeMapping"/> object.</param>
        /// <returns>
        /// The target type referenced by the <see cref="TypeMapping"/> object.
        /// </returns>
        public static explicit operator Type(TypeMapping mapping) => mapping?.TargetType;
    }

    /// <summary>
    /// Represents the target type to be used by a mapping operator. Instances of this type are
    /// manipulated internally by <see cref="InputMappingBuilder"/>, <see cref="MemberSelectorBuilder"/>,
    /// <see cref="VisualizerMappingBuilder"/> and other instances of <see cref="IVisualizerMappingBuilder"/>
    /// to specify output and visualizer types.
    /// </summary>
    /// <typeparam name="T">The target type to be used by the mapping operator.</typeparam>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class TypeMapping<T> : TypeMapping
    {
        internal override Type TargetType
        {
            get { return typeof(T); }
        }
    }
}
