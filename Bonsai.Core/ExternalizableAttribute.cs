using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <b>true</b> if the property can be explicitly externalized on a workflow editor;
        /// otherwise, <b>false</b>. The default is <b>true</b>.
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
