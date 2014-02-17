using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions.Properties
{
    /// <summary>
    /// Represents a property that has been externalized from a workflow element.
    /// </summary>
    public interface IExternalizedProperty : INamedElement
    {
        /// <summary>
        /// Gets the name of the externalized class member.
        /// </summary>
        string MemberName { get; }

        /// <summary>
        /// Gets the type of the workflow element to which the externalized member is bound to.
        /// </summary>
        Type ElementType { get; }
    }
}
