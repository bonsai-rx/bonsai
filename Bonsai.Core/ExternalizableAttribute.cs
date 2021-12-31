using System;

namespace Bonsai
{
    /// <summary>
    /// Specifies whether a property is allowed to be explicitly externalized on a
    /// workflow editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExternalizableAttribute : Attribute
    {
        /// <summary>
        /// Specifies the default value for the <see cref="ExternalizableAttribute"/> which is set to
        /// allow a property to be explicitly externalized on a workflow editor.
        /// </summary>
        public static readonly ExternalizableAttribute Default = new ExternalizableAttribute(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalizableAttribute"/> class.
        /// </summary>
        /// <param name="externalizable">
        /// <see langword="true"/> if the property can be explicitly externalized on a workflow editor;
        /// otherwise, <see langword="false"/>. The default is <see langword="true"/>.
        /// </param>
        public ExternalizableAttribute(bool externalizable)
        {
            Externalizable = externalizable;
        }

        /// <summary>
        /// Gets a value indicating whether a property is externalizable.
        /// </summary>
        public bool Externalizable { get; private set; }
    }
}
