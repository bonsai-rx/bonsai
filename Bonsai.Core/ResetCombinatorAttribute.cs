using System;

namespace Bonsai
{
    /// <summary>
    /// Instructs the build process to reset non-serializable public properties
    /// marked with <see cref="System.Xml.Serialization.XmlIgnoreAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ResetCombinatorAttribute : Attribute
    {
    }
}
