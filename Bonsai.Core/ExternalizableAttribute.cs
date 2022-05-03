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
        /// Specifies that a property can be explicitly externalized on a workflow editor.
        /// </summary>
        public static readonly ExternalizableAttribute Yes = new ExternalizableAttribute(true);

        /// <summary>
        /// Specifies that a property cannot be explicitly externalized on a workflow editor.
        /// </summary>
        public static readonly ExternalizableAttribute No = new ExternalizableAttribute(false);

        /// <summary>
        /// Specifies the default value for the <see cref="ExternalizableAttribute"/> which is set to
        /// allow a property to be explicitly externalized on a workflow editor.
        /// </summary>
        public static readonly ExternalizableAttribute Default = Yes;

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

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="ExternalizableAttribute"/>
        /// and the externalizable state equals the state of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            return obj is ExternalizableAttribute other && other.Externalizable == Externalizable;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Externalizable.GetHashCode();
        }

        /// <summary>
        /// When overridden in a derived class, indicates whether the value of this instance
        /// is the default value for the derived class.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this instance is the default attribute for the class; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool IsDefaultAttribute()
        {
            return Equals(Default);
        }
    }
}
